using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using pxr;
using USD.NET;
using USD.NET.Unity;
using UnityEngine;
using System.Linq;

[assembly: InternalsVisibleToAttribute("Unity.Formats.USD.Tests.Runtime")]
namespace Unity.Formats.USD
{
    /// <summary>
    /// When a prim is sparsely animated only dynamic properties are read into the Sample.
    /// This interface's purpose is to enable holding the time-invariant data (static properties or computed data) to ensure proper deserialization.
    /// </summary>
    public interface IRestorable
    {
        /// <summary>
        /// Returns true if the internal data has been restored from cached data
        /// </summary>
        bool IsRestoredFromCachedData();

        /// <summary>
        /// Restore internal data from the cached data
        /// </summary>
        void FromCachedData(IRestorableData data);

        /// <summary>
        /// Return data from internal data
        /// </summary>
        IRestorableData ToCachedData();
    }

    /// <summary>
    /// This interface provides a Sanitization service for Samples that need to be sanitized to be consumed by Unity (i.e. meshes, xforms, cameras...)
    /// </summary>
    public interface ISanitizable
    {
        /// <summary>
        /// Sanitize the USD data held by this sample to your target specifications according to the import options.
        /// </summary>
        void Sanitize(Scene scene, SceneImportOptions importOptions);
    }

    public class MeshStaticPropertiesData : IRestorableData
    {
        /// <summary>
        /// Store the face vertex counts straight from USD deserialization
        /// </summary>
        internal int[] originalFaceVertexCounts;

        /// <summary>
        /// Store the face vertex indices straight from USD deserialization
        /// </summary>
        internal int[] originalFaceVertexIndices;

        /// <summary>
        /// True when the mesh arrays have been converted to facevarying
        /// </summary>
        internal bool arePrimvarsFaceVarying;

        /// <summary>
        /// Store the mesh orientation
        /// </summary>
        internal Orientation orientation;
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
    /// IRestorable allows static data to be stored once for multiple samples in the case of animated meshes.
    /// </summary>
    public class SanitizedMeshSample : MeshSample, ISanitizable, IRestorable
    {
        /// <summary>
        /// Store the face vertex counts straight from USD deserialization
        /// </summary>
        internal int[] originalFaceVertexCounts;

        /// <summary>
        /// Store the face vertex indices straight from USD deserialization
        /// </summary>
        internal int[] originalFaceVertexIndices;

        /// <summary>
        /// True when the mesh arrays have been converted to facevarying
        /// </summary>
        internal bool arePrimvarsFaceVarying;

        /// <summary>
        /// After triangulation face ids are no longer correct. This maps each old face id to the first of the new triangulated face ids.
        /// The new face IDs are sequential, so you can use originalFaceVertexCounts (minus 2) to work out the extent of the new face IDs.
        /// </summary>
        internal int[] faceMapping;

        /// <summary>
        /// To unweld vertex attributes after the fact (skin weights, joint indices, ...) we need to store the face
        /// vertex indices post triangulation but before unweld.
        /// </summary>
        internal int[] triangulatedFaceVertexIndices;

        bool isRestoredFromCachedData;

        public bool IsRestoredFromCachedData()
        {
            return isRestoredFromCachedData;
        }

        /// <summary>
        /// Restore internal data from a copy of the data held in Deserialization Context for animated meshes.
        /// </summary>
        public void FromCachedData(IRestorableData restorableData)
        {
            var staticPropertiesData = restorableData as MeshStaticPropertiesData;
            if (staticPropertiesData == null)
                return;

            arePrimvarsFaceVarying = staticPropertiesData.arePrimvarsFaceVarying;

            if (faceVertexCounts == null)
                faceVertexCounts = staticPropertiesData.originalFaceVertexCounts;

            if (faceVertexIndices == null)
                faceVertexIndices = staticPropertiesData.originalFaceVertexIndices;

            // Orientation can't be animated and defaults to RightHanded, so restore it in the case the mesh is leftHanded
            orientation = staticPropertiesData.orientation;

            isRestoredFromCachedData = true;
        }

        /// <summary>
        /// Create a copy of static data in a format that can be stored in a DeserializationContext.
        /// </summary>
        public IRestorableData ToCachedData()
        {
            var staticPropertiesData = new MeshStaticPropertiesData()
            {
                originalFaceVertexCounts = originalFaceVertexCounts,
                originalFaceVertexIndices = originalFaceVertexIndices,
                arePrimvarsFaceVarying = arePrimvarsFaceVarying,
                orientation = orientation
            };
            return staticPropertiesData;
        }

        public void BackupTopology()
        {
            originalFaceVertexCounts = new int[faceVertexCounts.Length];
            Buffer.BlockCopy(faceVertexCounts, 0, originalFaceVertexCounts, 0, Buffer.ByteLength(faceVertexCounts));
            originalFaceVertexIndices = new int[faceVertexIndices.Length];
            Buffer.BlockCopy(faceVertexIndices, 0, originalFaceVertexIndices, 0, Buffer.ByteLength(faceVertexIndices));
        }

        public bool IsTopologyBackedUp()
        {
            return originalFaceVertexCounts != null && originalFaceVertexIndices != null;
        }

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

            var sanitizePrimvars = (IsRestoredFromCachedData() && arePrimvarsFaceVarying) ||
                importOptions.ShouldBindMaterials ||
                scene.IsPopulatingAccessMask ||                                  // this is true when initializing prims from the timeline
                scene.AccessMask != null;                                        // this is true when reading from the timeline

            var unwindVertices = ShouldUnwindVertices(changeHandedness);

            // We only support animated primvars if the mesh itself is animated
            if (points == null)
                return;

            // Points
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
            if (tangents != null && tangents.value != null && changeHandedness)
            {
                var newTangents = new Vector4[tangents.Length];
                for (var i = 0; i < tangents.Length; i++)
                {
                    var w = tangents.value[i].w;
                    var t = UnityTypeConverter.ChangeBasis(tangents.value[i]);
                    newTangents[i] = new Vector4(t.x, t.y, t.z, w);
                }
                tangents.value = newTangents;
            }

            // Colors
            if (colors.value != null && colors.indices != null)
            {
                Flatten(ref colors.value, colors.indices);
            }

            // Arbitrary primvars
            if (sanitizePrimvars && ArbitraryPrimvars != null)
            {
                foreach (var primvar in ArbitraryPrimvars)
                {
                    FlattenPrimvar(primvar.Value);
                }
            }

            if (!ShouldUnweldVertices(sanitizePrimvars))
                return;

            // At that point we know that primvars are of different interpolation type.
            // For now we use the worst case scenario which is to unroll all the values to faceVarying interpolation.
            // TODO: A more efficient solution would be to detect the larger interpolation type used across all primvars
            // and unroll values up to that interpolation type.

            // Normals need to be computed before we unweld faces to avoid faceted meshes
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

            // TODO: I suppose we should also compute tangents to avoid broken tangents
            ConvertInterpolationToFaceVarying(ref tangents.value, faceVertexIndices, unwindVertices);

            ConvertInterpolationToFaceVarying(ref colors.value, faceVertexIndices, unwindVertices);

            if (sanitizePrimvars && ArbitraryPrimvars != null)
            {
                foreach (var primvar in ArbitraryPrimvars)
                {
                    UnweldPrimvar(primvar.Value, unwindVertices);
                }
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
            // Start by backing up the topology
            BackupTopology();
            faceMapping = new int[faceVertexCounts.Length];

            // count the length of newCounts to pre-allocate the array-
            // this extra loop is more performant than using a dynamically-sized List
            int newFaceCountsLength = 0;
            foreach (int faceVertexCount in faceVertexCounts)
            {
                newFaceCountsLength += faceVertexCount - 2;
            }

            // We only have tris, so 3 indices per face
            int newIndicesLength = 3 * newFaceCountsLength;

            var newCounts = new int[newFaceCountsLength];
            var newIndices = new int[newIndicesLength];

            int last = 0, next = 0, triCount = 0;
            int currentFaceOffset = 0, currentIndexOffset = 0;

            // Unity uses a left handed basis and clockwise winding, so if the source mesh does not, we have to convert
            if (changeHandedness)
            {
                for (var i = 0; i < faceVertexCounts.Length; i++)
                {
                    faceMapping[i] = currentFaceOffset;

                    next = last + 1;
                    triCount = faceVertexCounts[i] - 2;

                    for (int t = 0; t < triCount; t++)
                    {
                        newCounts[currentFaceOffset] = 3;

                        newIndices[currentIndexOffset++] = faceVertexIndices[next++];
                        newIndices[currentIndexOffset++] = faceVertexIndices[last];
                        newIndices[currentIndexOffset++] = faceVertexIndices[next];
                    }
                    currentFaceOffset += triCount;
                    last += faceVertexCounts[i];
                }
            }
            else
            {
                for (var i = 0; i < faceVertexCounts.Length; i++)
                {
                    faceMapping[i] = currentFaceOffset;

                    next = last + 1;
                    triCount = faceVertexCounts[i] - 2;

                    for (int t = 0; t < triCount; t++)
                    {
                        newCounts[currentFaceOffset] = 3;

                        newIndices[currentIndexOffset++] = faceVertexIndices[last];
                        newIndices[currentIndexOffset++] = faceVertexIndices[next++];
                        newIndices[currentIndexOffset++] = faceVertexIndices[next];
                    }
                    currentFaceOffset += triCount;
                    last += faceVertexCounts[i];
                }
            }

            faceVertexIndices = newIndices;

            // triangulatedFaceVertexIndices needs to be a 'proper' copy of newIndices, else it will be affected when faceVertexIndices is modified
            triangulatedFaceVertexIndices = new int[newIndicesLength];
            Buffer.BlockCopy(newIndices, 0, triangulatedFaceVertexIndices, 0, Buffer.ByteLength(newIndices));

            faceVertexCounts = newCounts;
        }

        internal bool ShouldUnweldVertices(bool bindMaterials)
        {
            // If any primvar is face varying (1 value per vertex) or uniform (1 value per face), all  primvars + mesh attributes will have to be converted to face varying
            bool shouldUnweldColors = colors != null && (colors.GetInterpolationToken() == UsdGeomTokens.uniform || colors.GetInterpolationToken() == UsdGeomTokens.faceVarying);
            // TODO: expose interpolation for standard mesh attributes (normals, tangents)
            bool shouldUnweldNormals = normals != null && (normals.Length == originalFaceVertexCounts.Length || normals.Length > points.Length);
            bool shouldUnweldTangents = tangents != null && (tangents.Length == originalFaceVertexCounts.Length || tangents.Length > points.Length);

            return arePrimvarsFaceVarying || shouldUnweldNormals || shouldUnweldColors || shouldUnweldTangents || (bindMaterials && AreAnyArbitraryPrimvarsFaceVarying());
        }

        internal bool AreAnyArbitraryPrimvarsFaceVarying()
        {
            if (ArbitraryPrimvars == null) return false;
            foreach (var primvar in ArbitraryPrimvars)
            {
                if (primvar.Value.GetInterpolationToken() == UsdGeomTokens.faceVarying)
                    return true;
            }

            return false;
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

        void UnweldPrimvar(Primvar<object> primvar, bool changeHandedness)
        {
            if (primvar.value == null)
                return;

            if (primvar.GetInterpolationToken() == UsdGeomTokens.constant && primvar.IsArray)
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

        static void FlattenPrimvar(Primvar<object> primvar)
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

        internal static void TriangulateAttributes<T>(ref T[] values, int[] faceVertexCounts, bool changeHandedness)
        {
            int newFacesCount = 0;
            foreach (int faceVertexCount in faceVertexCounts)
            {
                newFacesCount += faceVertexCount - 2;
            }

            var newValues = new T[3 * newFacesCount]; // we're converting every face to a triangle, so must be three verts per face

            int last = 0, currentIndex = 0;
            if (changeHandedness)
            {
                for (var faceIndex = 0; faceIndex < faceVertexCounts.Length; faceIndex++)
                {
                    var next = last + 1;
                    for (var triangleIndex = 0; triangleIndex < faceVertexCounts[faceIndex] - 2; triangleIndex++)
                    {
                        newValues[currentIndex++] = values[next++];
                        newValues[currentIndex++] = values[last];
                        newValues[currentIndex++] = values[next];
                    }
                    last += faceVertexCounts[faceIndex];
                }
            }
            else
            {
                for (var faceIndex = 0; faceIndex < faceVertexCounts.Length; faceIndex++)
                {
                    var next = last + 1;
                    for (var triangleIndex = 0; triangleIndex < faceVertexCounts[faceIndex] - 2; triangleIndex++)
                    {
                        newValues[currentIndex++] = values[last];
                        newValues[currentIndex++] = values[next++];
                        newValues[currentIndex++] = values[next];
                    }
                    last += faceVertexCounts[faceIndex];
                }
            }

            values = newValues;
        }

        /// <summary>
        /// Utility method to convert a given array of values to the equivalent faceVarying array (one value per vertex per face).
        /// </summary>
        /// <remarks> If the interpolation of the array to convert is not known, it will be guessed based on the length of the array. </remarks>
        void ConvertInterpolationToFaceVarying<T>(ref T[] values, int[] vertexIndices, bool changeHandedness = false, TfToken interpolation = null)
        {
            if (values == null)
                return;

            // We assume a certain number of values in the array for certain interpolations, so check whether this assumption will work and not cause array overflows
            if (!IsInterpolationValid(interpolation, values.Length))
            {
                var newInterpolation = GuessInterpolation(values.Length);

                if (interpolation != null && interpolation != newInterpolation)
                {
                    UnityEngine.Debug.LogWarning($"Stated interpolation '{interpolation.ToString()}' for a PrimVar does not match required value counts. " +
                        $"We will convert to FaceVarying assuming an original interpolation of '{newInterpolation.ToString()}'.");
                }

                interpolation = newInterpolation;
            }

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
            if (IsTopologyBackedUp() && count == originalFaceVertexCounts.Length)
            {
                return UsdGeomTokens.uniform;
            }
            if (count == points.Length)
            {
                return UsdGeomTokens.vertex;
            }
            if (IsTopologyBackedUp() && count == originalFaceVertexIndices.Length)
            {
                return UsdGeomTokens.faceVarying;
            }

            return new TfToken();
        }

        /// <summary>
        /// Returns whether the stated interpolation is valid for the number of elements
        /// </summary>
        /// <param name="interpolation"> The expected interpolation of the array</param>
        /// <param name="count"> The number of elements in the array</param>
        /// <remarks>
        /// Attributes can have interpolation tokens stated that do not fit with our assumptions of value counts.
        /// This method checks that we can use the stated interpolation without risking array overflows, etc.
        /// </remarks>
        internal bool IsInterpolationValid(TfToken interpolation, int count)
        {
            if (interpolation == null)
                return false;
            if (interpolation == UsdGeomTokens.constant)
                return count == 1;
            if (interpolation == UsdGeomTokens.uniform)
                return (IsTopologyBackedUp() && count == originalFaceVertexCounts.Length);
            if (interpolation == UsdGeomTokens.vertex || interpolation == UsdGeomTokens.varying)
                return (count == points.Length);
            if (interpolation == UsdGeomTokens.faceVarying)
                return (IsTopologyBackedUp() && count == originalFaceVertexIndices.Length);
            return false;
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

            // for each original face
            for (int oldFaceIndex = 0, vertexIndex = 0; oldFaceIndex < values.Length; oldFaceIndex++)
            {
                var value = values[oldFaceIndex];
                // for each new face created
                for (int newFaceIndex = 0; newFaceIndex < originalFaceVertexCounts[oldFaceIndex] - 2; newFaceIndex++)
                {
                    // copy the old value for each vertex of the new triangle
                    newValues[vertexIndex] = value;
                    newValues[vertexIndex + 1] = value;
                    newValues[vertexIndex + 2] = value;

                    vertexIndex += 3;
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
