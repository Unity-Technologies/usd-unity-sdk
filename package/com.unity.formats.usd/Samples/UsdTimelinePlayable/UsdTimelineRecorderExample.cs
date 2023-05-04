using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.Formats.USD.Examples
{
    [ExecuteAlways]
    public class UsdTimelineRecorderExample : MonoBehaviour
    {
        [HideInInspector]
        public bool m_playableDirectorExists = false;

        private void Update()
        {
            m_playableDirectorExists = this.GetComponent<PlayableDirector>() != null;
        }

        public void AddPlayableDirectorComponent()
        {
            this.gameObject.AddComponent<PlayableDirector>();
        }
    }
}
