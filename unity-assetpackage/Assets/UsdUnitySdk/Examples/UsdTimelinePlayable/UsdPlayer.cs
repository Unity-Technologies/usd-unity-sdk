using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USD.NET.Examples;

namespace USD.NET.Unity.Extensions.Player {
  public class UsdPlayer : MonoBehaviour {
    public double Length { get { return m_scene != null ? (m_scene.EndTime - m_scene.StartTime) / (m_scene.Stage.GetFramesPerSecond()) : 0; } }

    public void SetTime(double time) {
      SetupScene();
      if (m_scene == null) return;
      m_scene.Time = m_scene.StartTime + time * m_scene.Stage.GetFramesPerSecond();
      UpdateScene();
    }

    public string m_usdFile;
    public Material m_material;

    private Scene m_scene;

    // Keep track of all objects loaded.
    [SerializeField]
    private Dictionary<string, GameObject> m_primMap = new Dictionary<string, GameObject>();

    void UpdateScene() {
      foreach (var path in m_scene.AllPaths) {
        if (!m_primMap.ContainsKey(path)) { continue; }
        var xf = new USD.NET.Unity.XformSample();
        m_scene.Read(new pxr.SdfPath(path), xf);
        var go = m_primMap[path];
        AssignTransform(xf, go);
      }
    }

    public void SetupScene() {
      InitUsd.Initialize();

      // Is the stage already loaded?
      if (m_scene != null && m_scene.Stage.GetRootLayer().GetIdentifier() == m_usdFile) {
        return;
      }

      // Does the path exist?
      if (!System.IO.File.Exists(m_usdFile)) {
        return;
      }

      // Clear out the old scene.
      UnloadGameObjects();

      // Load the new scene.
      m_scene = Scene.Open(m_usdFile);
      if (m_scene == null) {
        throw new Exception("Failed to load");
      }

      // Set the time at which to read samples from USD.
      m_scene.Time = 0;

      // Handle configurable up-axis (Y or Z).
      Vector3 up = GetUpVector(m_scene);
      var rootXf = new GameObject("root");
      rootXf.transform.SetParent(transform, worldPositionStays: false);

      if (up != Vector3.up) {
        rootXf.transform.localRotation = Quaternion.FromToRotation(Vector3.up, up);
      }

      // Convert from right-handed (USD) to left-handed (Unity).
      // The math below works out to either (1, -1, 1) or (1, 1, -1), depending on up.
      rootXf.transform.localScale = up + -1 * (Vector3.one - up - Vector3.right) + Vector3.right;

      // Assign this transform as the root.
      m_primMap.Add("/", rootXf);

      // Load transforms.
      foreach (var pathAndXf in m_scene.ReadAll<XformSample>()) {
        var go = new GameObject(pathAndXf.path.GetName());
        AssignTransform(pathAndXf.sample, go);
        AssignParent(new pxr.SdfPath(pathAndXf.path), go);
      }

      // Load meshes.
      foreach (var pathAndMesh in m_scene.ReadAll<MeshSample>()) {
        var go = new GameObject(pathAndMesh.path.GetName());
        AssignTransform(pathAndMesh.sample, go);
        AssignParent(new pxr.SdfPath(pathAndMesh.path), go);
        BuildMesh(pathAndMesh.sample, go);
      }

      // Load cameras.
      foreach (var pathAndCam in m_scene.ReadAll<CameraSample>()) {
        var go = new GameObject(pathAndCam.path.GetName());
        m_scene.Read(pathAndCam.path, pathAndCam.sample);
        AssignTransform(pathAndCam.sample, go);
        BuildCamera(pathAndCam.sample, go);
        AssignParent(new pxr.SdfPath(pathAndCam.path), go);
      }

      // Ensure the file and the identifier match.
      m_usdFile = m_scene.Stage.GetRootLayer().GetIdentifier();
    }

    Vector3 GetUpVector(Scene scene) {
      // Note: currently Y and Z are the only valid values.
      // https://graphics.pixar.com/usd/docs/api/group___usd_geom_up_axis__group.html

      switch (scene.UpAxis) {
        case Scene.UpAxes.Y:
          // Note, this is also Unity's default up vector.
          return Vector3.up;
        case Scene.UpAxes.Z:
          return new Vector3(0, 0, 1);
        default:
          throw new Exception("Invalid upAxis value: " + scene.UpAxis.ToString());
      }
    }

    // Convert Matrix4x4 into TSR components.
    void AssignTransform(Unity.XformableSample xf, GameObject go) {
      go.transform.localPosition = ExtractPosition(xf.transform);
      go.transform.localScale = ExtractScale(xf.transform);
      go.transform.localRotation = ExtractRotation(xf.transform);
    }

    // Recreate hierarchy.
    void AssignParent(pxr.SdfPath path, GameObject go) {
      m_primMap.Add(path, go);
      go.transform.SetParent(m_primMap[path.GetParentPath()].transform, worldPositionStays: false);
    }

    void BuildCamera(CameraSample usdCamera, GameObject go) {
      var cam = go.AddComponent<Camera>();
      usdCamera.CopyToCamera(cam);
    }

    // Copy mesh data to Unity and assign mesh with material.
    void BuildMesh(USD.NET.Unity.MeshSample usdMesh, GameObject go) {
      var mf = go.AddComponent<MeshFilter>();
      var mr = go.AddComponent<MeshRenderer>();
      var unityMesh = new UnityEngine.Mesh();
      var mat = Material.Instantiate(m_material);

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
          // FaceVarying and uniform both require breaking up the mesh and are not yet handled in this
          // example.
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

    // Destroy all previously created objects.
    void UnloadGameObjects() {
      foreach (var kvp in m_primMap) {
        if (Application.isPlaying) {
          Destroy(kvp.Value);
        } else {
          DestroyImmediate(kvp.Value);
        }
      }

      while (transform.childCount > 0) {
        if (Application.isPlaying) {
          Destroy(transform.GetChild(0).gameObject);
        } else {
          DestroyImmediate(transform.GetChild(0).gameObject);
        }
      }

      m_primMap.Clear();
    }

    // -------------------------------------------------------------------------------------------- //
    // Matrix4x4 helpers, not USD specific.
    // -------------------------------------------------------------------------------------------- //

    static Quaternion ExtractRotation(Matrix4x4 mat4) {
      Vector3 forward;
      forward.x = mat4.m02;
      forward.y = mat4.m12;
      forward.z = mat4.m22;
      Vector3 up;
      up.x = mat4.m01;
      up.y = mat4.m11;
      up.z = mat4.m21;
      return Quaternion.LookRotation(forward, up);
    }

    static Vector3 ExtractPosition(Matrix4x4 mat4) {
      Vector3 position;
      position.x = mat4.m03;
      position.y = mat4.m13;
      position.z = mat4.m23;
      return position;
    }

    static Vector3 ExtractScale(Matrix4x4 mat4) {
      Vector3 scale;
      scale.x = new Vector4(mat4.m00, mat4.m10, mat4.m20, mat4.m30).magnitude;
      scale.y = new Vector4(mat4.m01, mat4.m11, mat4.m21, mat4.m31).magnitude;
      scale.z = new Vector4(mat4.m02, mat4.m12, mat4.m22, mat4.m32).magnitude;
      return scale;
    }
  }
}
