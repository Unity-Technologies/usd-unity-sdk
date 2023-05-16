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

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;

namespace Unity.Formats.USD
{
    public static class UsdEditorAnalytics
    {
        // General analytics
        const int k_MaxEventsPerHour = 1000;
        const int k_MaxNumberOfElements = 1000;
        const string k_VendorKey = "unity.formats.usd";

        // Universal USD Analytics
        // This may not be necessary as it may be something already collected. If it is, it should be moved to the
        // runtime side so that it can be sent by InitUsd.Initialize().
        const string k_UsageEventName = "uUSDPackageUsage";

        struct UsageAnalyticsData
        {
            public string UnityVersion;
        }

        // USD Import Analytics
        const string k_ImportEventName = "uUSDFileImport";

        struct ImportAnalyticsData
        {
            public string FileExtension;
            public bool ImportSucceeded; // VRC: not sure this is actually useful, as we have no context why it failed.
        }

        // USD Export Analytics
        const string k_ExportEventName = "uUSDFileExport";

        struct ExportAnalyticsData
        {
            public string FileExtension;
            public bool ExportSucceeded; // VRC: not sure this is actually useful, as we have no context why it failed.
            public bool OnlyOverrides;
        }

        public static Dictionary<string, bool> sUsdEditorAnalyticsEvents = new Dictionary<string, bool>()
        {
            { k_UsageEventName, false },
            { k_ImportEventName, false },
            { k_ExportEventName, false }
        };

        static bool EnableAnalytics()
        {
            bool returnValue = true;
            AnalyticsResult result;

            foreach (var analyticsEvent in sUsdEditorAnalyticsEvents)
            {
                result = EditorAnalytics.RegisterEventWithLimit(analyticsEvent.Key, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey);
                if (result == AnalyticsResult.Ok)
                    sUsdEditorAnalyticsEvents[analyticsEvent.Key] = true;
                returnValue &= analyticsEvent.Value;
            }

            return returnValue; // VRC: Not sure this is particularly necessary, could return nothing and just use the dictionary.
        }

        // VRC: not currently used as InitUsd would be a more sensible place but cannot reach here.
        // We might already be able to get Unity version from exisitng data collections.
        public static void SendUsageEvent()
        {
            if (!EditorAnalytics.enabled)
                return;

            if (!sUsdEditorAnalyticsEvents[k_UsageEventName] && !EnableAnalytics())
                return;

            var data = new UsageAnalyticsData()
            {
                UnityVersion = Application.unityVersion
            };

            EditorAnalytics.SendEventWithLimit(k_UsageEventName, data);
        }

        public static void SendImportEvent(string fileExtension, bool importSucceeded)
        {
            if (!EditorAnalytics.enabled)
                return;

            if (!sUsdEditorAnalyticsEvents[k_ImportEventName] && !EnableAnalytics())
                return;

            var data = new ImportAnalyticsData()
            {
                FileExtension = fileExtension,
                ImportSucceeded = importSucceeded
            };

            EditorAnalytics.SendEventWithLimit(k_ImportEventName, data);
        }

        public static void SendExportEvent(string fileExtension, bool exportSucceeded, bool onlyOverrides = false)
        {
            if (!EditorAnalytics.enabled)
                return;

            if (!sUsdEditorAnalyticsEvents[k_ExportEventName] && !EnableAnalytics())
                return;

            var data = new ExportAnalyticsData()
            {
                FileExtension = fileExtension,
                ExportSucceeded = exportSucceeded,
                OnlyOverrides = onlyOverrides
            };

            EditorAnalytics.SendEventWithLimit(k_ExportEventName, data);
        }
    }
}
