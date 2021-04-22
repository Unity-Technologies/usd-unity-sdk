#if RECORDER_AVAILABLE
using System;
using System.Collections.Generic;
using UnityEditor.Recorder;

namespace UnityEditor.Formats.USD.Recorder
{
    [Serializable]
    public class UsdRecorderInputSettings: RecorderInputSettings
    {
        protected override bool ValidityCheck(List<string> errors)
        {
            return true;
        }

        protected override Type InputType => typeof(UsdRecorderInput);
    }
}
#endif
