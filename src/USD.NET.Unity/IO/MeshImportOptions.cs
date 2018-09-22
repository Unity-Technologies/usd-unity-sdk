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

using UnityEngine;

namespace USD.NET.Unity {

  /// <summary>
  /// Indicates how an attribute should be imported.
  /// </summary>
  public enum ImportMode {

    /// <summary>
    /// The value is imported if authored, otherwise null/default.
    /// </summary>
    Import,

    /// <summary>
    /// The value is imported if authored, else the value is computed (if possible).
    /// </summary>
    ImportOrCompute,

    /// <summary>
    /// The value is only computed (if possible), authored values are ignored.
    /// </summary>
    Compute,

    /// <summary>
    /// The authored value is ignored and no value is computed.
    /// </summary>
    Ignore,
  }

  /// <summary>
  /// Indicates how values are imported from the given scene into a UnityEngine.Mesh object.
  /// </summary>
  public class MeshImportOptions {

    /// <summary>
    /// If true, triangulates the mesh. Should only be set to false if the mesh is guaranteed
    /// to be a valid triangle mesh before import.
    /// </summary>
    public bool triangulateMesh = true;

    public ImportMode color = ImportMode.Import;
    public ImportMode normals = ImportMode.ImportOrCompute;
    public ImportMode tangents = ImportMode.ImportOrCompute;
    public ImportMode boundingBox = ImportMode.ImportOrCompute;
    public ImportMode texcoord0 = ImportMode.Import;
    public ImportMode texcoord1 = ImportMode.Import;
    public ImportMode texcoord2 = ImportMode.Import;
    public ImportMode texcoord3 = ImportMode.Import;
  }

}
