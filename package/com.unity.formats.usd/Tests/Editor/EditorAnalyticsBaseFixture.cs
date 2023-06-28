# if UNITY_2023_2_OR_NEWER

using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using USD.NET;
using Newtonsoft.Json;

namespace Unity.Formats.USD.Tests
{
    public class EditorAnalyticsBaseFixture : BaseFixtureEditor
    {
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
            EditorAnalytics.enabled = m_initialAnalyticsSetting;
            EditorAnalytics.recordEventsEnabled = m_initialRecordEventsSetting;
            EditorAnalytics.SendAnalyticsEventsImmediately = m_initialSendEventsImmediatelySetting;
        }

        [SetUp]
        public void ResetEventList()
        {
            DebuggerEventListHandler.ClearEventList();
        }

        public IEnumerator WaitForUsdAnalytics(string expectedType, System.Action<AnalyticsEvent> expectedEvent, float attemptTimeLimit = 1f)
        {
            float currTimeCount = 0;
            bool found = false;

            while (!found && currTimeCount < attemptTimeLimit)
            {
                AnalyticsEvent expectedAnalyticsEvent = new AnalyticsEvent();
                foreach (var concattedEvent in DebuggerEventListHandler.fetchEventList())
                {
                    var splitEvents = concattedEvent.Split("\n");
                    var sharedEvent = JsonConvert.DeserializeObject<SharedAnalyticsEvent>(splitEvents[0]);
                    var usdEvent = JsonConvert.DeserializeObject<UsdAnalyticsEvent>(splitEvents[1]);

                    if (usdEvent.type == expectedType)
                    {
                        expectedAnalyticsEvent.shared = sharedEvent;
                        expectedAnalyticsEvent.usd = usdEvent;

                        expectedEvent(expectedAnalyticsEvent);
                        found = true;
                        break;
                    }
                }

                currTimeCount += Time.fixedDeltaTime;

                yield return null;
            }
        }
    }

    [System.Serializable]
    public class AnalyticsEvent
    {
        public SharedAnalyticsEvent shared;
        public UsdAnalyticsEvent usd;
    }

    [System.Serializable]
    public class SharedAnalyticsEvent
    {
        object common;
    }

    [System.Serializable]
    public class UsdAnalyticsEvent
    {
        public string type;
        public IMsgContent msg;

        public UsdAnalyticsEvent(string type, ImportAnalyticsMsg msg)
        {
            this.type = type;
            this.msg = msg;
        }
    }

    public interface IMsgContent
    { }

    [System.Serializable]
    public class ImportAnalyticsMsg : IMsgContent
    {
        public string FileExtension;
        public float TimeTakenMs;
        public bool Succeeded;
        public bool IncludesMeshes;
        public bool IncludesPointInstancer;
        public bool IncludesMaterials;
        public bool IncludesSkel;
    }
}
#endif
