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

using System.IO;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Examples
{
    /// <remarks>
    /// Export Mesh Example
    /// This Example uses the same methods used when exporting Unity Objects via the 'USD Menu' in the Menu Bar.
    ///
    ///  * InitializeForExport:
    ///    * Create and configure an empty USD scene for Export.
    ///    ** For USDZ export, the Initialization of USD package is done within the export function itself
    ///
    ///  * ExportGameObject:
    ///    * Save and close the USD scene
    ///    * Release USD scene.
    ///
    ///  * ExportGameObjectAsUSDZ:
    ///    * Create the USDZ file structure
    ///    * Save and close the USD scene within the USDZ file structure as a .USDC file format.
    /// </remarks>
    public class ExportMeshExample : MonoBehaviour
    {
        public enum UsdFileExtension
        {
            usd,
            usda,
            usdc,
            usdz
        }

        [HeaderAttribute("Export Related Variables")]
        // The root GameObject to export to USD.
        public GameObject m_exportRoot;
        public GameObject[] m_trackedRoots;

        [SerializeField]
        private UsdFileExtension usdFileExtension;
        public UsdFileExtension FileExtension => usdFileExtension;

        public bool m_exportMaterials = true;
        public BasisTransformation m_convertHandedness = BasisTransformation.SlowAndSafe;
        public ActiveExportPolicy m_activePolicy = ActiveExportPolicy.ExportAsVisibility;

        // The file name of the exported USD file.
        public string m_newUsdFileName;

        // The scene object to which the recording will be saved.
        protected static Scene m_usdScene;

        // Export Context settings
        protected static ExportContext m_context = new ExportContext();

        // -- USD Export as Non .usdz Steps --

        public void InitUSD()
        {
            InitUsd.Initialize();
        }

        public void CreateNewUsdScene()
        {
            if (m_newUsdFileName == null)
            {
                Debug.LogError("<New USD File Name> not assigned.");
                return;
            }

            m_usdScene = CreateNewScene($"{m_newUsdFileName}.{FileExtension}");
        }

        public void SetUpExportContext()
        {
            m_context = SetUpInitialExportContext(m_usdScene, m_convertHandedness, m_exportMaterials);
        }

        public void Export()
        {
            InitialExport(m_exportRoot, m_trackedRoots);
        }

        public void SaveScene()
        {
            SaveScene(m_usdScene, m_newUsdFileName);
        }

        public void CloseScene()
        {
            CloseScene(m_usdScene);
        }

        // -- USD Export as .usdz Steps --

        public void ExportAsUsdz()
        {
            // Refer to the UsdzExporter.ExportUsdz for Export as USDZ
            // USDZ Export mainly follows the same steps as normal export, but involves some additional minor scale and folder changes to match the USDZ format
            UsdzExporter.ExportUsdz(Path.Combine(SampleUtils.SampleArtifactDirectory, $"{m_newUsdFileName}.usdz"), m_exportRoot);
        }

        // -- USD Export Functions --
        
        protected static Scene CreateNewScene(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return Scene.Create();
            }
            else
            {
                return Scene.Create(Path.Combine(SampleUtils.SampleArtifactDirectory, fileName));
            }
        }

        protected static ExportContext SetUpInitialExportContext(Scene usdScene, BasisTransformation convertHandedness, bool exportMaterials)
        {
            // For simplicity in this example, adding game objects while recording is not supported.
            var context = new ExportContext();
            context.scene = usdScene;
            context.basisTransform = convertHandedness;

            // First write materials and unvarying values (mesh topology, etc).
            context.exportMaterials = exportMaterials;
            context.scene.Time = null;
            context.activePolicy = ActiveExportPolicy.ExportAsVisibility;

            return context;
        }

        protected static void InitialExport(GameObject exportRoot, GameObject[] trackedRoots)
        {
            SceneExporter.SyncExportContext(exportRoot, m_context);
            ExportSceneData(exportRoot, trackedRoots);
        }

        protected static void ExportSceneData(GameObject rootGameObject, GameObject[] trackedRoots)
        {
            // Record the time varying data that changes from frame to frame.
            if (trackedRoots != null && trackedRoots.Length > 0)
            {
                foreach (var root in trackedRoots)
                {
                    SceneExporter.Export(root, m_context, zeroRootTransform: false);
                }
            }
            else
            {
                SceneExporter.Export(rootGameObject, m_context, zeroRootTransform: true);
            }
        }

        protected static void SaveScene(Scene scene, string sceneName)
        {
            // In a real exporter, additional error handling should be added here.
            if (!string.IsNullOrEmpty(sceneName))
            {
                // We could use SaveAs here, which is fine for small scenes, though it will require
                // everything to fit in memory and another step where that memory is copied to disk.
                scene.Save();
            }
        }

        protected static void CloseScene(Scene scene)
        {
            // Release memory associated with the scene.
            scene.Close();
            scene = null;
        }
    }
}
