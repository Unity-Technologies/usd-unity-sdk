#if RECORDER_AVAILABLE
using System;
using System.Collections.Generic;
using UnityEditor.Recorder;
using UnityEngine;

namespace UnityEditor.Formats.USD.Recorder
{
    [Serializable]
    public class UsdRecorderInputSettings : RecorderInputSettings
    {
        [SerializeField] string m_BindingId = null;
        public GameObject GameObject
        {
            get
            {
                if (string.IsNullOrEmpty(m_BindingId))
                {
                    return null;
                }

                return BindingManager.Get(m_BindingId) as GameObject;
            }

            set
            {
                if (string.IsNullOrEmpty(m_BindingId))
                {
                    m_BindingId = GUID.Generate().ToString();
                }

                BindingManager.Set(m_BindingId, value);
            }
        }

        protected override bool ValidityCheck(List<string> errors)
        {
            if (GameObject == null)
            {
                errors.Add("GameObject cannot be null");
                return false;
            }

            return true;
        }

        protected override Type InputType => typeof(UsdRecorderInput);
    }
}
#endif
