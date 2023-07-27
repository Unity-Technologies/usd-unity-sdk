// Copyright 2018 Jeremy Cowles. All rights reserved.
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

using System.IO;
using UnityEngine;
using USD.NET;
using pxr;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Unity.Formats.USD
{
    public class UsdzExporter
    {
        public static void ExportUsdz(string usdzFilePath,
            GameObject root)
        {
            // Ensure USD is initialized before changing CWD.
            // This does not protect us against external changes to CWD, so we are actively looking for
            // a more robust solution with UPM devs.
            InitUsd.Initialize();

            // Keep the current directory to restore it at the end.
            var currentDir = Directory.GetCurrentDirectory();

            // Setup a temporary directory to export the wanted USD file and zip it.
            string tmpDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            DirectoryInfo tmpDir = Directory.CreateDirectory(tmpDirPath);

            // Get the usd file name to export and the usdz file name of the archive.
            string usdcFileName = Path.GetFileNameWithoutExtension(usdzFilePath) + ".usdc";
            string usdzFileName = Path.GetFileName(usdzFilePath);

            bool success = true;

            Stopwatch analyticsTimer = new Stopwatch();
            analyticsTimer.Start();

            try
            {
                // Set the current working directory to the tmp directory to export with relative paths.
                Directory.SetCurrentDirectory(tmpDirPath);

                // Create the tmp .usd scene, into which the data will be exported.
                Scene scene = ExportHelpers.InitForSave(Path.Combine(tmpDirPath, usdcFileName));
                Vector3 localScale = root.transform.localScale;

                try
                {
                    // USDZ is in centimeters.
                    root.transform.localScale = localScale * 100;
                    scene.MetersPerUnit = 0.01;
                    // Export the temp scene.
                    SceneExporter.Export(root,
                        scene,
                        BasisTransformation.SlowAndSafe, // Required by ARKit
                        exportUnvarying: true,
                        zeroRootTransform: false,
                        exportMaterials: true);
                }
                finally
                {
                    // Undo temp scale.
                    root.transform.localScale = localScale;

                    // Flush any in-flight edits and release the scene so the file can be deleted.
                    scene.Save();
                    scene.Close();
                    scene = null;
                }

                SdfAssetPath assetPath = new SdfAssetPath(usdcFileName);
                success = pxr.UsdCs.UsdUtilsCreateNewARKitUsdzPackage(assetPath, usdzFileName);

                if (!success)
                {
                    Debug.LogError($"Couldn't export {root.name} to the usdz file {usdzFilePath}");
                    success = false;
                    return;
                }

                File.Copy(usdzFileName, usdzFilePath, overwrite: true);
            }
            finally
            {
                // Clean up temp files.
                Directory.SetCurrentDirectory(currentDir);
                tmpDir.Delete(recursive: true);

                analyticsTimer.Stop();
                UsdEditorAnalytics.SendExportEvent(".usdz", analyticsTimer.Elapsed.TotalMilliseconds, success);
            }
        }
    }
}
