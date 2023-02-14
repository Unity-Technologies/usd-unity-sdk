// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using pxr;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace USD.NET.Tests
{
    class StageTests : UsdTests
    {
        [Test]
        public static void OpenFailTest()
        {
            Assert.Throws<ApplicationException>(delegate { USD.NET.Scene.Open(@"C:\Exist.foo"); },
                "Opening a non-usd file should raise");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception,
                "ApplicationException: USD ERROR: Failed to open layer @C:\\Exist.foo@");

            Assert.Throws<ApplicationException>(delegate { USD.NET.Scene.Open(@"C:\This\Doesnt\Exist.usd"); },
                "Opening a non existing file should raise");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception,
                "ApplicationException: USD ERROR: Failed to open layer @C:\\This\\Doesnt\\Exist.usd@");

            Assert.Throws<ApplicationException>(delegate { USD.NET.Scene.Create(@"C:\Exist.foo"); },
                "Creating a non-usd file should raise");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception,
                "ApplicationException: USD ERROR: Failed verification: ' fileFormat '");

            var invalidPath = "";
#if UNITY_EDITOR_WIN
            invalidPath = @"Z:\This\Doesnt\Exist.usd";
#else
            invalidPath = @"/,/This/Doesnt/Exist.usd";
#endif
            Assert.Throws<ApplicationException>(delegate { USD.NET.Scene.Create(invalidPath); },
                "Creating a file to a non existing path should raise");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception,
                "ApplicationException: USD ERROR: Cannot create path to write '" + invalidPath + "'");
        }

        public UsdStage CreateStageHierarchy()
        {
            //
            // Setup a stage with parents and children.
            // TODO: Add tests with classes and instancing.
            //
            UsdStage s = UsdStage.CreateInMemory();
            s.DefinePrim(new SdfPath("/Foo"));
            s.DefinePrim(new SdfPath("/Foo/P1"));
            s.DefinePrim(new SdfPath("/Foo/P2"));
            s.DefinePrim(new SdfPath("/Foo/P3"));
            s.DefinePrim(new SdfPath("/Foo/P4"));
            s.DefinePrim(new SdfPath("/Foo/P5"));
            s.DefinePrim(new SdfPath("/Bar"));
            s.DefinePrim(new SdfPath("/Bar/B1"));
            s.DefinePrim(new SdfPath("/Bar/B2"));
            s.DefinePrim(new SdfPath("/Bar/B3"));
            s.DefinePrim(new SdfPath("/Bar/B3/C1"));
            s.DefinePrim(new SdfPath("/Bar/B3/C2"));
            s.DefinePrim(new SdfPath("/Bar/B3/C3"));
            s.DefinePrim(new SdfPath("/Bar/B4"));

            return s;
        }

        [Test]
        public void TraverseChildrenTest()
        {
            var s = CreateStageHierarchy();
            var foo = s.GetPrimAtPath(new SdfPath("/Foo"));
            // Prim children
            Assert.AreEqual(5, foo.GetChildren().Count());
            int suffix = 1;
            foreach (UsdPrim curPrim in foo.GetChildren())
            {
                Assert.AreEqual("/Foo/P" + suffix, curPrim.GetPath().GetString());
                suffix += 1;
            }


            var bar = s.GetPrimAtPath(new SdfPath("/Bar"));
            Assert.AreEqual(4, bar.GetChildren().Count());
            suffix = 1;
            foreach (UsdPrim curPrim in bar.GetChildren())
            {
                Assert.AreEqual("/Bar/B" + suffix, curPrim.GetPath().GetString());
                suffix += 1;
            }
        }

        // [Test]
        // public void TraverseDescendantsTest()
        // {
        //     // Prim Descendants
        //     Assert.AreEqual(7, bar.GetDescendants().Count());
        //     suffix = 1;
        //     foreach (UsdPrim curPrim in bar.GetDescendants())
        //     {
        //         Assert.AreEqual("/Bar/B" + suffix, curPrim.GetPath().GetString());
        //         suffix += 1;
        //     }
        // }

        [Test]
        public void BasicTraversalTest()
        {
            var s = CreateStageHierarchy();
            List<UsdPrim> primList = s.GetAllPrims().ToList();
            int i = 0;
            foreach (UsdPrim curPrim in s.Traverse())
            {
                AssertEqual(primList[i++], curPrim);
            }
        }

        [Test]
        public void TraverseWithPruningTest()
        {
            var s = CreateStageHierarchy();
            var range = new USD.NET.RangeIterator(s.Traverse());
            var pruned = false;
            foreach (UsdPrim curPrim in range)
            {
                if (pruned)
                    Assert.AreEqual("/Bar/B4", curPrim.GetPath().GetString());

                if (curPrim.GetPath() == "/Bar/B3")
                {
                    range.PruneChildren();
                    pruned = true;
                }
            }
        }

        [Test]
        public void PrePostTraversalTest()
        {
            var s = CreateStageHierarchy();
            var prePostRange = new USD.NET.RangeIterator(UsdPrimRange.PreAndPostVisit(s.GetPseudoRoot()));
            bool[] expected = { false, false, true, false, true, true };
            bool[] actual = new bool[6];
            var i = 0;
            foreach (UsdPrim curPrim in prePostRange)
            {
                if (!prePostRange.IsPostVisit() && i > 0)
                {
                    // It's only valid to prune on the pre-traversal.
                    prePostRange.PruneChildren();
                }

                actual[i++] = prePostRange.IsPostVisit();
            }

            AssertEqual(expected, actual);
        }

        [Test]
        public static void ApiTest()
        {
            UsdStage s = UsdStage.CreateInMemory();
            var prim = s.DefinePrim(new SdfPath("/Foo"));
            s.SetStartTimeCode(1.0);
            s.SetEndTimeCode(10.0);
            AssertEqual(1.0, s.GetStartTimeCode());
            AssertEqual(10.0, s.GetEndTimeCode());

            var defultPrimToken = new TfToken("defaultPrim");
            var value = new VtValue();

            // Verify initial default prim / built-in metadata states.
            {
                Assert.IsFalse(s.HasDefaultPrim());
                Assert.IsFalse(s.GetDefaultPrim().IsValid());

                // defaultPrim is always valid metadata, it's built-in.
                Assert.IsTrue(s.HasMetadata(defultPrimToken));
                // But it should not yet have an *authored* value.
                Assert.IsFalse(s.HasAuthoredMetadata(defultPrimToken));
                // Even though it's not authored, reading it should succeed.
                Assert.IsTrue(s.GetMetadata(defultPrimToken, value));
                AssertEqual(((TfToken)value).ToString(), "");
            }

            // Set the default prim, which should set the defaultPrim metadata.
            s.SetDefaultPrim(prim);
            {
                // Verify via the Stage API
                Assert.IsTrue(s.HasDefaultPrim());
                Assert.IsTrue(s.GetDefaultPrim().IsValid());
                AssertEqual(s.GetDefaultPrim().GetPath(), new SdfPath("/Foo"));

                // Verify via the metadata API
                Assert.IsTrue(s.HasMetadata(defultPrimToken));
                Assert.IsTrue(s.HasAuthoredMetadata(defultPrimToken));
                Assert.IsTrue(s.GetMetadata(defultPrimToken, value));
                AssertEqual(((TfToken)value).ToString(), "Foo");
            }

            string str = s.GetRootLayer().ExportToString();

            s.ClearDefaultPrim();
            {
                Assert.IsFalse(s.HasDefaultPrim());
                Assert.IsFalse(s.GetDefaultPrim().IsValid());
                Assert.IsFalse(s.HasAuthoredMetadata(defultPrimToken));
                Assert.IsTrue(s.GetMetadata(defultPrimToken, value));
                AssertEqual(((TfToken)value).ToString(), "");
            }

            var classPrim = s.CreateClassPrim(new SdfPath("/Foo_class_"));
            {
                Assert.IsTrue(classPrim.IsValid());
                AssertEqual(classPrim.GetPath(), new SdfPath("/Foo_class_"));
            }

            AssertEqual(s.GetFramesPerSecond(), 24.0);
            s.SetFramesPerSecond(90.0);
            AssertEqual(s.GetFramesPerSecond(), 90.0);
        }

        [USD.NET.UsdSchema("Material")]
        public class MaterialSampleTest : USD.NET.SampleBase
        {
            [USD.NET.UsdNamespace("unity")]
            public USD.NET.Relationship surface = new USD.NET.Relationship();

            [USD.NET.UsdNamespace("unity")]
            public USD.NET.Relationship surfaces = new USD.NET.Relationship();
        }

        [Test]
        public static void RelationshipTest()
        {
            var shaderPath = "/Model/Materials/SimpleMat/StandardShader";

            var material = new MaterialSampleTest();
            material.surface = shaderPath;
            material.surfaces = new string[] { "/Foo", "/Bar/Baz" };

            var mat2 = new MaterialSampleTest();
            WriteAndRead(ref material, ref mat2);
            AssertEqual(material.surface.targetPaths, mat2.surface.targetPaths);
            AssertEqual(material.surfaces.targetPaths, mat2.surfaces.targetPaths);

            material.surface = "";
            material.surfaces = new string[0];
            WriteAndRead(ref material, ref mat2);
            AssertEqual(material.surface.targetPaths, mat2.surface.targetPaths);
            AssertEqual(material.surfaces.targetPaths, mat2.surfaces.targetPaths);

            material.surface.targetPaths = null;
            material.surfaces.targetPaths = null;
            mat2.surface = new USD.NET.Relationship();
            mat2.surfaces = new USD.NET.Relationship();
            WriteAndRead(ref material, ref mat2);
            AssertEqual(material.surface.targetPaths, mat2.surface.targetPaths);
            AssertEqual(material.surfaces.targetPaths, mat2.surfaces.targetPaths);
        }

        [Test]
        public static void BadPrimTest()
        {
            var stage = UsdStage.CreateInMemory();
            var prim = stage.GetPrimAtPath(new SdfPath("/Foo"));
            Assert.IsTrue(prim != null);
            Assert.IsTrue(prim.IsValid() == false);
            stage.Dispose();
        }

        [Test]
        public static void GetAllPathByType_IgnoreAbstract()
        {
            var s = UsdStage.CreateInMemory();
            var meshToken = new TfToken("Mesh");

            var classPrim = s.DefinePrim(new SdfPath("/thisIsAClass"), meshToken);
            classPrim.SetSpecifier(SdfSpecifier.SdfSpecifierClass);

            var instancePrim = s.DefinePrim(new SdfPath("/inheritedPrim"));
            instancePrim.GetInherits().AddInherit(classPrim.GetPath());

            var meshPrim = s.DefinePrim(new SdfPath("/meshPrim"), meshToken);

            Assert.True(classPrim.IsAbstract());
            Assert.AreEqual(meshToken, classPrim.GetTypeName());
            Assert.False(instancePrim.IsAbstract());
            Assert.AreEqual(meshToken, instancePrim.GetTypeName());
            Assert.AreEqual(meshToken, meshPrim.GetTypeName());
            var allPaths = s.GetAllPathsByType(meshToken, SdfPath.AbsoluteRootPath());
            Assert.AreEqual(2, allPaths.Count);
        }
    }
}
