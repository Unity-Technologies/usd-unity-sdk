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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using USD.NET;
using USD.NET.Unity;

namespace USD.NET.Examples {

  /// <remarks>
  /// Export Mesh Example
  /// 
  ///  * StartRecording:
  ///    * Create and configure a USD scene.
  ///    * Traverse the Unity scene, for each GameObject:
  ///      * Create an association between the Unity object and a USD prim.
  ///      * Assign an ExportFunction that will export the data for the object.
  ///      
  ///  * Export unvarying data:
  ///    * Export mesh topology and any other data that doesn't change from frame-to-frame.
  ///  
  ///  * On Update, export time-varying data:
  ///    * Traverse the map of GameObjects, for each object:
  ///      * Call the associated export function.
  ///  
  ///  * StopRecording:
  ///    * Save and close the USD scene.
  ///    * Release the association map and USD scene.
  /// </remarks>
  public class ExportMeshExample : MonoBehaviour {

    // The root GameObject to export to USD.
    public GameObject m_exportRoot;
    public int m_curFrame;

    // The number of frames to capture after hitting record;
    [Range(1, 500)]
    public int m_frameCount = 100;

    // The path to where the USD file will be written.
    // If null/empty, the file will be created in memory only.
    public string m_usdFile;

    // The scene object to which the recording will be saved.
    private Scene m_usdScene;
    private int m_startFrame;

    // The export function allows for dispatch to different export functions without knowing what
    // type of data they export (e.g. mesh vs. transform).
    public delegate void ExportFunction(ObjectContext objContext, ExportContext exportContext);

    delegate void ObjectProcessor(GameObject go,
                                  ExportContext context);

    public struct ObjectContext {
      public GameObject gameObject;
      public string path;
      public SampleBase sample;
    }

    public class ExportContext {
      public Scene scene;
      public BasisTransformation basisTransform = BasisTransformation.FastAndDangerous;
      public Dictionary<GameObject, ExportPlan> plans = new Dictionary<GameObject, ExportPlan>();
      public Dictionary<Material, string> matMap = new Dictionary<Material, string>();
      public Dictionary<Transform, Transform[]> skelMap = new Dictionary<Transform, Transform[]>();
      public Dictionary<Transform, Matrix4x4> bones = new Dictionary<Transform, Matrix4x4>();

      // Sample object instances, shared across multiple export methods.
      public Dictionary<Type, SampleBase> samples = new Dictionary<Type, SampleBase>();
    }

    public class Exporter {
      // The sample type to be used when exporting.
      public SampleBase sample;

      // The export function which implements the logic to populate the sample.
      public ExportFunction exportFunc;
    }

    // An export plan will be created for each path in the scene. Each ExportPlan will use one of
    // the fixed export functions. For example, when setting up export for a mesh, an ExportPlan
    // will be created for that path in the scenegraph and the ExportFunction will the one which is
    // capable of exporting a mesh.
    public class ExportPlan {
      // The USD path at which the Unity data will be written.
      public string path;

      // The functions to run when exporting this object.
      public List<Exporter> exporters = new List<Exporter>();
    }

    ExportContext m_context = new ExportContext();

    // Used by the custom editor to determine recording state.
    public bool IsRecording
    {
      get;
      private set;
    }

    // ------------------------------------------------------------------------------------------ //
    // Recording Control.
    // ------------------------------------------------------------------------------------------ //

    public void StartRecording() {
      if (IsRecording) { return; }

      if (!m_exportRoot) {
        Debug.LogError("ExportRoot not assigned.");
        return;
      }

      if (m_usdScene != null) {
        m_usdScene.Close();
        m_usdScene = null;
      }

      try {
        if (string.IsNullOrEmpty(m_usdFile)) {
          m_usdScene = USD.NET.Scene.Create();
        } else {
          m_usdScene = Scene.Create(m_usdFile);
        }

        // USD operates on frames, so the frame rate is required for playback.
        // We could also set this to 1 to indicate that the TimeCode is in seconds.
        Application.targetFrameRate = 60;
        m_usdScene.FrameRate = Application.targetFrameRate;

        m_usdScene.StartTime = 0;
        m_usdScene.EndTime = m_frameCount;

        // For simplicity in this example, adding game objects while recording is not supported.
        Debug.Log("Init hierarchy");
        m_context = new ExportContext();
        m_context.scene = m_usdScene;
        InitHierarchy(m_exportRoot, m_context);

        // Do this last, in case an exception is thrown above.
        IsRecording = true;

        // Set the start frame and add one because the button event fires after update, so the first
        // frame update sees while recording is (frameCount + 1).
        m_startFrame = Time.frameCount + 1;
      } catch {
        if (m_usdScene != null) {
          m_usdScene.Close();
          m_usdScene = null;
        }
        throw;
      }
    }

    public void StopRecording() {
      if (!IsRecording) { return; }

      m_context = new ExportContext();

      // In a real exporter, additional error handling should be added here.
      if (!string.IsNullOrEmpty(m_usdFile)) {
        // We could use SaveAs here, which is fine for small scenes, though it will require
        // everything to fit in memory and another step where that memory is copied to disk.
        m_usdScene.Save();
      }

      // Release memory associated with the scene.
      m_usdScene.Close();
      m_usdScene = null;

      IsRecording = false;
    }

    // ------------------------------------------------------------------------------------------ //
    // Unity Behavior Events.
    // ------------------------------------------------------------------------------------------ //

    void Awake() {
      // Init USD.
      InitUsd.Initialize();
    }

    void Update() {
      if (!IsRecording) { return; }

      // On the first frame, export all the unvarying data (e.g. mesh topology).
      // On subsequent frames, skip unvarying data to avoid writing redundant data.
      if (Time.frameCount == m_startFrame) {
        // First write materials.
        foreach (var kvp in m_context.matMap) {
          ExportMaterial(m_usdScene, kvp.Key, kvp.Value);
        }
        // Next, write geometry, which may also bind to materials written above.
        foreach (var kvp in m_context.plans) {
          ExportPlan exportPlan = kvp.Value;
          GameObject go = kvp.Key;
          foreach (var exporter in kvp.Value.exporters) {
            exporter.exportFunc(
                new ObjectContext { gameObject=go,
                                    path = kvp.Value.path,
                                    sample = exporter.sample },
                m_context);
          }
        }
      }

      // Set the time at which to read samples from USD.
      // If the FramesPerSecond is set to 1 above, this should be Time.time instead of frame count.
      m_usdScene.Time = Time.frameCount - m_startFrame;
      m_curFrame = (int)m_usdScene.Time.Value;

      // Exit once we've recorded all frames.
      if (m_usdScene.Time > m_frameCount) {
        StopRecording();
        return;
      }

      // Record the time varying data that changes from frame to frame.
      foreach (var kvp in m_context.plans) {
        ExportPlan exportPlan = kvp.Value;
        GameObject go = kvp.Key;
        foreach (var exporter in kvp.Value.exporters) {
          exporter.exportFunc(
              new ObjectContext {
                gameObject = go,
                path = kvp.Value.path,
                sample = exporter.sample
              },
              m_context);
        }
      }
    }

    public static void Export(GameObject root,
                              USD.NET.Scene scene,
                              BasisTransformation basisTransform) {
      var context = new ExportContext();

      context.scene = scene;
      context.basisTransform = basisTransform;

      InitHierarchy(root, context);
      var oldTime = scene.Time;

      foreach (var kvp in context.matMap) {
        scene.Time = null;
        ExportMaterial(scene, kvp.Key, kvp.Value);
      }

      foreach (var kvp in context.plans) {
        ExportPlan exportPlan = kvp.Value;
        GameObject go = kvp.Key;
        string path = exportPlan.path;

        foreach (Exporter exporter in exportPlan.exporters) {
          scene.Time = null;
          SampleBase sample = exporter.sample;
          var objCtx = new ObjectContext { gameObject = go, path = exportPlan.path, sample = sample };

          exporter.exportFunc(objCtx, context);
          context.scene.Time = oldTime;
          exporter.exportFunc(objCtx, context);

          if (!go.gameObject.activeSelf) {
            var im = new pxr.UsdGeomImageable(scene.GetPrimAtPath(exportPlan.path));
            if (im) {
              im.CreateVisibilityAttr().Set(pxr.UsdGeomTokens.invisible);
            }
          }
        }
      }
    }

    // ------------------------------------------------------------------------------------------ //
    // Init Hierarchy.
    // ------------------------------------------------------------------------------------------ //

    static void Traverse(GameObject obj,
                         ObjectProcessor processor,
                         ExportContext context) {
      processor(obj, context);
      foreach (Transform child in obj.transform) {
        Traverse(child.gameObject, processor, context);
      }
    }

    static void AccumNestedBones(Transform curXf,
                                 List<Transform> children,
                                 ExportContext ctx) {
      if (ctx.bones.ContainsKey(curXf)) {
        children.Add(curXf);
      }
      foreach (Transform child in curXf.transform) {
        AccumNestedBones(child, children, ctx);
      }
    }

    static SampleBase CreateOne<T>(ExportContext context) where T: SampleBase, new() {
      SampleBase sb;
      if (context.samples.TryGetValue(typeof(T), out sb)) {
        return sb;
      }

      sb = (new T());
      context.samples[typeof(T)] = sb;
      return sb;
    }

    static void InitHierarchy(GameObject exportRoot,
                              ExportContext context) {
      Traverse(exportRoot, InitExportableObjects, context);

      foreach (Transform rootBone in context.skelMap.Keys.ToList()) {
        var children = new List<Transform>();
        var meshes = new List<GameObject>();
        CreateExportPlan(rootBone.gameObject, CreateOne<SkeletonSample>(context), ExportSkeleton, context);
        //CreateExportPlan(rootBone.gameObject, new SkeletonSample(), ExportSkeletonAnimation, context.plans);
        AccumNestedBones(rootBone, children, context);
        context.skelMap[rootBone] = children.ToArray();
      }
    }

    static void InitExportableObjects(GameObject go,
                                      ExportContext context) {
      var smr = go.GetComponent<SkinnedMeshRenderer>();
      if (smr != null) {
        foreach (var mat in smr.sharedMaterials) {
          if (!context.matMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name + "_" + mat.GetInstanceID().ToString());
            context.matMap.Add(mat, usdPath);
          }
        }
        CreateExportPlan(go, CreateOne<MeshSample>(context), ExportSkinnedMesh, context);
        if (smr.rootBone == null) {
          Debug.LogWarning("No root bone at: " + UnityTypeConverter.GetPath(go.transform));
        } else if (smr.bones == null || smr.bones.Length == 0) {
          Debug.LogWarning("No bones at: " + UnityTypeConverter.GetPath(go.transform));
        } else {
          MergeBones(smr.rootBone, smr.bones, smr.sharedMesh.bindposes, context);
        }
      } else if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshRenderer>() != null) {
        var mr = go.GetComponent<MeshRenderer>();
        foreach (var mat in mr.sharedMaterials) {
          if (mat == null) {
            continue;
          }
          if (!context.matMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name + "_" + mat.GetInstanceID().ToString());
            context.matMap.Add(mat, usdPath);
          }
        }
        CreateExportPlan(go, CreateOne<MeshSample>(context), ExportMesh, context);
      } else if (go.GetComponent<Camera>()) {
        CreateExportPlan(go, CreateOne<CameraSample>(context), ExportCamera, context);
      }
    }

    static void MergeBones(Transform rootBone, Transform[] bones, Matrix4x4[] bindPoses, ExportContext context) {
      if (!context.bones.ContainsKey(rootBone) && !context.skelMap.ContainsKey(rootBone)) {
        context.skelMap.Add(rootBone, bones);
      }
      for (int i = 0; i < bones.Length; i++) {
        Transform bone = bones[i];
        context.bones[bone] = bindPoses[i];
        if (bone == rootBone) {
          continue;
        }
        if (context.skelMap.ContainsKey(bone)) {
          context.skelMap.Remove(bone);
        }
      }
    }

    static void CreateExportPlan(GameObject go,
                                 SampleBase sample,
                                 ExportFunction exportFunc,
                                 ExportContext context) {
      // This is an exportable object.
      string path = Unity.UnityTypeConverter.GetPath(go.transform);
      if (!context.plans.ContainsKey(go)) {
        context.plans.Add(go, new ExportPlan { path = path });
      }

      context.plans[go].exporters.Add(new Exporter { exportFunc = exportFunc, sample = sample });

      // Include the parent xform hierarchy.
      // Note that the parent hierarchy is memoised, so despite looking expensive, the time
      // complexity is linear.
      Transform xf = go.transform.parent;
      if (xf != null && !context.plans.ContainsKey(xf.gameObject)) {
        // Since all GameObjects have a Transform, export all un-exported parents as transform.
        CreateExportPlan(xf.gameObject, CreateOne<XformSample>(context), ExportXform, context);
      }
    }

    // ------------------------------------------------------------------------------------------ //
    // Type-Specific (Mesh, Xform, Camera) Exporters.
    // ------------------------------------------------------------------------------------------ //
    static void ExportMaterial(Scene scene, Material mat, string usdMaterialPath) {
      string shaderPath = usdMaterialPath + "/PreviewSurface";

      var material = new USD.NET.Unity.MaterialSample();
      material.surface.SetConnectedPath(shaderPath, "outputs:surface");

      scene.Write(usdMaterialPath, material);

      var shader = new PreviewSurfaceSample();
      var texPath = /*TODO: this should be explicit*/
            System.IO.Path.GetDirectoryName(scene.Stage.GetRootLayer().GetIdentifier());

      if (mat.shader.name == "Standard (Specular setup)") {
        StandardShaderIo.ExportStandardSpecular(scene, shaderPath, mat, shader, texPath);
      } else if (mat.shader.name == "Standard (Roughness setup)") {
        StandardShaderIo.ExportStandardRoughness(scene, shaderPath, mat, shader, texPath);
      } else if (mat.shader.name == "Standard") {
        StandardShaderIo.ExportStandard(scene, shaderPath, mat, shader, texPath);
      } else if (mat.shader.name == "HDRenderPipeline/Lit") {
        HdrpShaderIo.ExportLit(scene, shaderPath, mat, shader, texPath);
      } else {
        StandardShaderIo.ExportGeneric(scene, shaderPath, mat, shader, texPath);
      }

      scene.Write(shaderPath, shader);
      scene.GetPrimAtPath(shaderPath).CreateAttribute(pxr.UsdShadeTokens.outputsSurface,
                                                      SdfValueTypeNames.Token,
                                                      false,
                                                      pxr.SdfVariability.SdfVariabilityUniform);
    }

    static Matrix4x4 ComputeWorldXf(Transform curBone, ExportContext context) {
      if (!context.bones.ContainsKey(curBone)) {
        return curBone.parent.localToWorldMatrix;
      }
      return context.bones[curBone] * ComputeWorldXf(curBone.parent, context);
    }

    static void ExportSkeleton(ObjectContext objContext, ExportContext exportContext) {
      var scene = exportContext.scene;
      var sample = (SkeletonSample)objContext.sample;
      var bones = exportContext.skelMap[objContext.gameObject.transform];
      sample.joints = new string[bones.Length];
      sample.bindTransforms = new Matrix4x4[bones.Length];
      sample.restTransforms = new Matrix4x4[bones.Length];

      var sb = new System.Text.StringBuilder();
      string rootPath = UnityTypeConverter.GetPath(objContext.gameObject.transform);
      sb.AppendLine(rootPath);

      int i = 0;
      foreach (Transform bone in bones) {
        var bonePath = UnityTypeConverter.GetPath(bone);
        sb.AppendLine(bonePath);
        sb.AppendLine(exportContext.bones[bone].ToString());
        sb.AppendLine();

        sample.joints[i] = bonePath;
        sample.bindTransforms[i] = exportContext.bones[bone].inverse;
        sample.restTransforms[i] = GetLocalTransformMatrix(bone, false, false, exportContext.basisTransform);

        if (exportContext.basisTransform == BasisTransformation.SlowAndSafe) {
          sample.bindTransforms[i] = UnityTypeConverter.ChangeBasis(sample.bindTransforms[i]);
          // The restTransforms will get a change of basis from GetLocalTransformMatrix().
        }

        i++;
      }

      Debug.Log(sb.ToString());
      scene.Write(objContext.path, sample);

      //var im = new pxr.UsdGeomImageable(scene.GetPrimAtPath(exportPlan.path));
      //im.CreatePurposeAttr().Set(pxr.UsdGeomTokens.guide);

      var parent = scene.GetPrimAtPath(new pxr.SdfPath(objContext.path).GetParentPath());
      parent.CreateRelationship(new pxr.TfToken("skel:skeleton")).AddTarget(new pxr.SdfPath(objContext.path));
      parent.SetTypeName(new pxr.TfToken("SkelRoot"));
      //scene.Write(new pxr.SdfPath(exportPlan.path).GetParentPath(), new SkelRootSample());
    }

    static void ExportSkelAnimation(ObjectContext objContext, ExportContext exportContext) {
      var scene = exportContext.scene;
      var sample = (SkeletonSample)objContext.sample;
      var go = objContext.gameObject;
      var bones = exportContext.skelMap[go.transform];
      sample.joints = new string[bones.Length];
      sample.bindTransforms = new Matrix4x4[bones.Length];
      sample.restTransforms = new Matrix4x4[bones.Length];

      var sb = new System.Text.StringBuilder();
      string rootPath = UnityTypeConverter.GetPath(go.transform);
      sb.AppendLine(rootPath);
      int i = 0;
      foreach (Transform b in bones) {
        var bonePath = UnityTypeConverter.GetPath(b);
        sb.AppendLine(bonePath);
        sample.joints[i] = bonePath;
        if (exportContext.basisTransform == BasisTransformation.SlowAndSafe) {
          sample.bindTransforms[i] = UnityTypeConverter.ChangeBasis(b.localToWorldMatrix);
        } else {
          sample.bindTransforms[i] = b.localToWorldMatrix;
        }
        sample.restTransforms[i] = GetLocalTransformMatrix(b, false, false, exportContext.basisTransform);
        i++;
      }

      Debug.Log(sb.ToString());
      scene.Write(objContext.path, sample);
      var parent = scene.GetPrimAtPath(new pxr.SdfPath(objContext.path).GetParentPath());
      parent.CreateRelationship(new pxr.TfToken("skel:skeleton")).AddTarget(new pxr.SdfPath(objContext.path));
      parent.SetTypeName(new pxr.TfToken("SkelRoot"));
      //scene.Write(new pxr.SdfPath(exportPlan.path).GetParentPath(), new SkelRootSample());
    }

    static void ExportSkinnedMesh(ObjectContext objContext, ExportContext exportContext) {
      var smr = objContext.gameObject.GetComponent<SkinnedMeshRenderer>();
      Mesh mesh = smr.sharedMesh;

      // TODO: export smr.sharedMesh when unvarying.
      // Ugh. Note that BakeMesh bakes the parent transform into the points, which results in
      // compounded transforms on export. The way Unity handles this is to apply a scale as part
      // of the importer, which bakes the scale into the points.
#if false
      mesh = new Mesh();
      smr.BakeMesh(mesh);
#endif
      ExportMesh(objContext, exportContext, mesh, smr.sharedMaterial, smr.sharedMaterials);

      // Note that the baked mesh no longer has the bone weights, so here we switch back to the
      // shared SkinnedMeshRenderer mesh.
      ExportSkelWeights(exportContext.scene, objContext.path, smr.sharedMesh, smr.bones);
    }

    static void ExportSkelWeights(Scene scene, string path, Mesh unityMesh, Transform[] bones) {
      var sample = new SkelBindingSample();
      sample.jointIndices = new int[unityMesh.boneWeights.Length * 4];
      sample.jointWeights = new float[unityMesh.boneWeights.Length * 4];
      sample.geomBindTransform = Matrix4x4.identity;
      sample.joints = new string[bones.Length];

      int b = 0;
      foreach (Transform bone in bones) {
        sample.joints[b++] = UnityTypeConverter.GetPath(bone);
      }

      int i = 0;
      int w = 0;
      foreach (var bone in unityMesh.boneWeights) {
        sample.jointIndices[i++] = bone.boneIndex0;
        sample.jointIndices[i++] = bone.boneIndex1;
        sample.jointIndices[i++] = bone.boneIndex2;
        sample.jointIndices[i++] = bone.boneIndex3;
        sample.jointWeights[w++] = bone.weight0;
        sample.jointWeights[w++] = bone.weight1;
        sample.jointWeights[w++] = bone.weight2;
        sample.jointWeights[w++] = bone.weight3;
      }

      scene.Write(path, sample);
      var prim = scene.GetPrimAtPath(path);

      var attrIndices = new pxr.UsdGeomPrimvar(
                            prim.CreateAttribute(
                                pxr.UsdSkelTokens.primvarsSkelJointIndices,
                                SdfValueTypeNames.IntArray));
      attrIndices.SetElementSize(4);

      var attrWeights = new pxr.UsdGeomPrimvar(
                            prim.CreateAttribute(
                                pxr.UsdSkelTokens.primvarsSkelJointWeights,
                                SdfValueTypeNames.FloatArray));
      attrWeights.SetElementSize(4);

      var attrGeomBindXf = new pxr.UsdGeomPrimvar(
                      prim.CreateAttribute(
                          pxr.UsdSkelTokens.primvarsSkelGeomBindTransform,
                          SdfValueTypeNames.Matrix4d));
      attrGeomBindXf.SetInterpolation(pxr.UsdGeomTokens.constant);
    }

    static void ExportMesh(ObjectContext objContext, ExportContext exportContext) {
      MeshFilter mf = objContext.gameObject.GetComponent<MeshFilter>();
      MeshRenderer mr = objContext.gameObject.GetComponent<MeshRenderer>();
      Mesh mesh = mf.sharedMesh;
      ExportMesh(objContext, exportContext, mesh, mr.sharedMaterial, mr.sharedMaterials);
    }

    static void ExportMesh(ObjectContext objContext,
                   ExportContext exportContext,
                   Mesh mesh,
                   Material sharedMaterial,
                   Material[] sharedMaterials) {
      string path = objContext.path;
      if (mesh == null) {
        Debug.LogWarning("Null mesh for: " + path);
        return;
      }
      var scene = exportContext.scene;
      bool unvarying = scene.Time == null;
      bool slowAndSafeConversion = exportContext.basisTransform == BasisTransformation.SlowAndSafe;
      var sample = (MeshSample)objContext.sample;
      var go = objContext.gameObject;

      if (slowAndSafeConversion) {
        // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
        // basis is required. There are shortcuts, but this is fully general.
        sample.ConvertTransform();
      }

      // Only export the mesh topology on the first frame.
      if (unvarying) {
        // TODO: Technically a mesh could be the root transform, which is not handled correctly here.
        // It should ahve the same logic for root prims as in ExportXform.
        sample.transform = GetLocalTransformMatrix(go.transform,
                                                   scene.UpAxis == Scene.UpAxes.Z,
                                                   new pxr.SdfPath(path).IsRootPrimPath(),
                                                   exportContext.basisTransform);

        sample.normals = mesh.normals;
        sample.points = mesh.vertices;
        sample.tangents = mesh.tangents;
        sample.extent = mesh.bounds;
        if (mesh.bounds.center == Vector3.zero && mesh.bounds.extents == Vector3.zero) {
          mesh.RecalculateBounds();
          sample.extent = mesh.bounds;
        }
        sample.colors = mesh.colors;

        if (sample.colors == null || sample.colors.Length == 0 && sharedMaterial.HasProperty("_Color")) {
          sample.colors = new Color[1];
          sample.colors[0] = sharedMaterial.color.linear;
        }

        // Gah. There is no way to inspect a meshes UVs.
        sample.st = mesh.uv;

        // Set face vertex counts and indices.
        var tris = mesh.triangles;

        if (slowAndSafeConversion) {
          // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change
          // of basis is required. There are shortcuts, but this is fully general.
          sample.extent.center = UnityTypeConverter.ChangeBasis(sample.extent.center);

          for (int i = 0; i < sample.points.Length; i++) {
            sample.points[i] = UnityTypeConverter.ChangeBasis(sample.points[i]);
            if (sample.normals != null && sample.normals.Length == sample.points.Length) {
              sample.normals[i] = UnityTypeConverter.ChangeBasis(sample.normals[i]);
            }
          }

          for (int i = 0; i < tris.Length; i += 3) {
            var t = tris[i];
            tris[i] = tris[i + 1];
            tris[i + 1] = t;
          }
        }

        sample.SetTriangles(tris);

        Debug.Log("Write unvarying: " + path);
        scene.Write(path, sample);

        // TODO: this is a bit of a half-measure, we need real support for primvar interpolation.
        // Set interpolation based on color count.
        if (sample.colors != null && sample.colors.Length == 1) {
          pxr.UsdPrim usdPrim = scene.GetPrimAtPath(path);
          var colorPrimvar = new pxr.UsdGeomPrimvar(usdPrim.GetAttribute(pxr.UsdGeomTokens.primvarsDisplayColor));
          colorPrimvar.SetInterpolation(pxr.UsdGeomTokens.constant);
          var opacityPrimvar = new pxr.UsdGeomPrimvar(usdPrim.GetAttribute(pxr.UsdGeomTokens.primvarsDisplayOpacity));
          opacityPrimvar.SetInterpolation(pxr.UsdGeomTokens.constant);
        }

        string usdMaterialPath;

        if (!exportContext.matMap.TryGetValue(sharedMaterial, out usdMaterialPath)) {
          Debug.LogError("Invalid material bound for: " + path);
        } else {
          MaterialSample.Bind(scene, path, usdMaterialPath);
        }

        // In USD subMeshes are represented as UsdGeomSubsets.
        // When there are multiple subMeshes, convert them into UsdGeomSubsets.
        if (mesh.subMeshCount > 1) {
          // Build a table of face indices, used to convert the subMesh triangles to face indices.
          var faceTable = new Dictionary<Vector3, int>();
          for (int i = 0; i < tris.Length; i += 3) {
            if (!slowAndSafeConversion) {
              faceTable.Add(new Vector3(tris[i], tris[i + 1], tris[i + 2]), i / 3);
            } else {
              // Under slow and safe export, index 0 and 1 are swapped.
              // This swap will not be present in the subMesh indices, so must be undone here.
              faceTable.Add(new Vector3(tris[i + 1], tris[i], tris[i + 2]), i / 3);
            }
          }

          var usdPrim = scene.GetPrimAtPath(path);
          var usdGeomMesh = new pxr.UsdGeomMesh(usdPrim);
          // Process each subMesh and create a UsdGeomSubset of faces this subMesh targets.
          for (int si = 0; si < mesh.subMeshCount; si++) {
            int[] indices = mesh.GetTriangles(si);
            int[] faceIndices = new int[indices.Length / 3];

            for (int i = 0; i < indices.Length; i += 3) {
              faceIndices[i / 3] = faceTable[new Vector3(indices[i], indices[i + 1], indices[i + 2])];
            }

            var materialBindToken = new pxr.TfToken("materialBind");
            var vtIndices = UnityTypeConverter.ToVtArray(faceIndices);
            var subset = pxr.UsdGeomSubset.CreateUniqueGeomSubset(
                usdGeomMesh,            // The object of which this subset belongs.
                "subMeshes",            // An arbitrary name for the subset.
                pxr.UsdGeomTokens.face, // Indicator that these represent face indices
                vtIndices,              // The actual face indices.
                materialBindToken       // familyName = "materialBind"
                );

            if (si >= sharedMaterials.Length || !exportContext.matMap.TryGetValue(sharedMaterials[si], out usdMaterialPath)) {
              Debug.LogError("Invalid material bound for: " + path);
            } else {
              MaterialSample.Bind(scene, subset.GetPath(), usdMaterialPath);
            }
          }
        }
      } else {
        // Only write the transform when animating.
        var xfSample = new XformSample();
        xfSample.transform = GetLocalTransformMatrix(go.transform,
                                                     scene.UpAxis == Scene.UpAxes.Z,
                                                     new pxr.SdfPath(path).IsRootPrimPath(),
                                                     exportContext.basisTransform);
        Debug.Log("Write varying: " + path);
        scene.Write(path, xfSample);
      }
    }

    static void ExportCamera(ObjectContext objContext, ExportContext exportContext) {
      CameraSample sample = (CameraSample)objContext.sample;
      Camera camera = objContext.gameObject.GetComponent<Camera>();
      var path = objContext.path;
      var scene = exportContext.scene;
      bool fastConvert = exportContext.basisTransform == BasisTransformation.FastAndDangerous;

      // If doing a fast conversion, do not let the constructor do the change of basis for us.
      sample.CopyFromCamera(camera, convertTransformToUsd: !fastConvert);

      if (fastConvert) {
        // Partial change of basis.
        var basisChange = UnityEngine.Matrix4x4.identity;
        // Invert the forward vector.
        basisChange[2, 2] = -1;
        // Full change of basis would be b*t*b-1, but here we're placing only a single inversion
        // at the root of the hierarchy, so all we need to do is get the camera into the same
        // space.
        sample.transform = sample.transform * basisChange;

        // Is this also a root path?
        // If so the partial basis conversion must be completed on the camera itself.
        if (path.LastIndexOf("/") == 0) {
          sample.transform = basisChange * sample.transform;
        }
      }

      scene.Write(path, sample);
    }

    static void ExportXform(ObjectContext objContext, ExportContext exportContext) {
      XformSample sample = (XformSample)objContext.sample;
      var localRot = objContext.gameObject.transform.localRotation;
      var localScale = objContext.gameObject.transform.localScale;
      var path = new pxr.SdfPath(objContext.path);

      // If exporting for Z-Up, rotate the world.
      bool correctZUp = exportContext.scene.UpAxis == Scene.UpAxes.Z;
      sample.transform = GetLocalTransformMatrix(objContext.gameObject.transform,
                                                 correctZUp,
                                                 path.IsRootPrimPath(),
                                                 exportContext.basisTransform);
      exportContext.scene.Write(objContext.path, sample);
    }

    static Matrix4x4 GetLocalTransformMatrix(Transform tr,
                                             bool correctZUp,
                                             bool isRootPrim,
                                             BasisTransformation conversionType) {
      var localRot = tr.localRotation;
      bool fastConvert = conversionType == BasisTransformation.FastAndDangerous;

      if (correctZUp && isRootPrim) {
        float invert = fastConvert ? 1 : -1;
        localRot = localRot * Quaternion.AngleAxis(invert * 90, Vector3.right);
      }

      var mat = Matrix4x4.TRS(tr.localPosition, localRot, tr.localScale);

      // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
      // basis is required. There are shortcuts, but this is fully general.
      //
      // Here we can either put a partial conversion at the root (fast & dangerous) or convert the
      // entire hierarchy, along with the points, normals and triangle winding. The benefit of the
      // full conversion is that there are no negative scales left in the hierarchy.
      //
      // Note that this is the correct partial conversion for the root transforms, however the
      // camera and light matrices must contain the other half of the conversion
      // (e.g. mat * basisChangeInverse).
      if (fastConvert && isRootPrim) {
        // Partial change of basis.
        var basisChange = Matrix4x4.identity;
        // Invert the forward vector.
        basisChange[2, 2] = -1;
        mat = basisChange * mat;
      } else if (!fastConvert) {
        // Full change of basis.
        mat = UnityTypeConverter.ChangeBasis(mat);
      }

      return mat;
    }
  }
}