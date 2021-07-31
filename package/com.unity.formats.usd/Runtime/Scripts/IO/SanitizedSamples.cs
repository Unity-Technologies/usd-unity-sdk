using System;
using System.Collections.Generic;
using pxr;
using USD.NET;
using USD.NET.Unity;
using UnityEngine;

namespace Unity.Formats.USD
{
    public interface ISanitizable
    {
        /// <summary>
        /// Sanitize the USD data held by this sample to your target specifications according to the import options.
        /// </summary>
        public void Sanitize(Scene scene, SceneImportOptions importOptions);
    }

    public class SanitizedXformSample : XformSample, ISanitizable
    {
        public void Sanitize(Scene scene, SceneImportOptions importOptions)
        {
            if (importOptions.changeHandedness != BasisTransformation.FastWithNegativeScale)
                ConvertTransform();
        }
    }

    public class SanitizedMeshSample : MeshSample, ISanitizable
    {
        int[] _originalFaceVertexCounts;
        [NonSerialized] public int[] originalFaceVertexIndices;
        [NonSerialized] public bool meshesUnrolled;

        /// <summary>
        /// After triangulation face ids are no longer correct. This maps old face ids to the new triangulated face ids.
        /// </summary>
        [NonSerialized] public List<List<int>> faceMapping;

        /// <summary>
        /// To unweld vertex attributes after the fact (skin weights, joint indices, ...) we need to store the face
        /// vertex indices post triangulation but before unweld.
        /// </summary>
        [NonSerialized] public int[] triangulatedFaceVertexIndices;


        /// <summary>
        /// Sanitize Mesh data for Unity:
        ///     * change basis
        ///     * triangulate
        ///     * unweld vertices and attributes/primvar if necessary
        /// </summary>
        public void Sanitize(Scene scene, SceneImportOptions importOptions)
        {
            // Start with the xform
            if (importOptions.changeHandedness != BasisTransformation.FastWithNegativeScale)
                ConvertTransform();

            var santizePrimvars = importOptions.ShouldBindMaterials ||
                scene.IsPopulatingAccessMask || scene.AccessMask != null;                        //this is true when reading from the timeline

            var changeHandedness = true;
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

            if (normals == null)
            {
                normals = new Vector3[points.Length];
                ComputeNormals(points, faceVertexIndices, ref normals);
                Flatten(ref normals, faceVertexIndices);
            }
            else
            {
                ConvertInterpolation(ref normals, faceVertexIndices, unwindVertices);
            }

            ConvertInterpolation(ref tangents, faceVertexIndices, unwindVertices);

            ConvertInterpolation(ref colors.value, faceVertexIndices, unwindVertices);
            // colors.SetValue(colorValues);

            if (santizePrimvars)
            {
                UnweldUVs(st, unwindVertices);
                UnweldUVs(uv, unwindVertices);
                UnweldUVs(uv2, unwindVertices);
                UnweldUVs(uv3, unwindVertices);
                UnweldUVs(uv4, unwindVertices);
            }

            // Convert points last, as points count is used to guess the interpolation of other attributes
            // also update the vertex mapping
            Flatten(ref points, faceVertexIndices);

            // Now that all attributes and primvar are converted to facevarying, update the faceIndices
            for (var i = 0; i < faceVertexIndices.Length; i++)
                faceVertexIndices[i] = i;


            meshesUnrolled = true;
        }

        /// <summary>
        /// Triangulate the usd mesh, compute the face mapping to remap face sets and store original face vertex counts
        /// and indices to unroll attributes and primvars.
        /// </summary>
        /// <param name="changeHandedness"></param>
        public void Triangulate(bool changeHandedness)
        {
            _originalFaceVertexCounts = faceVertexCounts;
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

        bool ShouldUnweldVertices(bool bindMaterials)
        {
            // If any primvar is face varying (1 value per vertex) or uniform (1 value per face), all  primvars + mesh attributes will have to be unrolled
            // Assumes points attribute is of vertex/varying scope.
            // TODO: expose interpolation for standard mesh attributes (points, normals, colors, ...)
            return normals != null && (normals.Length == _originalFaceVertexCounts.Length || normals.Length > points.Length) ||
                colors != null && (colors.GetInterpolationToken() == UsdGeomTokens.uniform || colors.GetInterpolationToken() == UsdGeomTokens.faceVarying) ||
                tangents != null &&
                (tangents.Length == _originalFaceVertexCounts.Length || tangents.Length > points.Length) ||
                bindMaterials &&
                (st.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv2.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv3.GetInterpolationToken() == UsdGeomTokens.faceVarying ||
                    uv4.GetInterpolationToken() == UsdGeomTokens.faceVarying);
        }

        public static void Flatten<T>(ref T[] values, int[] indices)
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
                ConvertInterpolation(ref value, faceVertexIndices, changeHandedness, primvar.GetInterpolationToken());
                primvar.SetValue(value);
                return;
            }

            if (primvar.value.GetType() == typeof(Vector3[]))
            {
                var value = primvar.value as Vector3[];
                ConvertInterpolation(ref value, faceVertexIndices, changeHandedness, primvar.GetInterpolationToken());
                primvar.SetValue(value);
                return;
            }

            if (primvar.value.GetType() == typeof(Vector4[]))
            {
                var value = primvar.value as Vector4[];
                ConvertInterpolation(ref value, faceVertexIndices, changeHandedness, primvar.GetInterpolationToken());
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

        public static void TriangulateAttributes<T>(ref T[] values, int[] faceVertexCount, bool changeHandedness)
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

        void ConvertInterpolation<T>(ref T[] values, int[] vertexIndices, bool changeHandedness = false, TfToken interpolation = null)
        {
            if (values == null)
                return;

            if (interpolation == null)
                interpolation = GuessInterpolation(values.Length);

            if (interpolation == UsdGeomTokens.constant)
            {
                // Ignore, it's supported by the importer
            }
            if (interpolation == UsdGeomTokens.uniform) // interpolation is uniform (1 per face)
            {
                UniformToFaceVarying(ref values, vertexIndices);
            }
            else if (interpolation == UsdGeomTokens.vertex || interpolation == UsdGeomTokens.varying)
            {
                Flatten(ref values, faceVertexIndices);
            }
            else if (interpolation == UsdGeomTokens.faceVarying)
            {
                TriangulateAttributes(ref values, _originalFaceVertexCounts, changeHandedness);
            }
        }

        public TfToken GuessInterpolation(int count)
        {
            if (count == 1)
            {
                return UsdGeomTokens.constant;
            }
            if (count == _originalFaceVertexCounts.Length)
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
        /// <param name="values"></param>
        /// <param name="indices"></param>
        /// <typeparam name="T"></typeparam>
        public void UniformToFaceVarying<T>(ref T[] values, int[] indices)
        {
            var newValues = new T[indices.Length];
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

        public bool ShouldUnwindVertices(bool changeHandedness)
        {
            return changeHandedness && orientation == Orientation.RightHanded ||
                !changeHandedness && orientation == Orientation.LeftHanded;
        }
    }
}
