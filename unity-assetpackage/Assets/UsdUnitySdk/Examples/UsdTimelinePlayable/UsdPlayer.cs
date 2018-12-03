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
    private GameObject m_root;

    void UpdateScene() {
      Debug.Assert(m_root);
      
      var options = new SceneImportOptions();
      var importer = m_root.GetComponent<StageRoot>();
      importer.m_usdTime = (float)m_scene.Time.GetValueOrDefault();
    }

    public void SetupScene() {
      InitUsd.Initialize();

      // Is the stage already loaded?
      if (m_root != null && m_scene != null && m_scene.FilePath == m_usdFile) {
        return;
      }

      // Does the path exist?
      if (!System.IO.File.Exists(m_usdFile)) {
        return;
      }

      // Load the new scene.
      m_scene = Scene.Open(m_usdFile);
      if (m_scene == null) {
        throw new Exception("Failed to load");
      }

      // Set the time at which to read samples from USD.
      m_scene.Time = 0;

      if (m_root) {
        DestroyImmediate(m_root);
      }

      m_root = new GameObject("root");
      m_root.transform.SetParent(transform, worldPositionStays: false);
      var options = new SceneImportOptions();
      options.materialImportMode = MaterialImportMode.ImportDisplayColor;
      options.changeHandedness = BasisTransformation.SlowAndSafe;
      options.materialMap.SpecularWorkflowMaterial = new Material(Shader.Find("Standard (Specular setup)"));
      options.materialMap.MetallicWorkflowMaterial = new Material(Shader.Find("Standard (Roughness setup)"));
      options.materialMap.FallbackMasterMaterial = new Material(Shader.Find("USD/StandardVertexColor"));

      try {
        SceneImporter.BuildScene(m_scene, m_root, pxr.SdfPath.AbsoluteRootPath(), options);
      } finally {
        // Ensure the file and the identifier match.
        m_usdFile = m_scene.FilePath;
      }
    }

  }
}
