using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD
{
    public static class ExportHelpers
    {
        public static Scene InitForSave(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var fileDir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(fileDir))
            {
                var di = Directory.CreateDirectory(fileDir);
                if (!di.Exists)
                {
                    Debug.LogError("Failed to create directory: " + fileDir);
                    return null;
                }
            }

            InitUsd.Initialize();
            var scene = Scene.Create(filePath);
            scene.Time = 0;
            scene.StartTime = 0;
            scene.EndTime = 0;
            return scene;
        }

        public static void ExportGameObjects(GameObject[] objects, Scene scene, BasisTransformation basisTransform,
            bool exportMonoBehaviours = false)
        {
            if (scene == null)
            {
                UsdEditorAnalytics.SendExportEvent("", .0f, false);
                return;
            }

            bool success = true;
            System.Diagnostics.Stopwatch analyticsTimer = new System.Diagnostics.Stopwatch();
            analyticsTimer.Start();

            foreach (GameObject go in objects)
            {
                try
                {
                    SceneExporter.Export(go, scene, basisTransform,
                        exportUnvarying: true, zeroRootTransform: false,
                        exportMonoBehaviours: exportMonoBehaviours);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    success = false;
                }
            }
            scene.Save();

            analyticsTimer.Stop();
            UsdEditorAnalytics.SendExportEvent(Path.GetExtension(scene.FilePath), analyticsTimer.ElapsedMilliseconds * 0.001f, success);

            scene.Close();
        }
    }
}
