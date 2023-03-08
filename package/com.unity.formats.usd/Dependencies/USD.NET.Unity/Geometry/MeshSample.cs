// Copyright 2021 Unity Technologies. All rights reserved.
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

namespace USD.NET.Unity
{
    // This class adds the additional data needed to make the mesh a proper USD mesh. This is split
    // out as a separate class because in the common read case, this data is not needed. Rather than
    // splitting the class, the values could also be read individually, though with a performance hit.

    [System.Serializable]
    [UsdSchema("Mesh")]
    public class MeshSample : MeshSampleBase
    {
        public Visibility visibility;

        [UsdVariability(Variability.Uniform)]
        public Purpose purpose;

        [UsdVariability(Variability.Uniform)]
        public bool doubleSided;

        [UsdVariability(Variability.Uniform)]
        public Orientation orientation;

        // Should be an array of "3", one for each triangle, unles arbitrary polygons are used.
        public int[] faceVertexCounts; // TODO: remove- they're always 3 so just create at export to save time & memory

        // ------------------------------------------------------------------------------------------ //
        // Helper Functions
        // ------------------------------------------------------------------------------------------ //

        /// <summary>
        /// Sets the faceVertexIndices and faceVertexCounts from triangle indices alone.
        /// </summary>
        public void SetTriangles(int[] triangleIndices)
        {
            faceVertexIndices = triangleIndices;
            faceVertexCounts = new int[faceVertexIndices.Length / 3];
            for (int i = 0; i < faceVertexCounts.Length; i++)
            {
                faceVertexCounts[i] = 3;
            }
        }
    }
}
