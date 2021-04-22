#if RECORDER_AVAILABLE
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [RecorderSettings(typeof(UsdRecorder), "Usd Clip", "usd_recorder")]
    public class UsdRecorderSettings : RecorderSettings
    {
        [SerializeField] UsdRecorderInputSettings inputSettings = new();
        string m_extension;

        protected override string Extension
        {
            get { return m_extension; }
        }

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return inputSettings; }
        }
    }
}

#endif
