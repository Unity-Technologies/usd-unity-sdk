// Copyright 2018 Jeremy Cowles. All rights reserved.
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

using System;
using System.Collections.Generic;
using USD.NET;
using UnityEngine;
using NUnit.Framework;

namespace USD.NET.Tests
{
    [System.Serializable]
    [UsdSchema("UsdGeomXformable")]
    class XformableQuery : SampleBase
    {
        public XformableQuery()
        {
        }
    }

    [System.Serializable]
    [UsdSchema("UsdGeomCube")]
    class CubeSample : SampleBase
    {
    }

    [System.Serializable]
    [UsdSchema("UsdGeomMesh")]
    class MeshSample : SampleBase
    {
        public Visibility visibility;
    }

    [System.Serializable]
    [UsdSchema("BOGUS_TYPE_INTENDED_TO_THROW_ERROR")]
    class BadBaseTypeQuery : SampleBase
    {
        public BadBaseTypeQuery()
        {
        }
    }

    class QueryTests : UsdTests
    {
        [Test]
        public static void BasicTest()
        {
            var cubeSample = new CubeSample();
            var meshSample = new MeshSample();
            meshSample.visibility = Visibility.Invisible;
            var scene = Scene.Create();

            scene.Write("/Root/Cube", cubeSample);
            scene.Write("/Root/Mesh", meshSample);
            scene.Write("/Root/Mesh2", meshSample);

            var paths = new List<string>();
            foreach (var mesh in scene.ReadAll<XformableQuery>(rootPath: "/Root"))
            {
                paths.Add(mesh.path);
            }
            Assert.AreEqual(3, paths.Count);
            paths.Clear();


            foreach (var path in scene.Find<XformableQuery>(rootPath: "/Root"))
            {
                paths.Add(path);
            }
            Assert.AreEqual(3, paths.Count);
            paths.Clear();


            foreach (var mesh in scene.ReadAll<XformableQuery>(rootPath: "/Bogus/Root/Path"))
            {
                paths.Add(mesh.path);
            }
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "ApplicationException: USD ERROR: Invalid root path </Bogus/Root/Path>");
            paths.Clear();


            foreach (var mesh in scene.ReadAll<BadBaseTypeQuery>(rootPath: "/Root"))
            {
                paths.Add(mesh.path);
            }
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "ApplicationException: USD ERROR: Base type 'BOGUS_TYPE_INTENDED_TO_THROW_ERROR' was not known to the TfType system");
            paths.Clear();
        }
    }
}
