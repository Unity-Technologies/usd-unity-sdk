// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using pxr;

namespace USD.NET
{
    /// <summary>
    /// A Scene object represents a UsdStage and enables USD serialization and deserialization.
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// Dictates how data is written to the scene.
        /// </summary>
        public enum WriteModes
        {
            /// <summary>
            /// Every call to Write() also ensures the Prim is defined.
            /// </summary>
            Define,

            /// <summary>
            /// Calls to Write() will not define the Prim.
            /// </summary>
            Over,
        }

        /// <summary>
        /// Indicates how to interpolate values when requesting a value which lies between two key
        /// frames.
        /// </summary>
        /// <remarks>
        /// Held indicates each keyframe should be held (no interpolation) until the next
        /// key frame is reached; linear indicates the two bracketing key frames should be linearly
        /// interpolated.
        /// </remarks>
        public enum InterpolationMode
        {
            Held,
            Linear
        }

        /// <summary>
        /// Indicates the up-axis of the world contained in this cache.
        /// </summary>
        /// <remarks>
        /// These values are defined by USD, thus are limited to Y and Z.
        /// </remarks>
        public enum UpAxes
        {
            Y,
            Z
        }

        /// <summary>
        /// Gets the underlying UsdStage for this scene, if available.
        /// </summary>
        /// <remarks>
        /// This is exposed only for low level, direct access to the underlying stage, not intended
        /// for common use.
        /// </remarks>
        public UsdStage Stage { get { return m_stage; } }

        /// <summary>
        /// When non-null, limits the members which the scene will read during serialization.
        /// </summary>
        public AccessMask AccessMask { get; set; }

        /// <summary>
        /// When true, populates the AccessMask with with values which vary over time.
        /// </summary>
        /// <remarks>
        /// Usage: Set a valid AccessMaks, set the value to true, read all prims, set to false.
        /// Subsequent reads will only include prims from this set which vary over time.
        /// </remarks>
        public bool IsPopulatingAccessMask { get; set; }

        /// <summary>
        /// Returns the root layer identifier upon which this scene is operating (the EditTarget
        /// identifier). Note that for in-memory layers, this may not be a path on disk.
        /// </summary>
        public string FilePath
        {
            get
            {
                return m_stage.GetEditTarget().GetLayer().GetIdentifier();
            }
        }

        /// <summary>
        /// Dicatates how calls to Write() are handled. When set to Define (the default), every write
        /// will ensure the prim is also defined.
        /// </summary>
        public WriteModes WriteMode { get; set; }

        /// <summary>
        /// The time at which key frames should be read and written.
        /// </summary>
        /// <remarks>
        /// Setting this value to null indicates values should be read from the "default" time
        /// (e.g. to store a rest pose, etc.)
        /// </remarks>
        private double? m_time;
        public double? Time
        {
            get
            {
                return m_time;
            }
            set
            {
                m_time = value;
                TimeCode = m_time.HasValue ? new UsdTimeCode(m_time.Value) : pxr.UsdTimeCode.Default();
            }
        }

        /// <summary>
        /// The beginning bracket at which animation begins, the time of the first key frame.
        /// </summary>
        public double StartTime
        {
            get
            {
                return Stage.GetEditTarget().GetLayer().GetStartTimeCode();
            }
            set
            {
                lock (m_stageLock)
                {
                    Stage.GetEditTarget().GetLayer().SetStartTimeCode(value);
                }
            }
        }

        /// <summary>
        /// The closing bracket at which animation ends, the time of the last key frame.
        /// </summary>
        public double EndTime
        {
            get
            {
                return Stage.GetEditTarget().GetLayer().GetEndTimeCode();
            }
            set
            {
                lock (m_stageLock)
                {
                    Stage.GetEditTarget().GetLayer().SetEndTimeCode(value);
                }
            }
        }

        /// <summary>
        /// The rate at which the cache should playback, in frames per second.
        /// </summary>
        public double FrameRate
        {
            get
            {
                return Stage.GetFramesPerSecond();
            }
            set
            {
                if (value <= 0)
                {
                    throw new ApplicationException("Invalid frame rate, frame rate must be > 0");
                }
                lock (m_stageLock)
                {
                    Stage.SetTimeCodesPerSecond(value);
                    Stage.SetFramesPerSecond(value);
                }
            }
        }

        /// <summary>
        /// The Up-axis of the world contained in the cache.
        /// </summary>
        public UpAxes UpAxis
        {
            get
            {
                VtValue val = new VtValue();
                string upAxis = null;
                if (Stage.GetMetadata(kUpAxisToken, val))
                {
                    upAxis = UsdCs.VtValueToTfToken(val).ToString();
                }

                if (!string.IsNullOrEmpty(upAxis))
                {
                    return (UpAxes)Enum.Parse(typeof(UpAxes), upAxis);
                }
                else
                {
                    // USD defines the default up-axis to be Z.
                    return UpAxes.Z;
                }
            }
            set
            {
                VtValue val = new pxr.TfToken(value.ToString());
                Stage.SetMetadata(kUpAxisToken, val);
            }
        }

        /// <summary>
        /// The linear meters per unit of the world contained in the cache.
        /// The fallback value if not authored is centimeters.
        /// </summary>
        public double MetersPerUnit
        {
            get
            {
                // default to centimeters if we cannot retrieve metadata
                const double centimeters = 0.01;
                if (Stage == null)
                    return centimeters;
                VtValue val = new VtValue();
                if (Stage.GetMetadata(kMetersPerUnitToken, val))
                {
                    return UsdCs.VtValueTodouble(val);
                }
                return centimeters;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ApplicationException("Invalid metersPerUnit, meters per unit must be > 0");
                }
                Stage.SetMetadata(kMetersPerUnitToken, new VtValue(value));
            }
        }


        /// <summary>
        /// A list of all Prim paths present in the scene.
        /// </summary>
        public SdfPath[] AllPaths
        {
            get
            {
                return VectorToArray(Stage.GetAllPaths());
            }
        }

        [Obsolete("Use Find<MeshSample>() instead. This API will be removed in a future release.")]
        public SdfPath[] AllMeshes
        {
            get
            {
                return VectorToArray(Stage.GetAllPathsByType("Mesh", SdfPath.AbsoluteRootPath()));
            }
        }

        [Obsolete("Use Find<XformableSample>() instead. This API will be removed in a future release.")]
        public SdfPath[] AllXforms
        {
            get
            {
                return VectorToArray(Stage.GetAllPathsByType("Xform", SdfPath.AbsoluteRootPath()));
            }
        }

        /// <summary>
        /// Constructs a new scene at the given file path. If the file exists, it is erased, if
        /// the file is in use or cannot be created, an exception is thrown.
        /// </summary>
        public static Scene Create(string filePath)
        {
            var stage = UsdStage.CreateNew(filePath);
            if (stage == null)
            {
                throw new ApplicationException("Failed to create: " + filePath);
            }
            var scene = new Scene(stage);
            scene.UpAxis = UpAxes.Y;
            scene.MetersPerUnit = 1.0;
            return scene;
        }

        /// <summary>
        /// Constructs a new scene in memory, not backed by a file.
        /// </summary>
        ///
        /// <remarks>
        /// Note that SaveAs can be used to write memory to disk.
        /// </remarks>
        public static Scene Create()
        {
            var scene = new Scene(UsdStage.CreateInMemory());
            scene.UpAxis = UpAxes.Y;
            scene.MetersPerUnit = 1.0;
            return scene;
        }

        /// <summary>
        /// Opens an existing USD file for reading.
        /// An exception is thrown if the filePath cannot be opened.
        /// </summary>
        public static Scene Open(string filePath)
        {
            var stage = UsdStage.Open(filePath);
            if (stage == null)
            {
                throw new ApplicationException("Failed to open: " + filePath);
            }
            return new Scene(stage);
        }

        /// <summary>
        /// Constructs a scene from an existing stage.
        /// </summary>
        public static Scene Open(UsdStage stage)
        {
            if (stage == null)
            {
                throw new NullReferenceException("Null stage");
            }
            return new Scene(stage);
        }

        /// <summary>
        /// Gets the UsdPrim at the given path, retuns null if the UsdPrim is invalid.
        /// Therefore, if the return value is not null, IsValid need not be checked.
        /// </summary>
        public UsdPrim GetPrimAtPath(string primPath)
        {
            var p = Stage.GetPrimAtPath(new SdfPath(primPath));
            if (p == null || !p.IsValid())
            {
                return null;
            }
            return p;
        }

        /// <summary>
        /// Translates the given string into an SdfPath, returning a cached value if possible.
        /// </summary>
        public SdfPath GetSdfPath(string path)
        {
            SdfPath sdfPath;
            if (!m_pathMap.TryGetValue(path, out sdfPath))
            {
                sdfPath = new SdfPath(path);
                m_pathMap[path] = sdfPath;
            }
            return sdfPath;
        }

        /// <summary>
        /// Gets the UsdAttribute at the given path, retuns null if the UsdAttribute is invalid.
        /// Therefore, if the return value is not null, IsValid need not be checked.
        /// </summary>
        public UsdAttribute GetAttributeAtPath(string attrPath)
        {
            var attrSdfPath = new SdfPath(attrPath);
            var a = Stage.GetAttributeAtPath(attrSdfPath);
            if (a == null || !a.IsValid())
            {
                return null;
            }
            return a;
        }

        /// <summary>
        /// Gets the UsdRelationship at the given path, retuns null if the UsdRelationship is invalid.
        /// Therefore, if the return value is not null, IsValid need not be checked.
        /// </summary>
        public UsdRelationship GetRelationshipAtPath(string relPath)
        {
            var relSdfPath = new SdfPath(relPath);
            var rel = Stage.GetRelationshipAtPath(relSdfPath);
            if (rel == null || !rel.IsValid())
            {
                return null;
            }
            return rel;
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name declared
        /// for the given sample type, or those which derive from it.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <returns>
        /// An iterable collection of the paths discovered
        /// </returns>
        public SdfPath[] Find<T>() where T : SampleBase, new()
        {
            return Find<T>(rootPath: SdfPath.AbsoluteRootPath());
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name declared
        /// for the given sample type, or those which derive from it.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <param name="rootPath">The root path at which to begin the search.</param>
        /// <returns>
        /// An iterable collection of the paths discovered
        /// </returns>
        public SdfPath[] Find<T>(string rootPath) where T : SampleBase, new()
        {
            return Find<T>(new SdfPath(rootPath));
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name declared
        /// for the given sample type, or those which derive from it.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <param name="rootPath">The root path at which to begin the search.</param>
        /// <returns>
        /// An iterable collection of the paths discovered
        /// </returns>
        public SdfPath[] Find<T>(SdfPath rootPath) where T : SampleBase, new()
        {
            var attrs = typeof(T).GetCustomAttributes(typeof(USD.NET.UsdSchemaAttribute), true);
            if (attrs.Length == 0)
            {
                throw new ApplicationException("Invalid type T, does not have UsdSchema attribute");
            }
            var schemaTypeName = ((UsdSchemaAttribute)attrs[0]).Name;
            return VectorToArray(Stage.GetAllPathsByType(schemaTypeName, rootPath));
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name or those
        /// which are derived from it.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="usdSchemaTypeName">
        /// The USD schema type name (e.g. UsdGeomMesh) or its alias (e.g. Mesh).
        /// </param>
        /// <returns>
        /// Returns an iterable collection of UsdPrim paths.
        /// </returns>
        public SdfPath[] Find(string rootPath, string usdSchemaTypeName)
        {
            return VectorToArray(Stage.GetAllPathsByType(usdSchemaTypeName, new SdfPath(rootPath)));
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name or those
        /// which are derived from it.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <returns>
        /// Returns a collection which will read each prim found and return the requested SampleBase
        /// object type.
        /// </returns>
        /// <remarks>Internally, this method reuses a single object while reading to minimize garbage
        /// generated during iteration.</remarks>
        public SampleCollection<T> ReadAll<T>() where T : SampleBase, new()
        {
            return ReadAll<T>(rootPath: SdfPath.AbsoluteRootPath());
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name or those
        /// which are derived from it.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <param name="rootPath">The root path at which to begin the search.</param>
        /// <returns>
        /// Returns a collection which will read each prim found and return the requested SampleBase
        /// object type.
        /// </returns>
        /// <remarks>Internally, this method reuses a single object while reading to minimize garbage
        /// generated during iteration.</remarks>
        public SampleCollection<T> ReadAll<T>(string rootPath) where T : SampleBase, new()
        {
            return ReadAll<T>(new SdfPath(rootPath));
        }

        /// <summary>
        /// Searches the USD Stage to find prims which either match the schema type name or those
        /// which are derived from it.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <param name="rootPath">The root path at which to begin the search.</param>
        /// <returns>
        /// Returns a collection which will read each prim found and return the requested SampleBase
        /// object type.
        /// </returns>
        /// <remarks>Internally, this method reuses a single object while reading to minimize garbage
        /// generated during iteration.</remarks>
        public SampleCollection<T> ReadAll<T>(SdfPath rootPath) where T : SampleBase, new()
        {
            var attrs = typeof(T).GetCustomAttributes(typeof(UsdSchemaAttribute), true);
            if (attrs.Length == 0)
            {
                throw new ApplicationException("Invalid type T, does not have UsdSchema attribute");
            }
            var schemaTypeName = ((UsdSchemaAttribute)attrs[0]).Name;
            var vec = Stage.GetAllPathsByType(schemaTypeName, rootPath);
            return new SampleCollection<T>(this, vec);
        }

        /// <summary>
        /// At each path, reads the given sample type from t he USD Stage.
        /// </summary>
        /// <typeparam name="T">
        /// A type which inherits from SampleBase, adorned with a UsdSchemaAttribute.
        /// </typeparam>
        /// <returns>
        /// Returns a collection which will read each prim found and return the requested SampleBase
        /// object type.
        /// </returns>
        public SampleCollection<T> ReadAll<T>(SdfPath[] paths) where T : SampleBase, new()
        {
            var vec = new SdfPathVector();
            foreach (SdfPath path in paths)
            {
                vec.Add(path);
            }
            return new SampleCollection<T>(this, vec);
        }

        /// <summary>
        /// Release any open files and stop asynchronous execution.
        /// </summary>
        public void Close()
        {
            if (m_stage != null)
            {
                this.m_stage.Dispose();
                this.m_stage = null;
            }
        }

        /// <summary>
        /// Sets the active interpolation mode.
        /// </summary>
        public void SetInterpolation(InterpolationMode mode)
        {
            if (mode == InterpolationMode.Held)
            {
                Stage.SetInterpolationType(UsdInterpolationType.UsdInterpolationTypeHeld);
            }
            else if (mode == InterpolationMode.Linear)
            {
                Stage.SetInterpolationType(UsdInterpolationType.UsdInterpolationTypeLinear);
            }
            else
            {
                throw new ArgumentException(string.Format("Unknown interpolation mode: {0}", mode));
            }
        }

        /// <summary>
        /// Retuns a dictionary of paths and the times at which each path has a keyframe for the given
        /// attribute. Only paths rooted under the given rootPath are considered.
        /// </summary>
        public Dictionary<string, double[]> ComputeKeyFrames(string rootPath, string attribute)
        {
            var keys = new Dictionary<string, double[]>();
            var prim = GetUsdPrim(GetSdfPath(rootPath));

            if (!prim)
            {
                throw new ArgumentException("rootPath does not exist");
            }
            var sdfRootPath = GetSdfPath(rootPath);
            var tfAttrName = new pxr.TfToken(attribute);
            foreach (var child in Stage.GetAllPrims())
            {
                if (child.GetPath() == SdfPath.AbsoluteRootPath())
                {
                    continue;
                }

                if (!child.GetPath().HasPrefix(sdfRootPath))
                {
                    continue;
                }

                var stdDoubleVector = child.GetAttribute(tfAttrName).GetTimeSamples();
                if (stdDoubleVector.Count == 0)
                {
                    continue;
                }

                double[] times = new double[stdDoubleVector.Count];
                stdDoubleVector.CopyTo(times);
                keys.Add(child.GetPath(), times);
            }

            return keys;
        }

        /// <summary>
        /// Adds the root layer of given Scene object as a sublayer of this Scene.
        /// Note that this operation triggers recomposition and will invalidate UsdPrim instances.
        /// </summary>
        public void AddSubLayer(Scene over)
        {
            SdfLayerHandle rootLayer = Stage.GetRootLayer();
            var overLayer = over.Stage.GetRootLayer();
            rootLayer.GetSubLayerPaths().push_back(overLayer.GetIdentifier());
        }

        /// <summary>
        /// Set the layer to which this scene will author when writing values.
        /// </summary>
        public void SetEditTarget(Scene other)
        {
            SdfLayerHandle rootLayer = other.Stage.GetRootLayer();
            var editTarget = Stage.GetEditTargetForLocalLayer(rootLayer);
            Stage.SetEditTarget(editTarget);
        }

        /// <summary>
        /// Saves the current scene if backed by a file, throws an exception if this scene is in
        /// memory only.
        /// </summary>
        public void Save()
        {
            m_stage.Save();
        }

        /// <summary>
        /// Write the root layer of the current scene to the given file path, preserving all references.
        /// </summary>
        public void SaveAs(string filePath)
        {
            m_stage.GetEditTarget().GetLayer().Export(filePath);
        }

        /// <summary>
        /// Writes the current scene to the given file path, flattening all references.
        /// </summary>
        ///
        /// <remarks>
        /// The resulting scene will have no external file references and all referenced data will be
        /// inlined.
        /// </remarks>
        public void FlattenAs(string filePath)
        {
            m_stage.Flatten(addSourceFileComment: false).Export(filePath);
        }

        /// <summary>
        /// Reads and deserializes an object of type T at the given path in the current USD scene.
        /// </summary>
        ///
        /// <remarks>
        /// If reading fails, an excpetion is thrown and the object may be left in an incomplete state.
        /// </remarks>
        public void Read<T>(string path, T sample) where T : SampleBase
        {
            ReadInternal(GetSdfPath(path), sample, TimeCode);
        }

        public void Read<T>(SdfPath path, T sample) where T : SampleBase
        {
            ReadInternal(path, sample, TimeCode);
        }

        /// <summary>
        /// Reads a single field of a sample, attribute ecorations are respected.
        /// </summary>
        /// <remarks>
        /// Note only one level of nesting is supported, e.g. if a sample has a nested structure, only
        /// immediate child structures of the root may be deserialized.
        /// </remarks>
        public void Read<T>(string path, System.Reflection.FieldInfo fieldInfo, ref T memberValue)
        {
            Read(GetSdfPath(path), fieldInfo, ref memberValue);
        }

        public void Read<T>(SdfPath path, System.Reflection.FieldInfo fieldInfo, ref T memberValue)
        {
            var prim = GetUsdPrim(path);
            // Erase type.
            object o = memberValue;
            bool? mayVary = null;
            m_usdIo.Deserialize(ref o, prim, TimeCode, fieldInfo, null, ref mayVary);

            // Bring the value back, required for value types.
            memberValue = (T)o;
        }

        /// <summary>
        /// Reads a single property of a sample, attribute decorations are respected.
        /// </summary>
        /// <remarks>
        /// Note only one level of nesting is supported, e.g. if a sample has a nested structure, only
        /// immediate child structures of the root may be deserialized.
        /// </remarks>
        public void Read<T>(string path, System.Reflection.PropertyInfo fieldInfo, ref T memberValue)
        {
            Read(GetSdfPath(path), fieldInfo, ref memberValue);
        }

        public void Read<T>(SdfPath path, System.Reflection.PropertyInfo propInfo, ref T memberValue)
        {
            var prim = GetUsdPrim(path);

            // Erase type.
            object o = memberValue;
            bool? mayVary = null;
            m_usdIo.Deserialize(ref o, prim, TimeCode, propInfo, null, ref mayVary);

            // Bring the value back, required for value types.
            memberValue = (T)o;
        }

        void ReadInternal<T>(SdfPath path,
            T sample,
            UsdTimeCode timeCode) where T : SampleBase
        {
            var prim = GetUsdPrim(path);
            if (!prim) { return; }

            var accessMap = AccessMask;
            DeserializationContext deserializationContext = null;

            // mayVary is nullable and has an accumulation semantic:
            //   null = members have already been checked for animation
            //   false = no dynamic members found
            //   true =  at least one member has been found dynamic
            bool? mayVary = false;

            // When reading animation data, the access map optimizes which prim members need to be read
            if (accessMap != null)
            {
                var populatingAccessMask = IsPopulatingAccessMask;
                lock (m_stageLock)
                {
                    // Check which attributes of the prim are dynamic
                    var primFound = accessMap.Included.TryGetValue(path, out deserializationContext);

                    // Populating the access map happens when reading the first frame of animation
                    // so if the prim is not already in the map add it and everything will be deserialized
                    if (!primFound && populatingAccessMask)
                    {
                        deserializationContext = new DeserializationContext();
                        accessMap.Included.Add(path, deserializationContext);
                    }
                }

                // If we are not populating the access map it means it's been done already so only dynamic members should be deserialized
                if (!populatingAccessMask)
                {
                    // If there are no dynamic members, then no need to call deserialize
                    if (deserializationContext?.dynamicMembers == null)
                        return;

                    // Notify the deserialization service that only dynamic members should be read
                    mayVary = null;
                }
            }

            m_usdIo.Deserialize(sample, prim, timeCode, deserializationContext?.dynamicMembers, ref mayVary);

            // If no members are varying, remove the prim from the access map.
            lock (m_stageLock)
            {
                if (accessMap != null && mayVary != null)
                {
                    if (!mayVary.Value)
                    {
                        if (accessMap.Included.ContainsKey(path))
                        {
                            accessMap.Included.Remove(path);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes an object of type T to the given path in the current USD scene.
        /// </summary>
        ///
        /// <remarks>
        /// If writing fails, the scene description may be partially written.
        /// </remarks>
        public void Write<T>(string path, T sample) where T : SampleBase
        {
            Write(GetSdfPath(path), sample);
        }

        public void Write<T>(SdfPath path, T sample) where T : SampleBase
        {
            WriteInternal(path, sample, TimeCode);
        }

        private void WriteInternal<T>(SdfPath path,
            T sample,
            UsdTimeCode timeCode) where T : SampleBase
        {
            pxr.UsdPrim prim;
            lock (m_stageLock)
            {
                if (WriteMode == WriteModes.Define)
                {
                    // At the moment multiple ExportPlans end up having the same SdfPath which make Xform schema type override the actual schema type
                    // Also typeless Prims may be created when Child objects are created before Parents in the export plans and we need to make sure their type is updated
                    // The next code is a hacky way to maintain the desired schema type until we refactor the export code
                    var primTypeName = Reflect.GetSchema(typeof(T));

                    prim = m_stage.GetPrimAtPath(path);
                    if (prim == null || !prim.IsValid())
                    {
                        prim = m_stage.DefinePrim(path, new TfToken(primTypeName));
                    }
                    else if (!string.IsNullOrEmpty(primTypeName) && (string.IsNullOrEmpty(prim.GetTypeName().GetText()) || primTypeName != "Xform"))
                    {
                        prim.SetTypeName(new TfToken(primTypeName));
                    }
                }
                else
                {
                    prim = m_stage.OverridePrim(path);
                }

                if (prim == null || !prim.IsValid())
                {
                    throw new Exception("Failed to "
                        + (WriteMode == WriteModes.Define ? "define" : "override") + " prim: " + path);
                }
            }
            m_usdIo.Serialize(sample, prim, timeCode);
        }

        /// <summary>
        /// Returns the entire scene serialized as a string. Expensive for large scenes.
        /// </summary>
        override public string ToString()
        {
            string s;
            m_stage.ExportToString(out s, false);
            return s;
        }

        #region "Private API"
        // ----------------------------------------------------------------------------------------- //
        // Private API
        // ----------------------------------------------------------------------------------------- //

        /// <remarks>
        /// Converts an std::vector to an array, but why?
        /// Swig's current implementation of std::vector returns references to private memory, which
        /// means any value obtained from it is extremely unsafe and will cause C++ style memory errors
        /// if used after the vector is disposed. For safety, our API should never return a raw vector.
        /// </remarks>
        private SdfPath[] VectorToArray(SdfPathVector vec)
        {
            var ret = new SdfPath[vec.Count];
            vec.CopyTo(ret);
            return ret;
        }

        private SdfPath GetSdfPath(pxr.SdfPath path)
        {
            throw new ApplicationException("TODO: don't allow implicit conversion path -> string");
        }

        /// <summary>
        /// Returns the UsdPrim at the given path, returning null if the path is invalid.
        /// </summary>
        private UsdPrim GetUsdPrim(string path)
        {
            return GetUsdPrim(GetSdfPath(path));
        }

        /// <summary>
        /// Returns the UsdPrim at the given path, returning null if the path is invalid.
        /// </summary>
        private pxr.UsdPrim GetUsdPrim(SdfPath path)
        {
            lock (m_stageLock)
            {
                return Stage.GetPrimAtPath(path);
            }
        }

        /// <summary>
        /// Constructor declared private to force access through the static factories.
        /// </summary>
        private Scene(UsdStage stage)
        {
            if (stage == null)
            {
                throw new System.NullReferenceException("Scene was constructed with a null UsdStage");
            }
            m_stage = stage;
            m_usdIo = new UsdIo(m_stageLock);
            // Initialize Time / TimeCode to UsdTimeCode.Default();
            Time = null;
        }

        /// <summary>
        /// Returns the current Time value as a UsdTimeCode object, converting null to Default time
        /// if needed.
        /// </summary>
        private UsdTimeCode TimeCode
        {
            get;
            set;
        }

        #endregion

        private Dictionary<string, pxr.SdfPath> m_pathMap = new Dictionary<string, SdfPath>();
        private object m_stageLock = new object();
        private UsdIo m_usdIo;
        private UsdStage m_stage;

        // Cache TfTokens for reuse to avoid P/Invoke and token churn.
        private static readonly TfToken kUpAxisToken = new TfToken("upAxis");
        private static readonly TfToken kYUpToken = new TfToken("Y");
        private static readonly TfToken kMetersPerUnitToken = new TfToken("metersPerUnit");
    }
}
