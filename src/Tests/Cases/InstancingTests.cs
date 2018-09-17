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
using System.Linq;
using USD.NET.Unity;
using System.Collections.Generic;

namespace Tests.Cases {
  class InstancingTests : UnitTest {

    public static void TraversalTest() {
      var scene = USD.NET.Scene.Open(@"D:\usd\Kitchen_set.usd");
      
      foreach (pxr.UsdPrim prim in scene.Stage.Traverse()) {
        var mesh = new pxr.UsdGeomMesh(prim);
        if (mesh) {
          Console.WriteLine("Non-instanced mesh: " + prim.GetPath());
        }

        //
        // See if this is instanced.
        //
        if (prim.IsInstance()) {
          Console.WriteLine("Instanced prim: " + prim.GetPath() + "\n\tMaster: " + prim.GetMaster().GetPath());
          
          //
          // Now traverse the master.
          //
          var master = prim.GetMaster();
          foreach (pxr.UsdPrim masterPrim in master.GetAllDescendants()) {
            var masterMesh = new pxr.UsdGeomMesh(masterPrim);
            if (masterMesh) {
              Console.WriteLine("\tMaster mesh: " + masterPrim.GetPath());
            }
          }
        }
      }
    }

    public static void CreatePointInstancerManualTest() {
      var scene = USD.NET.Scene.Create();
      var stageWeakRef = new pxr.UsdStageWeakPtr(scene.Stage);
      var pi = pxr.UsdGeomPointInstancer.Define(stageWeakRef, new pxr.SdfPath("/Instancer"));
      var cube = pxr.UsdGeomCube.Define(stageWeakRef, new pxr.SdfPath("/Instancer/Cube"));

      // The cube is the only prototype.
      pi.CreatePrototypesRel().AddTarget(cube.GetPath());

      // Create three instances of the cube (proto index=0).
      var intArray = new pxr.VtIntArray(3, 0);
      pi.CreateProtoIndicesAttr().Set(intArray);

      var mesh = new pxr.UsdGeomMesh();
      var meshSamples = new USD.NET.Unity.MeshSample[0];

      // Create three positions.
      var vec3fArray = new pxr.VtVec3fArray(3);
      vec3fArray[0] = new pxr.GfVec3f(-2.5f, 0, 0);
      vec3fArray[1] = new pxr.GfVec3f(0, 0, 0);
      vec3fArray[2] = new pxr.GfVec3f(2.5f, 0, 0);
      pi.CreatePositionsAttr().Set(vec3fArray);

      scene.Stage.Export("D:\\instancer.usda");

      //
      // Compute the root transform for each instance.
      //
      var xforms = new pxr.VtMatrix4dArray(3);

      pi.ComputeInstanceTransformsAtTime(xforms, time: 1.0, baseTime: 0.0);

      for (int i = 0; i < xforms.size(); i++) {
        Console.WriteLine(xforms[i]);
      }
    }

    [System.Serializable]
    [USD.NET.UsdSchema("PointInstancer")]
    class PointInstancerSample : USD.NET.SampleBase {
      public int[] protoIndices;
      public USD.NET.Relationship prototypes = new USD.NET.Relationship();
      public UnityEngine.Vector3[] positions;
      public UnityEngine.Quaternion[] rotations;
      public UnityEngine.Vector3[] scales;

      public UnityEngine.Matrix4x4[] ComputeInstanceMatrices(USD.NET.Scene scene, string primPath) {
        var prim = scene.GetPrimAtPath(primPath);
        var pi = new pxr.UsdGeomPointInstancer(prim);
        var xforms = new pxr.VtMatrix4dArray();

        pi.ComputeInstanceTransformsAtTime(xforms, scene.Time == null ? pxr.UsdTimeCode.Default() : scene.Time, 0);

        // Slow, but works.
        var matrices = new UnityEngine.Matrix4x4[xforms.size()];
        for (int i = 0; i < xforms.size(); i++) {
          matrices[i] = UnityTypeConverter.FromMatrix(xforms[i]);
        }
        return matrices;
      }
    }

    public static void PointInstancerTest() {
      var scene = USD.NET.Scene.Create();
      var stageWeakRef = new pxr.UsdStageWeakPtr(scene.Stage);
      var pi = new PointInstancerSample();
      var cube = new CubeSample();

      pi.prototypes.targetPaths = new string[] { "/Instancer/Cube" };

      // Three instances, all prototype index zero.
      pi.protoIndices = new int[3];
      pi.positions = new UnityEngine.Vector3[3];
      pi.positions[0] = new UnityEngine.Vector3(-2.5f, 0, 0);
      pi.positions[1] = new UnityEngine.Vector3(0, 0, 0);
      pi.positions[2] = new UnityEngine.Vector3(2.5f, 0, 0);

      scene.Write("/Instancer", pi);
      scene.Write("/Instancer/Cube", cube);

      scene.Stage.Export("D:\\instancer-high-level.usda");

      var piSample = new PointInstancerSample();
      scene.Read("/Instancer", piSample);

      var matrices = piSample.ComputeInstanceMatrices(scene, "/Instancer");

      Console.WriteLine(String.Join(",", matrices.Select(p => p.ToString()).ToArray()));
      Console.WriteLine(String.Join(",", piSample.prototypes.targetPaths.Select(p => p.ToString()).ToArray()));
      Console.WriteLine(String.Join(",", piSample.protoIndices.Select(p => p.ToString()).ToArray()));
    }

  }
}
