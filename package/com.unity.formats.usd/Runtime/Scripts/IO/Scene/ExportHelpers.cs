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
                return;

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
                }
            }
            scene.Save();
            scene.Close();
        }
    }
}
