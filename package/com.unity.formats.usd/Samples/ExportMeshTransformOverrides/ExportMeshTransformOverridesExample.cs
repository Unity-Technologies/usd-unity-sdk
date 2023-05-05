// Copyright 2023 Unity Technologies. All rights reserved.
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

using pxr;
using System.IO;
using UnityEditor;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Examples
{
    /// <remarks>
    /// Export Mesh Transform Overrides Example
    /// This Example uses the same methods used when exporting Transform Overrides of an imported USD file via the 'USD Menu' in the Menu Bar.
    ///
    ///  * ImportInitialUsdFile:
    ///    * Import an example USD file
    ///
    ///  * ChangeExampleTransformData:
    ///    * Change the transform data of the imported example asset randomly
    ///
    ///  * ExportTransformOverride:
    ///    * Create the Overrides USD file.
    ///    * Exports the Overrides data on to the created Overrides USD file.
    /// </remarks>
    public class ExportMeshTransformOverridesExample : MonoBehaviour
    {
        // GUID of the example USD file
        private const string k_exampleUsdFileGUID = "82266c9d3cfb61d49906dfe90e9c060a";

        private const string k_exportOverridesFileExtension = "usda";

        public static GameObject m_exampleImportedUsdObject;

        public void ImportInitialUsdFile()
        {
            var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_exampleUsdFileGUID));
            var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
            var scene = Scene.Open(stage);
            m_exampleImportedUsdObject = ImportHelpers.ImportSceneAsGameObject(scene, null, new SceneImportOptions() { materialImportMode = MaterialImportMode.ImportPreviewSurface });
            scene.Close();
        }

        public void ChangeExampleTransformData()
        {
            const string debugFloatDecimalPoint = "F4";

            var cube = m_exampleImportedUsdObject.transform.GetChild(0).GetChild(2); // The Middle cube
            cube.transform.position = Random.insideUnitSphere * Random.Range(-1, 2);
            cube.localScale = Random.insideUnitSphere;
            cube.rotation = Random.rotation;

            SampleUtils.FocusConsoleWindow();
            Debug.Log($"For <{cube.name}> the following have changed:");
            Debug.Log($"The new position is now: {cube.transform.position.ToString(debugFloatDecimalPoint)}");
            Debug.Log($"The new scale is now: {cube.transform.localScale.ToString(debugFloatDecimalPoint)}");
            Debug.Log($"The new rotation is now: {cube.transform.rotation.eulerAngles.ToString(debugFloatDecimalPoint)}");
        }

        // Utilizes the same method when doing Export Overrides through:
        // Menu Bar -> USD -> Exported Transform Overrides
        // Will save the Transform Override file in the project 'Assets/Samples' folder
        public void ExportTransformOverride()
        {
            var newFileName = $"{m_exampleImportedUsdObject.name}_overs.{k_exportOverridesFileExtension}";
            var overs = ExportHelpers.InitForSave(Path.Combine(SampleUtils.SampleArtifactDirectory, newFileName));
            m_exampleImportedUsdObject.GetComponent<UsdAsset>().ExportOverrides(overs);

            SampleUtils.FocusConsoleWindow();
            Debug.Log(SampleUtils.SetTextColor(SampleUtils.TextColor.Green, $"Created USD file of Transform Overrides: <<b>{newFileName}</b>> under project <b>'{SampleUtils.SampleArtifactRelativeDirectory}'</b> folder."));
        }
    }
}
