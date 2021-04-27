using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.Formats.USD;
using UnityEditor.Formats.USD.Recorder;
using UnityEditor.Recorder.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using USD.NET;
using USD.NET.Unity;

namespace DefaultNamespace
{
    class TimelineDataTests
    {
        PlayableDirector director;
        GameObject cube;
        UsdRecorderSettings usdSettings;
        List<string> deleteFileList = new List<string>();

        [SetUp]
        public void SetUp()
        {
            var curve = AnimationCurve.Linear(0, 0, 10, 10);
            var clip = new AnimationClip {hideFlags = HideFlags.DontSave};
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.hideFlags = HideFlags.DontSave;
            var aTrack = timeline.CreateTrack<AnimationTrack>(null, "CubeAnimation");
            aTrack.CreateClip(clip).displayName = "CubeClip";

            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.AddComponent<Animator>();
            director = cube.AddComponent<PlayableDirector>();
            director.playableAsset = timeline;
            director.SetGenericBinding(aTrack, cube);

            var rTrack = timeline.CreateTrack<RecorderTrack>();
            var rAsset = rTrack.CreateDefaultClip().asset as RecorderClip;
            usdSettings = ScriptableObject.CreateInstance<UsdRecorderSettings>();
            rAsset.settings = usdSettings;

            var outputFile = Path.Combine(Application.dataPath, "..", "SampleRecordings", "test");
            usdSettings.OutputFile = outputFile;
            usdSettings.BasisTransformation = BasisTransformation.None;
            usdSettings.Scale = 1;

            var iSettings = usdSettings.InputsSettings.First() as UsdRecorderInputSettings;
            iSettings.GameObject = cube;

            deleteFileList.Add(outputFile + ".usd");
        }

        [TearDown]
        public void TearDown()
        {
            // For some reason Yamato throws System.UnauthorizedAccessException 
        /*    foreach (var file in deleteFileList)
            {
                File.Delete(file);
            }

            deleteFileList.Clear();*/
        }

        [UnityTest]
        public IEnumerator UsdRecorder_Records()
        {
            var sampleT = 3f;
            director.Play();
            while (Time.time < director.duration )
            {
                yield return null;
            }

            var scene = USD.NET.Scene.Open(usdSettings.OutputFile + ".usd");
            var sample = new XformSample();
            scene.Time = sampleT * scene.FrameRate; // Frames not seconds
            scene.Read("/Cube", sample);

            Assert.That(sampleT, Is.EqualTo(sample.transform.m03).Within(1e-5));

        }
    }
}
