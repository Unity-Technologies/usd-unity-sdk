using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace USD.NET.Unity {
  public class ImportOptionsWindow : EditorWindow {
    [Header("Source Asset")]
    public string m_usdFile;
    public double m_time;
    public Scene.InterpolationMode m_interpolation;

    [Header("Conversions")]
    public float m_scale;
    public BasisTransformation m_changeHandedness;

    [Header("Materials")]
    public MaterialImportMode m_materialImportMode = MaterialImportMode.ImportDisplayColor;
    public bool m_enableGpuInstancing;
    public Material m_fallbackMaterial;

    [Header("Mesh Options")]
    public bool m_generateLightmapUVs;
    public ImportMode m_boundingBox;
    public ImportMode m_color;
    public ImportMode m_normals;
    public ImportMode m_tangents;
    public ImportMode m_texcoord0;
    public ImportMode m_texcoord1;
    public ImportMode m_texcoord2;
    public ImportMode m_texcoord3;


    public static void Open(string path) {
      ImportOptionsWindow window = (ImportOptionsWindow)EditorWindow.GetWindow(typeof(ImportOptionsWindow));
      window.titleContent = new GUIContent("Import Settings");
      window.Show();
    }

    void OnGUI() {

      //Editor objectEditor = Editor.CreateEditor(object);
      //base.DrawDefaultInspector();
      EditorGUILayout.Space();

      GUILayout.Space(10.0f);

      if (GUILayout.Button("Import")) {
        Close();
      }
    }

  }
}