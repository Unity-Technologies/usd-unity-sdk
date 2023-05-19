using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    class AnimationRecordingTests : BaseFixtureRuntime
    {
        [UnityTest]
        public IEnumerator TestExportSparseTimesampling()
        {
            // Create the necessary objects
            // Cube is animated by an animation clip, Cylinder by a rigidbody,
            // and Sphere is not animated.
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.AddComponent<Rigidbody>();
            // parent sphere and cylinder under cube
            sphere.transform.SetParent(cube.transform);
            cylinder.transform.SetParent(cube.transform);

            // create simple animation clip (to animate the cube)
            var curve = AnimationCurve.Linear(0, 50, 1, 100);
            var clip = new AnimationClip();
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

            // create and setup the timeline
            CreateTimeline(out var director, out var timeline);

            var aTrack = timeline.CreateTrack<AnimationTrack>(null, "CubeAnimation");
            aTrack.CreateClip(clip).displayName = "CubeClip";

            // bind the timeline track to the cube
            director.SetGenericBinding(aTrack, cube.AddComponent<Animator>());

            // add usd recording track
            var usdRecordTrack = timeline.CreateTrack<UsdRecorderTrack>(null, "");
            var usdRecorderClip = usdRecordTrack.CreateDefaultClip();
            usdRecorderClip.start = 0;
            usdRecorderClip.duration = 2;
            var usdRecorderAsset = usdRecorderClip.asset as UsdRecorderClip;

            // set path to record to
            var recordedUsdFile = TestUtility.GetUSDScenePath(ArtifactsDirectoryFullPath);
            usdRecorderAsset.m_usdFile = recordedUsdFile;

            usdRecorderAsset.m_exportRoot = new ExposedReference<GameObject> { exposedName = Guid.NewGuid().ToString() };
            director.SetReferenceValue(usdRecorderAsset.m_exportRoot.exposedName, cube);
            Time.captureFramerate = (int)timeline.editorSettings.fps;

            // record to USD
            director.Play();
            while (director.time <= 1.1)
                yield return null;
            director.Stop();
            yield return null;

            Assert.That(recordedUsdFile, Does.Exist);
            // Check that the Cube and Cylinder have timesamples, but not the Sphere (not animated)
            var stage = pxr.UsdStage.Open(recordedUsdFile, pxr.UsdStage.InitialLoadSet.LoadAll);
            var scene = Scene.Open(stage);
            var cubePath = "/Cube";
            var spherePath = "/Cube/Sphere";
            var cylinderPath = "/Cube/Cylinder";

            var keyframeDict = scene.ComputeKeyFrames(cubePath, attribute: "xformOp:transform");
            // there should be transform keyframes for cube and cylinder but not sphere
            Assert.That(keyframeDict.ContainsKey(cubePath), Is.True, "Missing timesamples for Cube in recorded usd");
            Assert.That(keyframeDict.ContainsKey(cylinderPath), Is.True, "Missing timesamples for Cylinder in recorded usd");
            Assert.That(keyframeDict.ContainsKey(spherePath), Is.False, "There should not be timesamples for Sphere in the recorded usd");

            scene.Close();
        }

        static void CreateTimeline(out PlayableDirector director, out TimelineAsset timeline)
        {
            director = new GameObject("Timeline").AddComponent<PlayableDirector>();
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            director.playableAsset = timeline;
        }
    }
}
