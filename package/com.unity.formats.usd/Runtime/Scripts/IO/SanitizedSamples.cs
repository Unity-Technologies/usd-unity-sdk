using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using pxr;
using USD.NET;
using USD.NET.Unity;
using UnityEngine;

[assembly: InternalsVisibleToAttribute("Unity.Formats.USD.Tests")]
namespace Unity.Formats.USD
{
    public interface ISanitizable
    {
        /// <summary>
        /// Sanitize the USD data held by this sample to your target specifications according to the import options.
        /// </summary>
        void Sanitize(Scene scene, SceneImportOptions importOptions);
    }

    /// <summary>
    /// A sanitizable version of an XFormSample. Enable automatic change of handedness.
    /// </summary>
    public class SanitizedXformSample : XformSample, ISanitizable
    {
        public void Sanitize(Scene scene, SceneImportOptions importOptions)
        {
            if (importOptions.changeHandedness != BasisTransformation.FastWithNegativeScale)
                ConvertTransform();
        }
    }

    /// <summary>
    /// A sanitizable version of an CameraSample. Enable automatic change of handedness.
    /// </summary>
    public class SanitizedCameraSample : CameraSample, ISanitizable
    {
        public void Sanitize(Scene scene, SceneImportOptions importOptions)
        {
            if (importOptions.changeHandedness != BasisTransformation.FastWithNegativeScale)
                ConvertTransform();
        }
    }

    /// <summary>
    /// A sanitizable version of a MeshSample. Enable automatic triangulation/handedness change/attribute interpolation conversion.
    /// </summary>
    public class SanitizedMeshSample : MeshSample, ISanitizable
    {
        /// <summary>
        /// Store the face vertex counts straight from USD deserialization
        /// </summary>
        int[] originalFaceVertexCounts;

        /// <summary>
        /// Store the face vertex indices straight from USD deserialization
        /// </summary>
        int[] originalFaceVertexIndices;

        /// <summary>
        /// True when the mesh arrays have been converted to facevarying
        /// </summary>
        internal bool arePrimvarsFaceVarying;

        /// <summary>
        /// After triangulation face ids are no longer correct. This maps old face ids to the new triangulated face ids.
        /// </summary>
        internal List<List<int>> faceMapping;

        /// <summary>
        /// To unweld vertex attributes after the fact (skin weights, joint indices, ...) we need to store the face
        /// vertex indices post triangulation but before unweld.
        /// </summary>
        internal int[] triangulatedFaceVertexIndices;


        /// <summary>
        /// Sanitize Mesh data for Unity:
        ///     * change basis
        ///     * triangulate
        ///     * convert vertices and attributes/primvar to facevarying if necessary
        /// </summary>
        public void Sanitize(Scene scene, SceneImportOptions importOptions)
        {
            var changeHandedness = importOptions.changeHandedness == BasisTransformation.SlowAndSafe ||
                importOptions.changeHandedness == BasisTransformation.SlowAndSafeAsFBX;
            // Start with the xform
            if (changeHandedness)
                ConvertTransform();

            var santizePrimvars = importOptions.ShouldBindMaterials ||
                scene.IsPopulatingAccessMask || scene.AccessMask != null;                        //this is true when reading from the timeline

            var unwindVertices = ShouldUnwindVertices(changeHandedness);
            if (points == null)
                return;

            // Points
            // originalPointCount = points.Length;
            if (changeHandedness)
            {
                for (var i = 0; i < points.Length; i++)
                    points[i] = UnityTypeConverter.ChangeBasis(points[i]);
            }

            // Topology
            Triangulate(unwindVertices);

            // Normals
            if (normals != null && changeHandedness)
            {
                for (var i = 0; i < normals.Length; i++)
                {
                    normals[i] = UnityTypeConverter.ChangeBasis(normals[i]);
                }
            }

            // Bounds
            if (changeHandedness)
                extent.center = UnityTypeConverter.ChangeBasis(extent.center);

            // Tangents
            // TODO: we should check interpolation
            if (changeHandedness && tangents != null)
            {
                var newTangents = new Vector4[tangents.Length];
                for (var i = 0; i < tangents.Length; i++)
                {
                    var w = tangents[i].w;
                    var t = UnityTypeConverter.ChangeBasis(tangents[i]);
                    newTangents[i] = new Vector4(t.x, t.y, t.z, w);
                }
            }

            // Colors
            if (colors.value != null && colors.indices != null)
            {
                Flatten(ref colors.value, colors.indices);
            }

            // UVs
            if (santizePrimvars)
            {
                FlattenUVs(st);
                FlattenUVs(uv);
                FlattenUVs(uv2);
                FlattenUVs(uv3);
                FlattenUVs(uv4);
            }

            if (!ShouldUnweldVertices(santizePrimvars))
                return;

            // At that point we know that primvars are of different interpolation type.
            // For now we use the worst case scenario which is to unroll all the values to faceVarying interpolation.
            // TODO: A more efficient solution would be to detect the larger interpolation type used across all primvars
            // and unroll values up to that interpolation type.
            if (normals == null)
            {
                normals = new Vector3[points.Length];
                ComputeNormals(points, faceVertexIndices, ref normals);
                Flatten(ref normals, faceVertexIndices);
            }
            else
            {
                ConvertInterpolationToFaceVarying(ref normals, faceVertexIndices, unwindVertices);
            }

            ConvertInterpolationToFaceVarying(ref tangents, faceVertexIndices, unwindVertices);

            ConvertInterpolationToFaceVarying(ref colors.value, faceVertexIndices, unwindVertices);

            if (santizePrimvars)
            {
                UnweldUVs(st, unwindVertices);
                UnweldUVs(uv, unwindVertices);
                UnweldUVs(uv2, unwindVertices);
                UnweldUVs(uv3, unwindVertices);
                UnweldUVs(uv4, unwindVertices);
            }

            // Convert points last, as points count might be used to guess the interpolation of other attributes
            // also update the vertex mapping
            Flatten(ref points, faceVertexIndices);

            // Now that all attributes and primvar are converted to facevarying, update the faceIndices
            for (var i = 0; i < faceVertexIndices.Length; i++)
                faceVertexIndices[i] = i;


            arePrimvarsFaceVarying = true;
        }

        /// <summary>
        /// Triangulate the usd mesh, compute the face mapping to remap face sets and store original face vertex counts
        /// and indices to unroll attributes and primvars.
        /// </summary>
        /// <param name="changeHandedness"></param>
        internal void Triangulate(bool changeHandedness)
        {
            originalFaceVertexCounts = faceVertexCounts;
            originalFaceVertexIndices = faceVertexIndices;
            faceMapping = new List<List<int>>();

            var newIndices = new List<int>();
            var newCounts = new List<int>();

            var last = 0;
            var currentOffset = 0;
            for (var i = 0; i < faceVertexCounts.Length; i++)
            {
                faceMapping.Add(new List<int>());

                var next = last + 1;
                var t = 0;
                for (; t < faceVertexCounts[i] - 2; t++)
                {
                    newCounts.Add(3);
                    if (changeHandedness)
                    {
                        newIndices.Add(faceVertexIndices[next++]);
                        newIndices.Add(faceVertexIndices[last]);
                        newIndices.Add(faceVertexIndices[next]);
                    }
                    else
                    {
                        newIndices.Add(faceVertexIndices[last]);
                        newIndices.Add(faceVertexIndices[next++]);
                        newIndices.Add(faceVertexIndices[next]);
                    }
                    faceMapping[i].Add(currentOffset++);
                }
                last += faceVertexCounts[i];
            }

            faceVertexIndices = newIndices.ToArray();
            triangulatedFaceVertexIndices = newIndices.ToArray();
            faceVertexCounts = newCounts.ToArray();
        }

        internal bool ShouldUnweldVertices(bool bindMaterials)
        {
            // If any primvar is face varying (1 value per vertex) or uniform (1 value per face), all  primvars + mesh attributes will have to be converted to face varying
            // TODO: expose interpolation for standard mesh attributes (normals, tangents)
            return normals != null && (normals.Length == originalFaceVertexCounts.Length || normals.Length > points.Length) ||
                colors != null && (colors.GetInterpolationToken() == UsdGeomTokens.uniform || colors.GetInterpolationToken() == UsdGeomTokens.faceVarying) ||
                tangents != null &&
                (tangents.Length == originalFaceVertexCounts.Length || tangents.Length > points.Length) ||
                bindMaterials &&
                (st.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv2.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv3.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv4.GetInterpolationToken() == UsdGeomTokens.faceVarying);
        }

        internal static void Flatten<T>(ref T[] values, int[] indices)
        {
            if (values == null || indices == null)
                return;

            var newValues = new T[indices.Length];
            for (var i = 0; i < indices.Length; i++)
            {
                newValues[i] = values[indices[i]];
            }

            values = newValues;
        }

        void UnweldUVs(Primvar<object> primvar, bool changeHandedness)
        {
            if (primvar.value == null)
                return;

            if (primvar.GetInterpolationToken() == UsdGeomTokens.constant)
            {
                UsdIo.ArrayAllocator.Free(primvar.value.GetType(), (uint)(primvar.value as Array).GetLength(0),
                    primvar.value as Array);
                return;
            }

            // We can't use uv.GetValueType() as it return "typeof(T)" and so would return "object" in this case instead of
            // the actual type of value.
            if (primvar.value.GetType() == typeof(Vector2[]))
            {
                var value = primvar.value as Vector2[];
                ConvertInterpolationToFaceVarying(ref value, faceVertexIndices, changeHandedness, primvar.GetInterpolationToken());
                primvar.SetValue(value);
                return;
            }

            if (primvar.value.GetType() == typeof(Vector3[]))
            {
                var value = primvar.value as Vector3[];
                ConvertInterpolationToFaceVarying(ref value, faceVertexIndices, changeHandedness, primvar.GetInterpolationToken());
                primvar.SetValue(value);
                return;
            }

            if (primvar.value.GetType() == typeof(Vector4[]))
            {
                var value = primvar.value as Vector4[];
                ConvertInterpolationToFaceVarying(ref value, faceVertexIndices, changeHandedness, primvar.GetInterpolationToken());
                primvar.SetValue(value);
                return;
            }
        }

        static void FlattenUVs(Primvar<object> primvar)
        {
            if (primvar.value == null)
                return;

            // We can't use uv.GetValueType() as it return "typeof(T)" and so would return "object" in this case instead of
            // the actual type of value.
            if (primvar.value.GetType() == typeof(Vector2[]))
            {
                var value = primvar.value as Vector2[];
                Flatten(ref value, primvar.indices);
                primvar.SetValue(value);
                return;
            }

            if (primvar.value.GetType() == typeof(Vector3[]))
            {
                var value = primvar.value as Vector3[];
                Flatten(ref value, primvar.indices);
                primvar.SetValue(value);
                return;
            }

            if (primvar.value.GetType() == typeof(Vector4[]))
            {
                var value = primvar.value as Vector4[];
                Flatten(ref value, primvar.indices);
                primvar.SetValue(value);
                return;
            }
        }

        internal static void TriangulateAttributes<T>(ref T[] values, int[] faceVertexCount, bool changeHandedness)
        {
            var newValues = new List<T>();
            var last = 0;
            for (var i = 0; i < faceVertexCount.Length; i++)
            {
                var next = last + 1;
                for (var t = 0; t < faceVertexCount[i] - 2; t++)
                    if (changeHandedness)
                    {
                        newValues.Add(values[next++]);
                        newValues.Add(values[last]);
                        newValues.Add(values[next]);
                    }
                    else
                    {
                        newValues.Add(values[last]);
                        newValues.Add(values[next++]);
                        newValues.Add(values[next]);
                    }

                last += faceVertexCount[i];
            }

            values = newValues.ToArray();
        }

        /// <summary>
        /// Utility method to convert a given array of values to the equivalent faceVarying array (one value per vertex per face).
        /// </summary>
        /// <remarks> If the interpolation of the array to convert is not known, it will be guessed based on the length of the array. </remarks
        void ConvertInterpolationToFaceVarying<T>(ref T[] values, int[] vertexIndices, bool changeHandedness = false, TfToken interpolation = null)
        {
            if (values == null)
                return;

            if (interpolation == null)
                interpolation = GuessInterpolation(values.Length);

            if (interpolation == UsdGeomTokens.constant)
            {
                // Ignore, constant values are supported by the importer
            }
            if (interpolation == UsdGeomTokens.uniform) // 1 value per face
            {
                UniformToFaceVarying(ref values, vertexIndices.Length);
            }
            else if (interpolation == UsdGeomTokens.vertex || interpolation == UsdGeomTokens.varying) // 1 value per point
            {
                Flatten(ref values, faceVertexIndices);
            }
            else if (interpolation == UsdGeomTokens.faceVarying) // 1 value per vertex per triangle
            {
                TriangulateAttributes(ref values, originalFaceVertexCounts, changeHandedness);
            }
        }

        /// <summary>
        /// Returns the interpolation of an data array based on the number of elements
        /// </summary>
        /// <param name="count"> The number of elements in the array</param>
        /// <remarks>
        /// This is a fallback mechanism when the interpolation token is not available for a given attribute.
        /// It might also fail in the case of degenerate polygons or single face mesh (vertex & face-varying are undistinguishable)
        /// </remarks>
        internal TfToken GuessInterpolation(int count)
        {
            if (count == 1)
            {
                return UsdGeomTokens.constant;
            }
            if (count == originalFaceVertexCounts.Length)
            {
                return UsdGeomTokens.uniform;
            }
            if (count == points.Length)
            {
                return UsdGeomTokens.vertex;
            }
            if (count == originalFaceVertexIndices.Length)
            {
                return UsdGeomTokens.faceVarying;
            }

            return new TfToken();
        }

        /// <summary>
        /// Convert an array of data per face to an array of data per vertex per triangle.
        /// Assume the input array is not indexed.
        /// </summary>
        /// <param name="values"> The data to convert</param>
        /// <param name="vertexCount"> The number of mesh vertices</param>
        /// <typeparam name="T"></typeparam>
        internal void UniformToFaceVarying<T>(ref T[] values, int vertexCount)
        {
            var newValues = new T[vertexCount];
            for (var faceIdx = 0; faceIdx < values.Length; faceIdx++)
            {
                var newFaceIndices = faceMapping[faceIdx];
                var value = values[faceIdx];
                foreach (var newFaceIdx in newFaceIndices)
                {
                    newValues[newFaceIdx * 3] = value;
                    newValues[newFaceIdx * 3 + 1] = value;
                    newValues[newFaceIdx * 3 + 2] = value;
                }
            }

            values = newValues;
        }

        static void ComputeNormals(Vector3[] points, int[] faceVertexIndices, ref Vector3[] normals)
        {
            for (int faceIndex = 0; faceIndex < faceVertexIndices.Length / 3; faceIndex++)
            {
                int i0 = faceVertexIndices[faceIndex * 3];
                int i1 = faceVertexIndices[faceIndex * 3 + 1];
                int i2 = faceVertexIndices[faceIndex * 3 + 2];

                Vector3 e1 = points[i1];
                e1 -= points[i0];
                Vector3 e2 = points[i2];
                e2 -= points[i0];
                Vector3 n = Vector3.Cross(e1, e2);

                normals[i0] += n;
                normals[i1] += n;
                normals[i2] += n;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i].Normalize();
            }
        }

        internal bool ShouldUnwindVertices(bool changeHandedness)
        {
            return changeHandedness && orientation == Orientation.RightHanded ||
                !changeHandedness && orientation == Orientation.LeftHanded;
        }
    }
}
