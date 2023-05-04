using UnityEditor;
using UnityEngine;
#if UNITY_TIMELINE
using UnityEngine.Timeline;
using UnityEditor.Timeline;
#endif


namespace Unity.Formats.USD.Examples
{
    [CustomEditor(typeof(UsdTimelineRecorderExample))]
    public class UsdTimelineRecorderExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Empty blank line
            GUILayout.Label("");

#if !UNITY_TIMELINE
            var labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            labelStyle.normal.textColor = Color.red;

            GUILayout.Label($"Unity 'Timeline' package is required for this sample.\nPlease install the <b>'Timeline' package (com.unity.timeline)</b> from the 'Unity Package Manager'", labelStyle);
            if (GUILayout.Button("Link"))
            {
                Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/index.html");
            }
#else
            var script = (UsdTimelineRecorderExample)target;

            if (!script.m_playableDirectorExists)
            {
                GUILayout.Label($"First you need to:");
                if (GUILayout.Button("Add 'Playable Director' Component"))
                {
                    script.AddPlayableDirectorComponent();
                }
            }
            else
            {
                var labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleLeft, wordWrap = true };
                labelStyle.normal.textColor = SampleUtils.TextColor.Default;

                GUILayout.Label($"To Play animation that exists within a USD file imported as a GameObject within Unity:", labelStyle);
                GUILayout.Label($"  1. From the OS Menu, select 'Window' > 'Sequencing' > <color={SampleUtils.TextColor.Blue}>'Timeline'</color>.", labelStyle);
                GUILayout.Label($"  2. From the 'Timeline Window' select the <color={SampleUtils.TextColor.Blue}>'Create'</color> Button.", labelStyle);
                GUILayout.Label($"  3. Finish the create step by saving the '.playble' file within your current project.", labelStyle);
                GUILayout.Label($"  4. Within the 'Timeline Window' Right Click within any <b>empty area</b> of the window, and select Unity.Formats.USD > <color={SampleUtils.TextColor.Yellow}>'Usd Playable Track'</color>.", labelStyle);
                GUILayout.Label($"  5. Set the <color={SampleUtils.TextColor.Blue}>Source Object</color> within the newly created <b>'Usd Playable Track'</b> as the USD GameObject, <color={SampleUtils.TextColor.Yellow}>'UsdAssetWithTimeline'</color> (first child of this object).", labelStyle);
                GUILayout.Label($"  6. Within the 'Timeline Window' Right Click within the newly created <b>Usd Playable Track</b>, and select <color={SampleUtils.TextColor.Yellow}>'Add From Usd Asset'</color>.", labelStyle);
                GUILayout.Label($"  7. In the new pop up <color={SampleUtils.TextColor.Blue}>'Select Usd Asset'</color> window select the USD GameObject, <color={SampleUtils.TextColor.Yellow}>'UsdAssetWithTimeline'</color>.", labelStyle);
                GUILayout.Label($"  8. You can now play the Timeline animation and see the animation being played in your <b>Scene View</b> window.", labelStyle);
            }
#endif
        }
    }
}
