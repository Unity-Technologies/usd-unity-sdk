# if UNITY_2023_2_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using USD.NET;
using UnityEngine.TestTools;
using Newtonsoft.Json;

namespace Unity.Formats.USD.Tests
{
    public class EditorAnalyticsTests : EditorAnalyticsBaseFixture
    {
        Scene m_scene;

        #region Import

        [UnityTest]
        public IEnumerator OnValidImport_AnalyticsAreSent()
        {
            m_scene = CreateTestUsdScene();
            ImportHelpers.ImportAsPrefab(m_scene, GetPrefabPath());

            AnalyticsEvent expectedEvent = null;
            yield return (WaitForUsdAnalytics("editor.USDFileImport.v1", (AnalyticsEvent waitedEvent) => {
                expectedEvent = waitedEvent;
            }));

            Assert.IsNotNull(expectedEvent);
            Assert.IsTrue(((ImportAnalyticsMsg)expectedEvent.usd.msg).Succeeded);
        }

        [UnityTest]
        public IEnumerator 

        // Analytics for a small / empty import are empty but valid

        // Analytics for a failed import have correct status

        // Analytics have correct file ending for valid files imported

        // Analytics for valid import contains non-zero timestamp

        // Analytics for failed import contains non-zero timestamp

        // Analytics for a mesh 'contains mesh'

        // Analytics for a material 'contains material'

        // Analytics for a PointInstancer 'contains pointinstancer'

        // Analytics for a skeleton 'contains skeleton'

        #endregion

        // IMPORT


        // REIMPORT

        // as above, for full and partial reimports

        // EXPORT
        // sending valid export analytics returns ok

        // Analytics for a small / empty export are empty but valid

        // Analytics for a failed export have correct status

        // Analytics have correct file ending for valid files exported

        // Analytics for valid export contains non-zero timestamp

        // Analytics for failed export contains non-zero timestamp

        // Analytics for a Transform Overrides Only export 'contains transform overrides only = true'

        // RECORDER
        // sending valid recorder export analytics returns ok

        // Analytics for valid recorder export contains success status

        // Analytics have correct file ending for valid files exported

        // Analytics for a Transform Overrides Only recorder export 'contains transform overrides only = true'

        // Analytics for valid recorder export contains correct number of frames
    }
}
#endif
