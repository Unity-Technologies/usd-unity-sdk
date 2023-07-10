# if UNITY_2023_2_OR_NEWER

using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    public class EditorAnalyticsBaseFixture : BaseFixtureEditor
    {
        static bool m_PackageSetUp = false;
        protected const string k_TestPrefabName = "TestPrefab";

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

        // Unity has no OneTimeSetUp for IEnumerator, gotta use this instead
        [UnitySetUp]
        public IEnumerator SetUpAnalyticsDebuggerPackage()
        {
            if (!m_PackageSetUp)
            {
                m_PackageSetUp = true;
                yield return AddPackage.Add("com.unity.editor-analytics-debugger");
            }
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

        public IEnumerator WaitForUsdAnalytics<T>(string expectedType, System.Action<AnalyticsEvent> expectedEvent, float attemptFrameCountLimit = 150) where T: UsdAnalyticsEvent
        {
            bool found = false;
            float currFrameCount = 0;

            while (!found && currFrameCount < attemptFrameCountLimit)
            {
                AnalyticsEvent expectedAnalyticsEvent = new AnalyticsEvent();
                foreach (var concattedEvent in DebuggerEventListHandler.fetchEventList())
                {
                    if (!concattedEvent.Contains(expectedType))
                    {
                        continue;
                    }

                    Debug.Log($"Raw Log: {concattedEvent}");

                    var splitEvents = concattedEvent.Split("\n");
                    var usdIndex = FindUsdAnalyticsIndex(expectedType, splitEvents);

                    //var sharedEvent = JsonConvert.DeserializeObject<SharedAnalyticsEvent>(splitEvents[0]);
                    var usdEvent = JsonUtility.FromJson<T>(splitEvents[usdIndex]);

                    if (usdEvent.type.Contains(expectedType))
                    {
                        //expectedAnalyticsEvent.shared = sharedEvent;
                        expectedAnalyticsEvent.usd = usdEvent;

                        expectedEvent(expectedAnalyticsEvent);
                        found = true;
                        break;
                    }
                }

                DebuggerEventListHandler.ClearEventList();
                currFrameCount++;

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

    public class AnalyticsEvent
    {
        public SharedAnalyticsEvent shared;
        public UsdAnalyticsEvent usd;
    }

    public class SharedAnalyticsEvent
    {
        // check if the class for this is in Unity source (if its a bit convoluted just make it)
        object common;
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
