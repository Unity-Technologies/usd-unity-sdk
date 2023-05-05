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

using UnityEngine;

namespace Unity.Formats.USD.Examples
{
    public class ImportProcessorExample_EntryPoint : MonoBehaviour
    {
        public UsdAsset usdAsset;

        public enum SampleMode
        {
            CombineMeshes,
            SetHideFlags
        }

        public SampleMode sampleMode;

        [HideInInspector]
        public HideFlags hideFlagsSetting;

        // Refresh will trigger the USD Import Processors
        public void RefreshUSD()
        {
            if (sampleMode == SampleMode.SetHideFlags)
            {
                usdAsset.transform.GetComponent<ImportProcessorExample_SetHideFlags>().hideFlagsSettings = hideFlagsSetting;
            }
            usdAsset.transform.GetComponent<UsdAsset>().Reload(true);
        }

        public void ResetUsdAsset()
        {
            var importProcessor = usdAsset.GetComponent<RegexImportProcessor>();
            if (importProcessor != null)
            {
                DestroyImmediate(importProcessor);
            }

            foreach (Transform child in usdAsset.transform)
            {
                DestroyImmediate(child.gameObject);
            }

            usdAsset.Reload(true);
        }

        public void SetImportProcessorMode(SampleMode mode)
        {
            ImportProcessorExample_CombineMeshes combineMeshesProcessor;
            ImportProcessorExample_SetHideFlags hideFlagsProcessor;
            usdAsset.TryGetComponent(out combineMeshesProcessor);
            usdAsset.TryGetComponent(out hideFlagsProcessor);

            switch (mode)
            {
                case SampleMode.CombineMeshes:
                    if (combineMeshesProcessor == null)
                        usdAsset.gameObject.AddComponent<ImportProcessorExample_CombineMeshes>();
                    if (hideFlagsProcessor != null)
                        DestroyImmediate(hideFlagsProcessor);
                    break;

                case SampleMode.SetHideFlags:
                    if (combineMeshesProcessor != null)
                        DestroyImmediate(combineMeshesProcessor);
                    if (hideFlagsProcessor == null)
                        usdAsset.gameObject.AddComponent<ImportProcessorExample_SetHideFlags>();
                    break;
            }
        }
    }
}
