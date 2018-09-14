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

namespace USD.NET {

  /// <summary>
  /// A Scene object represents a UsdStage and enables USD serialization and deserialization.
  /// </summary>
  public class Scene {

    /// <summary>
    /// Dictates how data is written to the scene.
    /// </summary>
    public enum WriteModes {
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
    public enum InterpolationMode {
      Held,
      Linear
    }

    /// <summary>
    /// Indicates the up-axis of the world contained in this cache.
    /// </summary>
    /// <remarks>
    /// These values are defined by USD, thus are limited to Y and Z.
    /// </remarks>
    public enum UpAxes {
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
    public double? Time {
      get {
        return m_time;
      }
      set {
        m_time = value;
        TimeCode = m_time.HasValue ? new UsdTimeCode(m_time.Value) : pxr.UsdTimeCode.Default();
      }
    }

    /// <summary>
    /// The beginning bracket at which animation begins, the time of the first key frame.
    /// </summary>
    public double StartTime {
      get {
        return Stage.GetEditTarget().GetLayer().GetStartTimeCode();
      }
      set {
        lock (m_stageLock) {
          Stage.GetEditTarget().GetLayer().SetStartTimeCode(value);
        }
      }
    }

    /// <summary>
    /// The closing bracket at which animation ends, the time of the last key frame.
    /// </summary>
    public double EndTime {
      get {
        return Stage.GetEditTarget().GetLayer().GetEndTimeCode();
      }
      set {
        lock (m_stageLock) {
          Stage.GetEditTarget().GetLayer().SetEndTimeCode(value);
        }
      }
    }

    /// <summary>
    /// The rate at which the cache should playback, in frames per second.
    /// </summary>
    public double FrameRate {
      get {
        return Stage.GetFramesPerSecond();
      }
      set {
        if (value <= 0) {
          throw new ApplicationException("Invalid frame rate, frame rate must be > 0");
        }
        lock (m_stageLock) {
          Stage.SetTimeCodesPerSecond(value);
          Stage.SetFramesPerSecond(value);
        }
      }
    }

    /// <summary>
    /// The Up-axis of the world contained in the cache.
    /// </summary>
    public UpAxes UpAxis {
      get {
        VtValue val = new VtValue();
        string upAxis = null;
        if (Stage.GetMetadata(kUpAxisToken, val)) {
          upAxis = UsdCs.VtValueToTfToken(val).ToString();
        }

        if (!string.IsNullOrEmpty(upAxis)) {
          return (UpAxes)Enum.Parse(typeof(UpAxes), upAxis);
        } else {
          // USD defines the default up-axis to be Z.
          return UpAxes.Z;
        }
      }
      set {
        VtValue val = new pxr.TfToken(value.ToString());
        Stage.SetMetadata(kUpAxisToken, val);
      }
    }

    /// <summary>
    /// A list of all Prim paths present in the scene.
    /// </summary>
    public PathCollection AllPaths {
      get {
        return new PathCollection(Stage.GetAllPaths());
      }
    }

    /// <summary>
    /// A list of all mesh paths in the scene.
    /// </summary>
    public PathCollection AllMeshes {
      get {
        return new PathCollection(Stage.GetAllPathsByType("Mesh"));
      }
    }

    /// <summary>
    /// A list of all Xform paths in the scene.
    /// </summary>
    public PathCollection AllXforms {
      get {
        return new PathCollection(Stage.GetAllPathsByType("Xform"));
      }
    }

    /// <summary>
    /// Constructs a new scene at the given file path. If the file exists, it is erased, if
    /// the file is in use or cannot be created, an exception is thrown.
    /// </summary>
    public static Scene Create(string filePath) {
      var stage = UsdStage.CreateNew(filePath);
      if (stage == null) {
        throw new ApplicationException("Failed to create: " + filePath);
      }
      var scene = new Scene(stage);
      scene.UpAxis = UpAxes.Y;
      return scene;
    }

    /// <summary>
    /// Constructs a new scene in memory, not backed by a file.
    /// </summary>
    /// 
    /// <remarks>
    /// Note that SaveAs can be used to write memory to disk.
    /// </remarks>
    public static Scene Create() {
      var scene = new Scene(UsdStage.CreateInMemory());
      scene.UpAxis = UpAxes.Y;
      return scene;
    }

    /// <summary>
    /// Opens an existing USD file for reading.
    /// An exception is thrown if the filePath cannot be opened.
    /// </summary>
    public static Scene Open(string filePath) {
      var stage = UsdStage.Open(filePath);
      if (stage == null) {
        throw new ApplicationException("Failed to open: " + filePath);
      }
      return new Scene(stage);
    }

    /// <summary>
    /// Gets the UsdPrim at the given path, retuns null if the UsdPrim is invalid.
    /// Therefore, if the return value is not null, IsValid need not be checked.
    /// </summary>
    public UsdPrim GetPrimAtPath(string primPath) {
      var p = Stage.GetPrimAtPath(new SdfPath(primPath));
      if (p == null || !p.IsValid()) {
        return null;
      }
      return p;
    }

    /// <summary>
    /// Gets the UsdAttribute at the given path, retuns null if the UsdAttribute is invalid.
    /// Therefore, if the return value is not null, IsValid need not be checked.
    /// </summary>
    public UsdAttribute GetAttributeAtPath(string attrPath) {
      var attrSdfPath = new SdfPath(attrPath);
      var p = Stage.GetPrimAtPath(attrSdfPath.GetPrimPath());
      if (p == null || !p.IsValid()) {
        return null;
      }
      var a = p.GetAttribute(attrSdfPath.GetNameToken());
      if (a == null || !a.IsValid()) {
        return null;
      }
      return a;
    }

    /// <summary>
    /// Gets the UsdRelationship at the given path, retuns null if the UsdRelationship is invalid.
    /// Therefore, if the return value is not null, IsValid need not be checked.
    /// </summary>
    public UsdRelationship GetRelationshipAtPath(string relPath) {
      var relSdfPath = new SdfPath(relPath);
      var p = Stage.GetPrimAtPath(relSdfPath.GetPrimPath());
      if (p == null || !p.IsValid()) {
        return null;
      }
      var rel = p.GetRelationship(relSdfPath.GetNameToken());
      if (rel == null || !rel.IsValid()) {
        return null;
      }
      return rel;
    }

    /// <summary>
    /// Release any open files and stop asynchronous execution.
    /// </summary>
    public void Close() {
      if (m_stage != null) {
        this.m_stage.Dispose();
        this.m_stage = null;
      }
      this.m_bgExe.Stop();
    }

    /// <summary>
    /// Sets the active interpolation mode.
    /// </summary>
    public void SetInterpolation(InterpolationMode mode) {
      if (mode == InterpolationMode.Held) {
        Stage.SetInterpolationType(UsdInterpolationType.UsdInterpolationTypeHeld);
      } else if (mode == InterpolationMode.Linear) {
        Stage.SetInterpolationType(UsdInterpolationType.UsdInterpolationTypeLinear);
      } else {
        throw new ArgumentException(string.Format("Unknown interpolation mode: {0}", mode));
      }
    }

    /// <summary>
    /// Retuns a dictionary of paths and the times at which each path has a keyframe for the given
    /// attribute. Only paths rooted under the given rootPath are considered.
    /// </summary>
    public Dictionary<string, double[]> ComputeKeyFrames(string rootPath, string attribute) {
      var keys = new Dictionary<string, double[]>();
      var prim = GetUsdPrim(GetSdfPath(rootPath));

      if (!prim) {
        throw new ArgumentException("rootPath does not exist");
      }
      var sdfRootPath = GetSdfPath(rootPath);
      var tfAttrName = new pxr.TfToken(attribute);
      foreach(var child in Stage.GetAllPrims()) {
        if (child.GetPath() == SdfPath.AbsoluteRootPath()) {
          continue;
        }

        if (!child.GetPath().HasPrefix(sdfRootPath)) {
          continue;
        }
        
        var stdDoubleVector = child.GetAttribute(tfAttrName).GetTimeSamples();
        if (stdDoubleVector.Count == 0) {
          continue;
        }

        double[] times = new double[stdDoubleVector.Count];
        stdDoubleVector.CopyTo(times);
        keys.Add(child.GetPath(), times);
      }

      return keys;
    }

    public void AddSubLayer(Scene over) {
      SdfLayerHandle rootLayer = Stage.GetRootLayer();
      var overLayer = over.Stage.GetRootLayer();
      StdStringVector subLayers;

      // TODO: add to existing layers if present.
#if false
      var vtValue = new VtValue();
      over.Stage.GetMetadata(new TfToken("subLayers"), vtValue);
      if (!vtValue.IsEmpty()) {
        var vtArray = pxr.UsdCs.VtValueToVtStringArray(vtValue);
        vtArray.push_back(overLayer.GetIdentifier());
        var strings = new string[vtArray.size()];
        vtArray.CopyToArray(strings);
        subLayers = new StdStringVector(strings);
      } else {
#endif

      subLayers = new StdStringVector(new string[] { overLayer.GetIdentifier() });

      /*
      var vtValue = new VtValue();
      over.Stage.GetMetadata(new TfToken("subLayers"), vtValue);
      if (!vtValue.IsEmpty()) {
        var vtArray = pxr.UsdCs.VtValueToVtStringArray(vtValue);
        vtArray.push_back(overLayer.GetIdentifier());
        var strings = new string[vtArray.size()];
        vtArray.CopyToArray(strings);
        subLayers = new StdStringVector(strings);
      }
      */

      rootLayer.SetSubLayerPaths(subLayers);
    }

    public void SetEditTarget(Scene over) {
      SdfLayerHandle rootLayer = over.Stage.GetRootLayer();
      var editTarget = Stage.GetEditTargetForLocalLayer(rootLayer);
      Stage.SetEditTarget(editTarget);
      m_primMap.Clear();
    }

    /// <summary>
    /// Wait until all asynchronous writes complete.
    /// </summary>
    public void WaitForWrites() {
      m_bgExe.Paused = false;
      m_bgExe.WaitForWrites();
    }

    /// <summary>
    /// Wait until all asynchronous reads complete.
    /// </summary>
    public void WaitForReads() {
      m_bgExe.Paused = false;
      m_bgExe.WaitForReads();
    }

    /// <summary>
    /// Saves the current scene if backed by a file, throws an exception if this scene is in
    /// memory only.
    /// </summary>
    public void Save() {
      WaitForWrites();
      m_stage.Save();
    }

    /// <summary>
    /// Write the root layer of the current scene to the given file path, preserving all references.
    /// </summary>
    public void SaveAs(string filePath) {
      WaitForWrites();
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
    public void FlattenAs(string filePath) {
      WaitForWrites();
      m_stage.Flatten(addSourceFileComment: false).Export(filePath);
    }

    /// <summary>
    /// Reads and deserializes an object of type T at the given path in the current USD scene.
    /// </summary>
    /// 
    /// <remarks>
    /// If reading fails, an excpetion is thrown and the object may be left in an incomplete state.
    /// </remarks>
    public void Read<T>(string path, T sample) where T : SampleBase {
      ReadInternal(GetSdfPath(path), sample, TimeCode);
    }
    public void Read<T>(SdfPath path, T sample) where T : SampleBase {
      ReadInternal(path, sample, TimeCode);
    }

    /// <summary>
    /// Reads a single field of a sample, attribute ecorations are respected.
    /// </summary>
    /// <remarks>
    /// Note only one level of nesting is supported, e.g. if a sample has a nested structure, only
    /// immediate child structures of the root may be deserialized.
    /// </remarks>
    public void Read<T>(string path, System.Reflection.FieldInfo fieldInfo, ref T memberValue) {
      Read(GetSdfPath(path), fieldInfo, ref memberValue);
    }
    public void Read<T>(SdfPath path, System.Reflection.FieldInfo fieldInfo, ref T memberValue) {
      var prim = GetUsdPrim(path);
      // Erase type.
      object o = memberValue;
      m_usdIo.Deserialize(ref o, prim, TimeCode, fieldInfo);

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
    public void Read<T>(string path, System.Reflection.PropertyInfo fieldInfo, ref T memberValue) {
      Read(GetSdfPath(path), fieldInfo, ref memberValue);
    }
    public void Read<T>(SdfPath path, System.Reflection.PropertyInfo propInfo, ref T memberValue) {
      var prim = GetUsdPrim(path);

      // Erase type.
      object o = memberValue;
      m_usdIo.Deserialize(ref o, prim, TimeCode, propInfo);

      // Bring the value back, required for value types.
      memberValue = (T)o;
    }

    public void ReadAsync<T>(string path, T sample) where T : SampleBase {
      ReadAsync(GetSdfPath(path), sample);
    }
    public void ReadAsync<T>(SdfPath path, T sample) where T : SampleBase {
      m_bgExe.AsyncRead(() => ReadInternal(path, sample, TimeCode));
    }

    private void ReadInternal<T>(SdfPath path,
                                 T sample,
                                 UsdTimeCode timeCode) where T: SampleBase {
      var prim = GetUsdPrim(path);
      if (!prim) { return; }
      m_usdIo.Deserialize(sample, prim, timeCode);
    }

    /// <summary>
    /// Writes an object of type T to the given path in the current USD scene.
    /// </summary>
    /// 
    /// <remarks>
    /// If writing fails, the scene description may be partially written.
    /// </remarks>
    public void Write<T>(string path, T sample) where T : SampleBase {
      Write(GetSdfPath(path), sample);
    }
    public void Write<T>(SdfPath path, T sample) where T : SampleBase {
      WriteInternal(path, sample, TimeCode);
    }

    public void WriteAsync<T>(string path, T sample) where T : SampleBase {
      WriteAsync(GetSdfPath(path), sample);
    }
    public void WriteAsync<T>(SdfPath path, T sample) where T : SampleBase {
      m_bgExe.AsyncWrite(() => WriteInternal(path, sample, TimeCode));
    }

    private void WriteInternal<T>(SdfPath path,
                                  T sample,
                                  UsdTimeCode timeCode) where T : SampleBase {
      pxr.UsdPrim prim;
      lock (m_stageLock) {
        // TODO(jcowles): there is a potential issue here if the cache gets out of sync with the 
        // underlying USD scene. The correct fix is to listen for change processing events and
        // clear the cache accordingly.
        if (!m_primMap.TryGetValue(path, out prim)) {
          if (WriteMode == WriteModes.Define) {
            prim = m_stage.DefinePrim(path, new TfToken(Reflect.GetSchema(typeof(T))));
          } else {
            prim = m_stage.OverridePrim(path);
          }
          if (!prim) {
            return;
          }
          m_primMap.Add(path, prim);
        }
      }
      m_usdIo.Serialize(sample, prim, timeCode);
    }

    /// <summary>
    /// Returns the entire scene serialized as a string. Expensive for large scenes.
    /// </summary>
    override public string ToString() {
      string s;
      m_stage.ExportToString(out s, false);
      return s;
    }

#region "Private API"
    // ----------------------------------------------------------------------------------------- //
    // Private API
    // ----------------------------------------------------------------------------------------- //

    /// <summary>
    /// Translates a string path to an SdfPath, caching the result to avoid churn.
    /// </summary>
    private pxr.SdfPath GetSdfPath(string path) {
      if (m_pathMap.ContainsKey(path)) {
        return m_pathMap[path];
      }
      var p = new pxr.SdfPath(path);
      m_pathMap.Add(path, p);
      return p;
    }

    private pxr.SdfPath GetSdfPath(pxr.SdfPath path) {
      throw new ApplicationException("TODO: don't allow implicit conversion path -> string");
    }

    private pxr.UsdPrim GetUsdPrim(string path) {
      return GetUsdPrim(GetSdfPath(path));
    }

    private pxr.UsdPrim GetUsdPrim(SdfPath path) {
      UsdPrim prim;
      if (!m_primMap.TryGetValue(path, out prim) || !prim.IsValid()) {
        prim = Stage.GetPrimAtPath(path);
        m_primMap.Add(path, prim);
      }
      return prim;
    }

    /// <summary>
    /// Constructor declared private to force access through the static factories.
    /// </summary>
    private Scene(UsdStage stage) {
      if (stage == null) {
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
    private Dictionary<SdfPath, pxr.UsdPrim> m_primMap = new Dictionary<SdfPath, UsdPrim>();
    private object m_stageLock = new object();
    private UsdIo m_usdIo;
    private UsdStage m_stage;
    private BackgroundExecutor m_bgExe = new BackgroundExecutor();

    // Cache TfTokens for reuse to avoid P/Invoke and token churn.
    private static readonly TfToken kUpAxisToken = new TfToken("upAxis");
    private static readonly TfToken kYUpToken = new TfToken("Y");
  }
}
