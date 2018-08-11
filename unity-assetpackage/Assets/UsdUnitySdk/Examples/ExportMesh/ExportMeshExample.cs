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
    delegate void ExportFunction(Scene scene, GameObject go, ExportPlan exportPlan);

    // An export plan will be created for each path in the scene. Each ExportPlan will use one of
    // the fixed export functions. For example, when setting up export for a mesh, an ExportPlan
    // will be created for that path in the scenegraph and the ExportFunction will the one which is
    // capable of exporting a mesh.
    struct ExportPlan {
      // The USD path at which the Unity data will be written.
      public string path;
      
      // The sample which will hold the data to export.
      public SampleBase sample;

      // The export function which implements the logic to populate the sample.
      public ExportFunction exportFunc;
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
        InitHierarchy(m_exportRoot);

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
          exportPlan.exportFunc(m_usdScene, go, exportPlan);
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
        exportPlan.exportFunc(m_usdScene, go, exportPlan);
      }
    }

    // ------------------------------------------------------------------------------------------ //
    // Init Hierarchy.
    // ------------------------------------------------------------------------------------------ //

    delegate void ObjectProcessor(GameObject go);

    void Traverse(GameObject obj, ObjectProcessor processor) {
      processor(obj);
      foreach (Transform child in obj.transform) {
        Traverse(child.gameObject, processor);
      }
    }

    void InitHierarchy(GameObject exportRoot) {
      Traverse(exportRoot, InitExportableObjects);
    }

    void InitExportableObjects(GameObject go) {
      SampleBase sample = null;
      ExportFunction exportFunc = null;

      if (go.GetComponent<MeshFilter>() != null && go.GetComponent<MeshRenderer>() != null) {
        sample = new MeshSample();
        exportFunc = ExportMesh;
        foreach (var mat in go.GetComponent<MeshRenderer>().materials) {
          if (!m_materialMap.ContainsKey(mat)) {
            string usdPath = "/World/Materials/" + pxr.UsdCs.TfMakeValidIdentifier(mat.name);
            m_materialMap.Add(mat, usdPath);
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
      m_primMap.Add(go, new ExportPlan { path=path, sample=sample, exportFunc=exportFunc });
      Debug.Log(path + " " + sample.GetType().Name);

      // Include the parent xform hierarchy.
      // Note that the parent hierarchy is memoised, so despite looking expensive, the time
      // complexity is linear.
      Transform xf = go.transform.parent;
      while (xf) {
        if (!InitExportableParents(xf.gameObject)) {
          break;
        }
        xf = xf.parent;
      }
    }

    bool InitExportableParents(GameObject go) {
      if (m_primMap.ContainsKey(go)) {
        // Stop processing parents, this keeps the performance of the traversal linear.
        return false;
      }

      // Any object we add will only be exported as an Xform.
      string path = UnityTypeConverter.GetPath(go.transform);
      SampleBase sample = new XformSample();
      m_primMap.Add(go, new ExportPlan { path = path, sample = sample, exportFunc = ExportXform });
      Debug.Log(path + " " + sample.GetType().Name);

      // Continue processing parents.
      return true;
    }

    // ------------------------------------------------------------------------------------------ //
    // Type-Specific (Mesh, Xform, Camera) Exporters.
    // ------------------------------------------------------------------------------------------ //
    void ExportMaterial(Scene scene, Material mat, string usdMaterialPath) {
      string shaderPath = usdMaterialPath + "/StandardShader";

      var material = new USD.NET.Unity.MaterialSample();
      material.surface.SetConnectedPath(shaderPath, "outputs:out");

      var shader = new StandardShaderSample();
      shader.id = new pxr.TfToken("Unity.Standard");
      shader.albedo.defaultValue = mat.color;

      scene.Write(usdMaterialPath, material);
      scene.Write(shaderPath, shader);
    }

    void ExportMesh(Scene scene, GameObject go, ExportPlan exportPlan) {
      MeshFilter mf = go.GetComponent<MeshFilter>();
      Mesh mesh = mf.mesh;
      bool unvarying = scene.Time == null;

      if (unvarying) {
        // Only export the mesh topology on the first frame.
        var sample = (MeshSample)exportPlan.sample;
        sample.transform = GetLocalTransformMatrix(go.transform);

        // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
        // basis is required. There are shortcuts, but this is fully general.
        sample.ConvertTransform();

        sample.normals = mesh.normals;
        sample.points = mesh.vertices;
        sample.tangents = mesh.tangents;
        sample.extent = mesh.bounds;
        sample.colors = mesh.colors;
        if (sample.colors != null && sample.colors.Length != sample.points.Length) {
          sample.colors = null;
        }

        // Set face vertex counts and indices.
        sample.SetTriangles(mesh.triangles);

        scene.Write(exportPlan.path, sample);

        var mr = go.GetComponent<MeshRenderer>();
        string usdMaterialPath;
        if (!m_materialMap.TryGetValue(mr.material, out usdMaterialPath)) {
          Debug.LogError("Invalid material bound for: " + exportPlan.path);
        } else {
          MaterialSample.Bind(scene, exportPlan.path, usdMaterialPath);
        }
      } else {
        var sample = new XformSample();
        sample.transform = GetLocalTransformMatrix(go.transform);

        // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
        // basis is required. There are shortcuts, but this is fully general.
        sample.ConvertTransform();

        scene.Write(exportPlan.path, sample);
      }

      // On all other frames, we just export the mesh transform data.
      // TODO(jcowles): cant currently do this because the USD prim typeName is overwritten.
      //ExportXform(scene, go, exportPlan, unvarying: false);
    }

    void ExportCamera(Scene scene, GameObject go, ExportPlan exportPlan) {
      CameraSample sample = (CameraSample)exportPlan.sample;
      Camera camera = go.GetComponent<Camera>();
      sample.CopyFromCamera(camera);
      scene.Write(exportPlan.path, sample);
    }

    void ExportXform(Scene scene, GameObject go, ExportPlan exportPlan) {
      XformSample sample = (XformSample)exportPlan.sample;
      sample.transform = GetLocalTransformMatrix(go.transform);

      // Unity uses a forward vector that matches DirectX, but USD matches OpenGL, so a change of
      // basis is required. There are shortcuts, but this is fully general.
      sample.ConvertTransform();

      scene.Write(exportPlan.path, sample);
    }

    Matrix4x4 GetLocalTransformMatrix(Transform tr) {
      return Matrix4x4.TRS(tr.localPosition, tr.localRotation, tr.localScale);
    }

    // ------------------------------------------------------------------------------------------ //
    // General Export Helpers.
    // ------------------------------------------------------------------------------------------ //

    // Copy mesh data to Unity and assign mesh with material.
    void BuildMesh(USD.NET.Unity.MeshSample usdMesh, GameObject go) {
      var mf = go.AddComponent<MeshFilter>();
      var mr = go.AddComponent<MeshRenderer>();
      var unityMesh = new UnityEngine.Mesh();
      Material mat = null;// = Material.Instantiate(m_material);

      unityMesh.vertices = usdMesh.points;

      // Triangulate n-gons.
      // For best performance, triangulate off-line and skip conversion.
      var indices = USD.NET.Unity.UnityTypeConverter.ToVtArray(usdMesh.faceVertexIndices);
      var counts = USD.NET.Unity.UnityTypeConverter.ToVtArray(usdMesh.faceVertexCounts);
      pxr.UsdGeomMesh.Triangulate(indices, counts);
      USD.NET.Unity.UnityTypeConverter.FromVtArray(indices, ref usdMesh.faceVertexIndices);

      if (usdMesh.orientation == Orientation.LeftHanded) {
        // USD is right-handed, so the mesh needs to be flipped.
        // Unity is left-handed, but that doesn't matter here.
        for (int i = 0; i < usdMesh.faceVertexIndices.Length; i += 3) {
          int tmp = usdMesh.faceVertexIndices[i];
          usdMesh.faceVertexIndices[i] = usdMesh.faceVertexIndices[i + 1];
          usdMesh.faceVertexIndices[i + 1] = tmp;
        }
      }

      unityMesh.triangles = usdMesh.faceVertexIndices;

      if (usdMesh.extent.size.x > 0 || usdMesh.extent.size.y > 0 || usdMesh.extent.size.z > 0) {
        unityMesh.bounds = usdMesh.extent;
      } else {
        unityMesh.RecalculateBounds();
      }

      if (usdMesh.normals != null) {
        unityMesh.normals = usdMesh.normals;
      } else {
        unityMesh.RecalculateNormals();
      }

      if (usdMesh.tangents != null) {
        unityMesh.tangents = usdMesh.tangents;
      } else {
        unityMesh.RecalculateTangents();
      }

      if (usdMesh.colors != null) {
        // NOTE: The following color conversion assumes PlayerSettings.ColorSpace == Linear.
        // For best performance, convert color space to linear off-line and skip conversion.

        if (usdMesh.colors.Length == 1) {
          // Constant color can just be set on the material.
          mat.color = usdMesh.colors[0].gamma;
        } else if (usdMesh.colors.Length == usdMesh.points.Length) {
          // Vertex colors map on to verts.
          // TODO: move the conversion to C++ and use the color management API.
          for (int i = 0; i < usdMesh.colors.Length; i++) {
            usdMesh.colors[i] = usdMesh.colors[i].gamma;
          }

          unityMesh.colors = usdMesh.colors;
        } else {
          // FaceVarying and uniform both require breaking up the mesh and are not yet handled in
          // this example.
          Debug.LogWarning("Uniform (color per face) and FaceVarying (color per vert per face) "
                         + "display color not supported in this example");
        }
      }

      // As in Unity, UVs are a dynamic type which can be vec2, vec3, or vec4.
      if (usdMesh.uv != null) {
        Type uvType = usdMesh.uv.GetType();
        if (uvType == typeof(Vector2[])) {
          unityMesh.SetUVs(0, ((Vector2[])usdMesh.uv).ToList());
        } else if (uvType == typeof(Vector3[])) {
          unityMesh.SetUVs(0, ((Vector3[])usdMesh.uv).ToList());
        } else if (uvType == typeof(Vector4[])) {
          unityMesh.SetUVs(0, ((Vector4[])usdMesh.uv).ToList());
        } else {
          throw new Exception("Unexpected UV type: " + usdMesh.uv.GetType());
        }
      }

      mr.material = mat;
      mf.mesh = unityMesh;
    }
  }
}