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
        [HeaderAttribute("Export Related Variables")]
        // The root GameObject to export to USD.
        public GameObject m_exportRoot;
        public GameObject[] m_trackedRoots;

        public bool m_exportMaterials = true;
        public BasisTransformation m_convertHandedness = BasisTransformation.SlowAndSafe;
        public ActiveExportPolicy m_activePolicy = ActiveExportPolicy.ExportAsVisibility;

        // The file name of the exported USD file.
        public string m_newUsdFileName;

        // Full path to exported file
        private string m_exportedUsdFilePath { get { return Path.Combine(Application.dataPath, m_newUsdFileName); } }

        // The scene object to which the recording will be saved.
        protected static Scene m_usdScene;

        // Export Context settings
        protected static ExportContext m_context = new ExportContext();

        public void InitializeForExport()
        {
            if (m_newUsdFileName == null)
            {
                Debug.LogError("<New USD File Name> not assigned.");
                return;
            }

            m_usdScene = ExportHelpers.InitForSave(m_exportedUsdFilePath);
        }

        // This is the function used in:
        // Menu Bar -> USD -> Exported Selected with Children
        public void ExportGameObject()
        {
            ExportHelpers.ExportGameObjects(new GameObject[] { m_exportRoot }, m_usdScene, BasisTransformation.SlowAndSafe);
            m_usdScene = null;
        }

        // This is the function used in:
        // Menu Bar -> USD -> Exported Selected as USDZ
        public void ExportGameObjectAsUSDZ()
        {
            UsdzExporter.ExportUsdz(Path.Combine(Application.dataPath, m_newUsdFileName), m_exportRoot);
        }
    }
}
