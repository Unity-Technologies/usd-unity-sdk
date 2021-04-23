#if RECORDER_AVAILABLE
using System;
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [RecorderSettings(typeof(UsdRecorder), "Usd Clip", "usd_recorder")]
    public class UsdRecorderSettings : RecorderSettings
    {
        [SerializeField] UsdRecorderInputSettings inputSettings = new();
        [SerializeField] Format exportFormat;
        internal enum Format
        {
            Usd,
            UsdZ,
        }

        protected override string Extension
        {
            get
            {
                return exportFormat switch
                {
                    Format.Usd => "usd",
                    Format.UsdZ => "usdz",
                    _ => throw new ArgumentException("Unhandled format")
                };
            }
        }

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return inputSettings; }
        }

        internal Format ExportFormat => exportFormat;
    }
}

#endif
