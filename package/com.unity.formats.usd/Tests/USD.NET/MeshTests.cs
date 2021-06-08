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
using NUnit.Framework;

namespace USD.NET.Tests
{
    class MeshTests : UsdTests
    {
        [Test]
        public void TriangulationTest()
        {
            VtIntArray indices = new VtIntArray();
            VtIntArray faceCounts = new VtIntArray();

            faceCounts.push_back(5);
            indices.push_back(0);
            indices.push_back(1);
            indices.push_back(2);
            indices.push_back(3);
            indices.push_back(4);

            faceCounts.push_back(4);
            indices.push_back(5);
            indices.push_back(6);
            indices.push_back(7);
            indices.push_back(8);

            faceCounts.push_back(3);
            indices.push_back(9);
            indices.push_back(10);
            indices.push_back(11);

            // Degenerate face.
            faceCounts.push_back(2);
            indices.push_back(12);
            indices.push_back(13);

            UsdGeomMesh.Triangulate(indices, faceCounts);

            Assert.AreEqual(6, (int)faceCounts.size());

            for (int i = 0; i < faceCounts.size(); i++)
            {
                Assert.AreEqual(3, (int)faceCounts[i]);
            }

            Assert.AreEqual(18, (int)indices.size());

            Assert.AreEqual(0, indices[0]);
            Assert.AreEqual(1, indices[1]);
            Assert.AreEqual(2, indices[2]);

            Assert.AreEqual(0, indices[3]);
            Assert.AreEqual(2, indices[4]);
            Assert.AreEqual(3, indices[5]);

            Assert.AreEqual(0, indices[6]);
            Assert.AreEqual(3, indices[7]);
            Assert.AreEqual(4, indices[8]);

            Assert.AreEqual(5, indices[9]);
            Assert.AreEqual(6, indices[10]);
            Assert.AreEqual(7, indices[11]);

            Assert.AreEqual(5, indices[12]);
            Assert.AreEqual(7, indices[13]);
            Assert.AreEqual(8, indices[14]);

            Assert.AreEqual(9, indices[15]);
            Assert.AreEqual(10, indices[16]);
            Assert.AreEqual(11, indices[17]);
        }
    }
}
