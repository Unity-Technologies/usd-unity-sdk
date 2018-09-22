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
using UnityEngine;
using USD.NET;
using USD.NET.Unity;

public class UsdAssetImporter {

  public static GameObject ImportUsd(string usdFilePath, string targetAssetPath) {
    USD.NET.Examples.InitUsd.Initialize();

    var scene = Scene.Open(usdFilePath);

    // Time-varying data is not supported and often scenes are written without "Default" time
    // values, which makes setting an arbitrary time safer (because if only default was authored
    // the time will be ignored and values will resolve to default time automatically).
    scene.Time = 1.0;
    
    if (scene == null) {
      throw new Exception("Failed to open: " + usdFilePath);
    }

    try {
      return ImportUsdScene(scene, usdFilePath);
    } finally {
      scene.Close();
    }
  }

  private static GameObject ImportUsdScene(Scene scene, string usdFilePath) {

    string fileName = UnityTypeConverter.MakeValidIdentifier(
        System.IO.Path.GetFileNameWithoutExtension(usdFilePath));

    var goRoot = new GameObject(fileName);
    var solidColorMat = new Material(Shader.Find("Standard"));
    solidColorMat.SetFloat("_Glossiness", 0.2f);

    var importOptions = new SceneImportOptions();
    importOptions.changeHandedness = BasisTransformation.SlowAndSafe;
    importOptions.materialMap.FallbackMasterMaterial = solidColorMat;

    // The root object at which the USD scene will be reconstructed.
    // It may need a Z-up to Y-up conversion and a right- to left-handed change of basis.
    SceneImporter.BuildScene(scene, goRoot, importOptions);

    return goRoot;
  }

}
