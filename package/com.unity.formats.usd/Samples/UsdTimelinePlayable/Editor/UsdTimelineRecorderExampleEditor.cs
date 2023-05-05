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
            var labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleLeft, wordWrap = true };
            labelStyle.normal.textColor = SampleUtils.TextColor.Default;

            GUILayout.Label($"1. From the OS Menu, select 'Window' > 'Sequencing' > <color={SampleUtils.TextColor.Blue}>'Timeline'</color>.", labelStyle);
            GUILayout.Label($"2. From the 'Timeline Window' you can select the 'Play' icon button to play the animation associated with the USD asset {SampleUtils.SetTextColor(SampleUtils.TextColor.Blue, "UsdAssetWithTimeline")}.", labelStyle);
#endif
        }
    }
}
