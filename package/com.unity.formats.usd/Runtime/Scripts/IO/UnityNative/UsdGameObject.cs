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

namespace USD.NET.Unity {

  /// <summary>
  /// Represents a Unity GameObject, in USD.
  /// WARNING: this sample type is highly experimental and subject to change.
  /// </summary>
  [Serializable]
  public class UsdGameObject : SampleBase {
    // Stores the true Unity name, which may not match USD, due to aliasing and invalid characters.
    public string name;
    public bool activeSelf;
    public int layer;
    public HideFlags hideFlags;
    public bool isStatic;
    public string tag;
    public Vector3 localPosition;
    public Vector3 localScale;
    public Quaternion localRotation;
  }

  // Serialization class to add 'unity:gameObject' namespace in USD.
  public class UsdGameObjectSample : SampleBase {
    [UsdNamespace("unity:gameObject")]
    public UsdGameObject gameObject = new UsdGameObject();
  }
}
