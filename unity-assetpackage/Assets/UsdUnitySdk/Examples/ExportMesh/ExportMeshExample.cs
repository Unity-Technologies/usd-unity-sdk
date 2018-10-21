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
    public delegate void ExportFunction(
        Scene scene,
        GameObject go,
        ExportPlan exportPlan,
        Dictionary<Material, string> matMap);

    // An export plan will be created for each path in the scene. Each ExportPlan will use one of
    // the fixed export functions. For example, when setting up export for a mesh, an ExportPlan
    // will be created for that path in the scenegraph and the ExportFunction will the one which is
    // capable of exporting a mesh.
    public struct ExportPlan {
      // The USD path at which the Unity data will be written.
      public string path;

      // The sample which will hold the data to export.
      public SampleBase sample;

      // The export function which implements the logic to populate the sample.
      public ExportFunction exportFunc;

      // Do a deep/safe conversion or a fast/dangerous conversion.
      public BasisTransformation convertHandedness;
    }

    // A map from Unity GameObject to an export plan, which indicates where and how the GameObject
    // should be written to USD.
    private Dictionary<GameObject, ExportPlan> m_primMap
      = new Dictionary<GameObject, ExportPlan>();

    private Dictionary<Material, string> m_materialMap
      = new Dictionary<Material, string>();

    // Used by the custom editor to determine recording state.
    public bool IsRecording {
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
        m_primMap.Clear();
        InitHierarchy(m_exportRoot, m_materialMap, m_primMap);

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

      m_primMap.Clear();

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
        foreach (var kvp in m_materialMap) {
          ExportMaterial(m_usdScene, kvp.Key, kvp.Value);
        }
        // Next, write geometry, which may also bind to materials written above.
        foreach (var kvp in m_primMap) {
          ExportPlan exportPlan = kvp.Value;
          GameObject go = kvp.Key;
          exportPlan.exportFunc(m_usdScene, go, exportPlan, m_materialMap);
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
      foreach (var kvp in m_primMap) {
        ExportPlan exportPlan = kvp.Value;
        GameObject go = kvp.Key;
        exportPlan.exportFunc(m_usdScene, go, exportPlan, m_materialMap);
      }
    }

    public static void Export(GameObject root, USD.NET.Scene scene) {
      var primMap = new Dictionary<GameObject, ExportPlan>();
      var matMap = new Dictionary<Material, string>();
      InitHierarchy(root, matMap, primMap);
      var oldTime = scene.Time;

      foreach (var kvp in matMap) {
        scene.Time = null;
        ExportMaterial(scene, kvp.Key, kvp.Value);
      }

      foreach (var kvp in primMap) {
        ExportPlan exportPlan = kvp.Value;
        GameObject go = kvp.Key;

        scene.Time = null;
        exportPlan.exportFunc(scene, go, exportPlan, matMap);
        scene.Time = oldTime;
        exportPlan.exportFunc(scene, go, exportPlan, matMap);

        if (!go.activeSelf) {
          scene.GetPrimAtPath(exportPlan.path).SetActive(false);
        }
      }
    }

    // ------------------------------------------------------------------------------------------ //
    // Init Hierarchy.
    // ------------------------------------------------------------------------------------------ //

    delegate void ObjectProcessor(GameObject go,
                                  Dictionary<Material, string> matMap,
                                  Dictionary<GameObject, ExportPlan> primMap);

    static void Traverse(GameObject obj,
                         ObjectProcessor processor,
                         Dictionary<Material, string> matMap,
                         Dictionary<GameObject, ExportPlan> primMap) {
      processor(obj, matMap, primMap);
      foreach (Transform child in obj.transform) {
        Traverse(child.gameObject, processor, matMap, primMap);
      }
    }

    static void InitHierarchy(GameObject exportRoot,
                              Dictionary<Material, string> matMap,
                              Dictionary<GameObject, ExportPlan> primMap) {
      Traverse(exportRoot, InitExportableObjects, matMap, primMap);
    }

    static void InitExportableObjects(GameObject go,
                                      Dictionary<Material, string> matMap,
                                      Dictionary<GameObject, ExportPlan> primMap) {
      SampleBase sample = null;
      ExportFunction exportFunc = null;

      if (go.GetComponent<SkinnedMeshRenderer>() != null) {
        sample = new MeshSample();
        exportFunc = ExportSkinnedMesh;
        foreach (var mat in go.GetComponent<SkinnedMeshRenderer>().sharedMaterials) {
          if (!matMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name + "_" + mat.GetInstanceID().ToString());
            matMap.Add(mat, usdPath);
          }
        }
      } else if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshRenderer>() != null) {
        sample = new MeshSample();
        exportFunc = ExportMesh;
        foreach (var mat in go.GetComponent<MeshRenderer>().sharedMaterials) {
          if (!matMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name + "_" + mat.GetInstanceID().ToString());
            matMap.Add(mat, usdPath);
          }
        }
      } else if (go.GetComponent<Camera>()) {
        sample = new CameraSample();
        exportFunc = ExportCamera;
      } else {
        return;
      }

      // This is an exportable object.
      string path = Unity.UnityTypeConverter.GetPath(go.transform);
      primMap.Add(go, new ExportPlan {
        path = path,
        sample = sample,
        exportFunc = exportFunc,
        convertHandedness = BasisTransformation.FastAndDangerous
      });

      // Include the parent xform hierarchy.
      // Note that the parent hierarchy is memoised, so despite looking expensive, the time
      // complexity is linear.
      Transform xf = go.transform.parent;
      while (xf) {
        if (!InitExportableParents(xf.gameObject, primMap)) {
          break;
        }
        xf = xf.parent;
      }
    }

    static bool InitExportableParents(GameObject go,
                           Dictionary<GameObject, ExportPlan> primMap) {
      if (primMap.ContainsKey(go)) {
        // Stop processing parents, this keeps the performance of the traversal linear.
        return false;
      }

      // Any object we add will only be exported as an Xform.
      string path = UnityTypeConverter.GetPath(go.transform);
      SampleBase sample = new XformSample();
      primMap.Add(go, new ExportPlan { path = path, sample = sample, exportFunc = ExportXform });

      // Continue processing parents.
      return true;
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

      // HDRenderPipeline/Lit
      if (mat.shader.name == "Standard (Specular setup)") {
        StandardShaderIo.ExportStandardSpecular(scene, shaderPath, mat, shader, texPath);
        scene.Write(shaderPath, shader);
        return;
      }
      if (mat.shader.name == "Standard (Roughness setup)") {
        StandardShaderIo.ExportStandardRoughness(scene, shaderPath, mat, shader, texPath);
        scene.Write(shaderPath, shader);
        return;
      }

      if (mat.shader.name == "Standard") {
        StandardShaderIo.ExportStandard(scene, shaderPath, mat, shader, texPath);
        scene.Write(shaderPath, shader);
        return;
      }

      if (mat.shader.name == "HDRenderPipeline/Lit") {
        HdrpShaderIo.ExportLit(scene, shaderPath, mat, shader, texPath);
        scene.Write(shaderPath, shader);
        return;
      }

      Color c;

      if (mat.HasProperty("_Color")) {
        // Standard.
        c = mat.GetColor("_Color").linear;
      } else if (mat.HasProperty("_BaseColor")) {
        // HDRP Lit.
        c = mat.GetColor("_BaseColor").linear;
      } else if (mat.HasProperty("_BaseColor0")) {
        // HDRP Layered Lit.
        c = mat.GetColor("_BaseColor").linear;
      } else {
        c = Color.white;
      }
      shader.diffuseColor.defaultValue = new Vector3(c.r, c.g, c.b);

      if (mat.HasProperty("_SpecColor")) {
        // If there is a spec color, then this is not metallic workflow.
        c = mat.GetColor("_SpecColor");
        shader.useSpecularWorkflow.defaultValue = 1;
      } else {
        c = new Color(.4f, .4f, .4f);
      }
      shader.specularColor.defaultValue = new Vector3(c.r, c.g, c.b);

      if (mat.HasProperty("_EmissionColor")) {
        c = mat.GetColor("_EmissionColor").linear;
        shader.emissiveColor.defaultValue = new Vector3(c.r, c.g, c.b);
      }

      if (mat.HasProperty("_Metallic")) {
        shader.metallic.defaultValue = mat.GetFloat("_Metallic");
      } else {
        shader.metallic.defaultValue = .5f;
      }

      if (mat.HasProperty("_Smoothness")) {
        shader.roughness.defaultValue = 1 - mat.GetFloat("_Smoothness");
      } else if (mat.HasProperty("_Glossiness")) {
        shader.roughness.defaultValue = 1 - mat.GetFloat("_Glossiness");
      } else if (mat.HasProperty("_Roughness")) {
        shader.roughness.defaultValue = mat.GetFloat("_Roughness");
      } else {
        shader.roughness.defaultValue = 0.5f;
      }

      scene.Write(shaderPath, shader);
      scene.GetPrimAtPath(shaderPath).CreateAttribute(
          new pxr.TfToken("outputs:surface"),
          SdfValueTypeNames.Token,
          false, pxr.SdfVariability.SdfVariabilityUniform);

    }

    static void ExportSkinnedMesh(Scene scene,
                       GameObject go,
                       ExportPlan exportPlan,
                       Dictionary<Material, string> matMap) {
      var smr = go.GetComponent<SkinnedMeshRenderer>();
      Mesh mesh = new Mesh();
      // TODO: export smr.sharedMesh when unvarying.
      // Ugh. Note that BakeMesh bakes the parent transform into the points, which results in
      // compounded transforms on export. The way Unity handles this is to apply a scale as part
      // of the importer, which bakes the scale into the points.
      smr.BakeMesh(mesh);
      ExportMesh(scene, go, exportPlan, matMap, mesh, smr.sharedMaterial, smr.sharedMaterials);
    }

    static void ExportMesh(Scene scene,
                       GameObject go,
                       ExportPlan exportPlan,
                       Dictionary<Material, string> matMap) {
      MeshFilter mf = go.GetComponent<MeshFilter>();
      MeshRenderer mr = go.GetComponent<MeshRenderer>();
      Mesh mesh = mf.sharedMesh;
      ExportMesh(scene, go, exportPlan, matMap, mesh, mr.sharedMaterial, mr.sharedMaterials);
    }

    static void ExportMesh(Scene scene,
                   GameObject go,
                   ExportPlan exportPlan,
                   Dictionary<Material, string> matMap,
                   Mesh mesh,
                   Material sharedMaterial,
                   Material[] sharedMaterials) {
      bool unvarying = scene.Time == null;
      bool slowAndSafeConversion = exportPlan.convertHandedness == BasisTransformation.SlowAndSafe;
      var sample = (MeshSample)exportPlan.sample;

      if (slowAndSafeConversion) {
        // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
        // basis is required. There are shortcuts, but this is fully general.
        sample.ConvertTransform();
      }

      if (unvarying) {
        // Only export the mesh topology on the first frame.
        sample.transform = GetLocalTransformMatrix(go.transform);
        sample.normals = mesh.normals;
        sample.points = mesh.vertices;
        sample.tangents = mesh.tangents;
        sample.extent = mesh.bounds;
        sample.colors = mesh.colors;

        if (sample.colors == null || sample.colors.Length == 0) {
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
          var c = sample.extent.center; c.z *= -1;
          sample.extent.center = c;

          for (int i = 0; i < sample.points.Length; i++) {
            sample.points[i] = new Vector3(sample.points[i].x, sample.points[i].y, -sample.points[i].z);
            if (sample.normals != null && sample.normals.Length == sample.points.Length) {
              sample.normals[i] = new Vector3(sample.normals[i].x, sample.normals[i].y, -sample.normals[i].z);
            }
          }

          for (int i = 0; i < tris.Length; i += 3) {
            var t = tris[i];
            tris[i] = tris[i + 1];
            tris[i + 1] = t;
          }
        }

        sample.SetTriangles(tris);
        scene.Write(exportPlan.path, sample);

        string usdMaterialPath;

        if (!matMap.TryGetValue(sharedMaterial, out usdMaterialPath)) {
          Debug.LogError("Invalid material bound for: " + exportPlan.path);
        } else {
          MaterialSample.Bind(scene, exportPlan.path, usdMaterialPath);
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

          var usdPrim = scene.GetPrimAtPath(exportPlan.path);
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

            if (!matMap.TryGetValue(sharedMaterials[si], out usdMaterialPath)) {
              Debug.LogError("Invalid material bound for: " + exportPlan.path);
            } else {
              MaterialSample.Bind(scene, subset.GetPath(), usdMaterialPath);
            }
          }
        }
      } else {
        sample.transform = GetLocalTransformMatrix(go.transform);
        scene.Write(exportPlan.path, sample);
      }
    }

    static void ExportCamera(Scene scene,
                           GameObject go,
                           ExportPlan exportPlan,
                           Dictionary<Material, string> matMap) {
      CameraSample sample = (CameraSample)exportPlan.sample;
      Camera camera = go.GetComponent<Camera>();
      bool fastConvert = exportPlan.convertHandedness == BasisTransformation.FastAndDangerous;

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
        sample.transform = basisChange * sample.transform;
      }

      scene.Write(exportPlan.path, sample);
    }

    static void ExportXform(Scene scene,
                           GameObject go,
                           ExportPlan exportPlan,
                           Dictionary<Material, string> matMap) {
      XformSample sample = (XformSample)exportPlan.sample;
      var localRot = go.transform.localRotation;
      var localScale = go.transform.localScale;
      var path = new pxr.SdfPath(exportPlan.path);
      bool fastConvert = exportPlan.convertHandedness == BasisTransformation.FastAndDangerous;

      // If exporting for Z-Up, rotate the world.
      if (path.IsRootPrimPath()) {
        float invert = fastConvert ? -1 : 1;
        if (scene.UpAxis == Scene.UpAxes.Z) {
          go.transform.transform.localRotation = localRot * Quaternion.AngleAxis(invert * 90, Vector3.right);
        }
      }

      sample.transform = GetLocalTransformMatrix(go.transform);

      // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
      // basis is required. There are shortcuts, but this is fully general.
      //
      // Here we can either put a partial conversion at the root (fast & dangerous) or convert the
      // entire hierarchy, along with the points, normals and triangle winding. The benefit of the
      // full conversion is that there are no negative scales left in the hierarchy.
      //
      if (fastConvert && path.IsRootPrimPath()) {
        // Partial change of basis.
        var basisChange = UnityEngine.Matrix4x4.identity;
        // Invert the forward vector.
        basisChange[2, 2] = -1;
        sample.transform = basisChange * sample.transform;
      } else if (!fastConvert) {
        // Full change of basis.
        sample.ConvertTransform();
      }

      if (path.IsRootPrimPath()) {
        go.transform.localRotation = localRot;
        go.transform.localScale = localScale;
      }

      scene.Write(exportPlan.path, sample);
    }

    static Matrix4x4 GetLocalTransformMatrix(Transform tr) {
      return Matrix4x4.TRS(tr.localPosition, tr.localRotation, tr.localScale);
    }
  }
}