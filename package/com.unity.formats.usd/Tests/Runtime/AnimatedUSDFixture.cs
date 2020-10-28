using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using USD.NET;
using Object = UnityEngine.Object;

namespace Unity.Formats.USD.Tests
{
     class AnimatedUsdFixture
    {
        readonly List<string> m_filesToDelete = new List<string>();
        UsdAsset m_usdRoot;
        
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            InitUsd.Initialize();
            
            var curve = AnimationCurve.Linear(0, 50, 1, 100);
            var clip = new AnimationClip();
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            clip.SetCurve("", typeof(Camera), "field of view", curve);

            CreateTimeline(out var director, out var timeline);
            
            var aTrack = timeline.CreateTrack<AnimationTrack>(null, "CubeAnimation");
            aTrack.CreateClip(clip).displayName = "CamClip";
            var cam = new GameObject("Camera").AddComponent<Camera>().gameObject;
            director.SetGenericBinding(aTrack, cam.AddComponent<Animator>());
            var usdRecordTrack = timeline.CreateTrack<UsdRecorderTrack>(null, "");
            var usdRecorderClip = usdRecordTrack.CreateDefaultClip();
            usdRecorderClip.start = 0;
            usdRecorderClip.duration = 2;
            var usdRecorderAsset = usdRecorderClip.asset as UsdRecorderClip;
            usdRecorderAsset.m_usdFile = "Assets/" + Path.GetFileNameWithoutExtension(Path.GetTempFileName())+".usd";
            m_filesToDelete.Add(usdRecorderAsset.m_usdFile);
            usdRecorderAsset.m_exportRoot = new ExposedReference<GameObject> {exposedName = Guid.NewGuid().ToString()};
            director.SetReferenceValue(usdRecorderAsset.m_exportRoot.exposedName, cam);
            Time.captureFramerate = (int)timeline.editorSettings.fps;
            director.Play();
            while (director.time <= 1.1)
                yield return null;
            director.Stop();
            yield return null;
            var stage = pxr.UsdStage.Open(usdRecorderAsset.m_usdFile, pxr.UsdStage.InitialLoadSet.LoadAll);
            var scene =  Scene.Open(stage);
            m_usdRoot = ImportSceneAsGameObject(scene).GetComponent<UsdAsset>();
            scene.Close();
            m_usdRoot.name = "Potato";
        }

        [Test]
        public void TestCamAnimation()
        {
            CreateTimeline(out var director, out var timeline);
            var usdTrack = timeline.CreateTrack<UsdPlayableTrack>(null,"");
            var clip = usdTrack.CreateDefaultClip();
            clip.duration = 2;
            var usdPlayableAsset = clip.asset as UsdPlayableAsset;
            usdPlayableAsset.SourceUsdAsset = new ExposedReference<UsdAsset> {exposedName = Guid.NewGuid().ToString()};
            director.SetGenericBinding(usdTrack, m_usdRoot);
            director.SetReferenceValue(usdPlayableAsset.SourceUsdAsset.exposedName, m_usdRoot);

            var cam = m_usdRoot.gameObject.GetComponent<Camera>();
            director.time = 0;
            director.Evaluate();

            Assert.That(50, Is.EqualTo(cam.transform.localPosition.x).Within(1e-5));
            Assert.That(50, Is.EqualTo(cam.fieldOfView).Within(1e-5));
            
            director.time = 1;
            director.Evaluate();
            Assert.That(100, Is.EqualTo(cam.transform.localPosition.x).Within(1e-5));
            Assert.That(100, Is.EqualTo(cam.fieldOfView).Within(1e-5));
            
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_usdRoot);
            foreach (var file in m_filesToDelete)
            {
                File.Delete(file);
            }
            m_filesToDelete.Clear();
        }

        static void CreateTimeline(out PlayableDirector director, out TimelineAsset timeline)
        {
            director = new GameObject("Timeline").AddComponent<PlayableDirector>();
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            director.playableAsset = timeline;
        }
        
        static GameObject ImportSceneAsGameObject(Scene scene, SceneImportOptions importOptions=null)
        {
            string path = scene.FilePath;

            // Time-varying data is not supported and often scenes are written without "Default" time
            // values, which makes setting an arbitrary time safer (because if only default was authored
            // the time will be ignored and values will resolve to default time automatically).
            scene.Time = 1.0;

            if (importOptions == null)
            {
                importOptions = new SceneImportOptions();
                importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
                importOptions.materialImportMode = MaterialImportMode.ImportDisplayColor;
                importOptions.usdRootPath = GetDefaultRoot(scene);
            }

            GameObject root = new GameObject(GetObjectName(importOptions.usdRootPath, path));

            if (Selection.gameObjects.Length > 0) {
                root.transform.SetParent(Selection.gameObjects[0].transform);
            }

            try
            {
                UsdToGameObject(root, scene, importOptions);
                return root;
            }
            catch (SceneImporter.ImportException)
            {
                Object.DestroyImmediate(root);
                return null;
            }
        }

        static pxr.SdfPath GetDefaultRoot(Scene scene) {
            // We can't safely assume the default prim is the model root, because Alembic files will
            // always have a default prim set arbitrarily.

            // If there is only one root prim, reference this prim.
            var children = scene.Stage.GetPseudoRoot().GetChildren().ToList();
            if (children.Count == 1) {
                return children[0].GetPath();
            }

            // Otherwise there are 0 or many root prims, in this case the best option is to reference
            // them all, to avoid confusion.
            return pxr.SdfPath.AbsoluteRootPath();
        }
        static GameObject UsdToGameObject(GameObject parent,
            Scene scene,
            SceneImportOptions importOptions) {
            try {
                SceneImporter.ImportUsd(parent, scene, new PrimMap(), importOptions);
            } finally {
                scene.Close();
            }

            return parent;
        }
        static string GetObjectName(pxr.SdfPath rootPrimName, string path) {
            return pxr.UsdCs.TfIsValidIdentifier(rootPrimName.GetName())
                ? rootPrimName.GetName()
                : GetObjectName(path);
        }
        static string GetObjectName(string path) {
            return IntrinsicTypeConverter.MakeValidIdentifier(Path.GetFileNameWithoutExtension(path));
        }
    }
}