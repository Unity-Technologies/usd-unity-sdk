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
# if ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_EDITOR
# define USE_EDITOR_ANALYTICS
# endif

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Unity.Formats.USD
{
    public enum ImportType
    {
        Initial,
        ForceRebuild,
        Refresh,
        // Do not send analytics when streaming, as they get sent for every frame.
        Streaming
    }
    public static class UsdEditorAnalytics
    {
        // General analytics
        const int k_MaxEventsPerHour = 1000;
        const int k_MaxNumberOfElements = 100;
        const string k_VendorKey = "unity.formats.usd";

        // Universal USD Analytics
        const string k_UsageEventName = "USDPackageUsage";

        struct UsageAnalyticsData
        {
            public bool InitSucceeded;
            public double TimeTakenMs;
        }

        // USD Import Analytics
        const string k_ImportEventName = "USDFileImport";

        struct ImportAnalyticsData
        {
            public string FileExtension;
            public double TimeTakenMs;
            public bool Succeeded;
            public bool IncludesMeshes;
            public bool IncludesPointInstancer;
            public bool IncludesMaterials;
            public bool IncludesSkel;
        }

        public struct ImportResult
        {
            public bool Success;
            public ImportType ImportType;
            public bool ContainsMeshes;
            public bool ContainsPointInstancer;
            public bool ContainsSkel;
            public bool ContainsMaterials;

            public static ImportResult Default => new ImportResult()
            {
                Success = false,
                ImportType = ImportType.Initial,
                ContainsMeshes = false,
                ContainsPointInstancer = false,
                ContainsSkel = false,
                ContainsMaterials = false
            };
        };

        // USD Reimport Analytics
        const string k_ReimportEventName = "USDFileReimport";

        struct ReimportAnalyticsData
        {
            public string FileExtension;
            public double TimeTakenMs;
            public bool Succeeded;
            public bool ForceRebuild;
            public bool IncludesMeshes;
            public bool IncludesPointInstancer;
            public bool IncludesMaterials;
            public bool IncludesSkel;
        }

        // USD Export Analytics
        const string k_ExportEventName = "USDFileExport";

        struct ExportAnalyticsData
        {
            public string FileExtension;
            public double TimeTakenMs;
            public bool Succeeded;
            public bool OnlyOverrides;
        }

        // USD Recorder Export Analytics
        const string k_RecorderExportEventName = "USDFileRecorderExport";

        struct RecorderExportAnalyticsData
        {
            public string FileExtension;
            public double TimeTakenMs;
            public bool Succeeded;
            public bool OnlyOverrides;
            public int FrameCount;
        }

        // key = event name
        // value (bool) = whether the event is already registered
        public static Dictionary<string, bool> sUsdEditorAnalyticsEvents = new Dictionary<string, bool>()
        {
            { k_UsageEventName, false },
            { k_ImportEventName, false },
            { k_ReimportEventName, false },
            { k_ExportEventName, false },
            { k_RecorderExportEventName, false }
        };

        static bool RegisterAnalytics(string eventName)
        {
# if USE_EDITOR_ANALYTICS
            // If EditorAnalytics are disabled, don't waste cycles doing any of the set up
            if (!EditorAnalytics.enabled)
                return false;

            if (sUsdEditorAnalyticsEvents[eventName])
                return true;

            AnalyticsResult result = EditorAnalytics.RegisterEventWithLimit(eventName, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey);
            if (result == AnalyticsResult.Ok)
            {
                sUsdEditorAnalyticsEvents[eventName] = true;
                return true;
            }
            else
            {
                Debug.LogError($"Failed to register EditorAnalytics event '{eventName}'. Reason: {result}.");
            }
# endif
            return false;
        }

        public static void SendUsageEvent(bool success, double timeTakenMs)
        {
# if USE_EDITOR_ANALYTICS
            if (!RegisterAnalytics(k_UsageEventName))
                return;

            var data = new UsageAnalyticsData()
            {
                InitSucceeded = success,
                TimeTakenMs = timeTakenMs
            };

            AnalyticsResult result = EditorAnalytics.SendEventWithLimit(k_UsageEventName, data);
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to send EditorAnalytics event '{k_UsageEventName}'. Reason: {result}.");
            }
# endif
        }

        public static void SendImportEvent(string fileExtension, double timeTakenMs, ImportResult importResult)
        {
# if USE_EDITOR_ANALYTICS
            if (!RegisterAnalytics(k_ImportEventName))
                return;

            var data = new ImportAnalyticsData()
            {
                FileExtension = fileExtension,
                TimeTakenMs = timeTakenMs,
                Succeeded = importResult.Success,
                IncludesMeshes = importResult.ContainsMeshes,
                IncludesPointInstancer = importResult.ContainsPointInstancer,
                IncludesMaterials = importResult.ContainsMaterials,
                IncludesSkel = importResult.ContainsSkel
            };

            AnalyticsResult result = EditorAnalytics.SendEventWithLimit(k_ImportEventName, data);
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to send EditorAnalytics event '{k_ImportEventName}'. Reason: {result}.");
            }
# endif
        }

        public static void SendReimportEvent(string fileExtension, double timeTakenMs, ImportResult importResult)
        {
# if USE_EDITOR_ANALYTICS
            if (!RegisterAnalytics(k_ReimportEventName))
                return;

            var data = new ReimportAnalyticsData()
            {
                FileExtension = fileExtension,
                TimeTakenMs = timeTakenMs,
                Succeeded = importResult.Success,
                ForceRebuild = importResult.ImportType == ImportType.ForceRebuild,
                IncludesMeshes = importResult.ContainsMeshes,
                IncludesPointInstancer = importResult.ContainsPointInstancer,
                IncludesMaterials = importResult.ContainsMaterials,
                IncludesSkel = importResult.ContainsSkel
            };

            AnalyticsResult result = EditorAnalytics.SendEventWithLimit(k_ReimportEventName, data);
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to send EditorAnalytics event '{k_ReimportEventName}'. Reason: {result}.");
            }
# endif
        }

        public static void SendExportEvent(string fileExtension, double timeTakenMs, bool exportSucceeded, bool onlyOverrides = false)
        {
# if USE_EDITOR_ANALYTICS
            if (!RegisterAnalytics(k_ExportEventName))
                return;

            var data = new ExportAnalyticsData()
            {
                FileExtension = fileExtension,
                TimeTakenMs = timeTakenMs,
                Succeeded = exportSucceeded,
                OnlyOverrides = onlyOverrides
            };

            AnalyticsResult result = EditorAnalytics.SendEventWithLimit(k_ExportEventName, data);
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to send EditorAnalytics event '{k_ExportEventName}'. Reason: {result}.");
            }
# endif
        }

        public static void SendRecorderExportEvent(string fileExtension, double timeTakenMs, bool exportSucceeded, bool onlyOverrides = false, int frameCount = 0)
        {
# if USE_EDITOR_ANALYTICS
            if (!RegisterAnalytics(k_RecorderExportEventName))
                return;

            var data = new RecorderExportAnalyticsData()
            {
                FileExtension = fileExtension,
                TimeTakenMs = timeTakenMs,
                Succeeded = exportSucceeded,
                OnlyOverrides = onlyOverrides,
                FrameCount = frameCount
            };

            AnalyticsResult result = EditorAnalytics.SendEventWithLimit(k_RecorderExportEventName, data);
            if (result != AnalyticsResult.Ok)
            {
                Debug.LogError($"Failed to send EditorAnalytics event '{k_RecorderExportEventName}'. Reason: {result}.");
            }
# endif
        }
    }
}
