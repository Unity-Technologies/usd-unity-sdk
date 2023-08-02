# if UNITY_2023_3_OR_NEWER

using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorAnalyticsBaseFixture : BaseFixtureEditor
    {
        protected const string k_TestPrefabName = "TestPrefab";
        protected static string[] usdExtensions = new string[] { TestUtility.FileExtension.Usd, TestUtility.FileExtension.Usda, TestUtility.FileExtension.Usdc };

        protected struct UsdAnalyticsTypes
        {
            public const string Import = "editor.USDFileImport";
            public const string ReImport = "editor.USDFileReimport";
            public const string Export = "editor.USDFileExport";
        }

        public enum ImportMethods
        {
            AsGameObject,
            AsPrefab,
            AsTimelineRecording
        }

        bool m_initialAnalyticsSetting;
        bool m_initialRecordEventsSetting;
        bool m_initialSendEventsImmediatelySetting;

        [OneTimeSetUp]
        public void SetEditorAnalyticsSettings()
        {
            m_initialAnalyticsSetting = EditorAnalytics.enabled;
            m_initialRecordEventsSetting = EditorAnalytics.recordEventsEnabled;
            m_initialSendEventsImmediatelySetting = EditorAnalytics.SendAnalyticsEventsImmediately;

            EditorAnalytics.enabled = true;
            EditorAnalytics.recordEventsEnabled = true;
            EditorAnalytics.SendAnalyticsEventsImmediately = true;
        }

        [OneTimeTearDown]
        public void RevertEditorAnalyticsSettings()
        {
            // Maybe implmeent a package clean up here

            EditorAnalytics.enabled = m_initialAnalyticsSetting;
            EditorAnalytics.recordEventsEnabled = m_initialRecordEventsSetting;
            EditorAnalytics.SendAnalyticsEventsImmediately = m_initialSendEventsImmediatelySetting;
        }

        [SetUp]
        public void ResetEventList()
        {
            DebuggerEventListHandler.ClearEventList();
            InitUsd.Initialize();
        }

        public IEnumerator WaitForUsdAnalytics<T>(string expectedType, System.Action<T> actualEvent, float attemptTimeLimitMs = 1500) where T: UsdAnalyticsEvent
        {
            bool found = false;
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();

            while (!found && stopWatch.Elapsed.TotalMilliseconds < attemptTimeLimitMs)
            {
                T expectedAnalyticsEvent = null;
                foreach (var concattedEvent in DebuggerEventListHandler.fetchEventList())
                {
                    if (!concattedEvent.Contains(expectedType))
                    {
                        continue;
                    }

                    var splitEvents = concattedEvent.Split("\n");
                    var usdIndex = FindUsdAnalyticsIndex(expectedType, splitEvents);
                    var usdEvent = JsonUtility.FromJson<T>(splitEvents[usdIndex]);

                    if (usdEvent.type.Contains(expectedType))
                    {
                        expectedAnalyticsEvent = usdEvent;

                        actualEvent(expectedAnalyticsEvent);
                        found = true;
                        break;
                    }
                }

                DebuggerEventListHandler.ClearEventList();

                yield return null;
            }
        }

        private int FindUsdAnalyticsIndex(string expectedType, string[] splitEvents)
        {
            for (int index = 0; index < splitEvents.Length; index++)
            {
                if (splitEvents[index].Contains(expectedType))
                {
                    return index;
                }
            }

            return -1;
        }
    }

    public class UsdAnalyticsEvent
    {
        public string type;
    }

    public class UsdAnalyticsEventImport: UsdAnalyticsEvent
    {
        public UsdEditorAnalytics.ImportAnalyticsData msg;
    }

    public class UsdAnalyticsEventReImport : UsdAnalyticsEvent
    {
        public UsdEditorAnalytics.ReimportAnalyticsData msg;
    }

    public class UsdAnalyticsEventExport : UsdAnalyticsEvent
    {
        public UsdEditorAnalytics.ExportAnalyticsData msg;
    }
}
#endif
