using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Formats.USD;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Examples
{
    /// <remarks>
    /// Export Mesh Example
    /// This Example uses the same methods used when exporting Unity Objects via the 'USD Menu' in 'OS Menu Bar'.
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

        // The path to where the USD file will be written.
        // If null/empty, the file will be created in memory only.
        public string m_newUsdFileName;

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

            var path = Path.Combine(Application.dataPath, m_newUsdFileName);
            m_usdScene = ExportHelpers.InitForSave(path);
        }

        public void ExportGameObject()
        {
            ExportHelpers.ExportGameObjects(new GameObject[] { m_exportRoot }, m_usdScene, BasisTransformation.SlowAndSafe);
            m_usdScene = null;
        }

        public void ExportGameObjectAsUSDZ()
        {
            UsdzExporter.ExportUsdz(Path.Combine(Application.dataPath, m_newUsdFileName), m_exportRoot);
        }
    }
}
