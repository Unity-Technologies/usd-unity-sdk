using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD
{
    public static class ExportHelpers
    {
#if UNITY_EDITOR
        public static Scene InitForSave(string defaultName, string fileExtension = "usd,usda,usdc")
        {
            var filePath = EditorUtility.SaveFilePanel("Export USD File", "", defaultName, fileExtension);

            if (filePath == null || filePath.Length == 0)
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
#endif

        public static void ExportSelected(BasisTransformation basisTransform,
            string fileExtension = "usd,usda,usdc",
            bool exportMonoBehaviours = false)
        {
            Scene scene = null;

            foreach (GameObject go in Selection.gameObjects)
            {
                if (scene == null)
                {
                    scene = InitForSave(go.name, fileExtension);
                    if (scene == null)
                    {
                        return;
                    }
                }

                try
                {
                    SceneExporter.Export(go, scene, basisTransform,
                        exportUnvarying: true, zeroRootTransform: false,
                        exportMonoBehaviours: exportMonoBehaviours);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    continue;
                }
            }

            if (scene != null)
            {
                scene.Save();
                scene.Close();
            }
        }
    }
}
