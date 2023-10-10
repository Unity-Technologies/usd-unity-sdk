// Copyright 2023 Unity Technologies. All rights reserved.
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

using NUnit.Framework;
using System.IO;
using Unity.Formats.USD;
using Unity.Formats.USD.Tests;
using UnityEngine;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    class ComplianceTests : BaseFixtureEditor
    {
        private Scene m_USDScene;
        private string m_USDScenePath;

        [SetUp]
        public void SetUp()
        {
            m_USDScenePath = TestUtility.GetUSDScenePath(ArtifactsDirectoryFullPath, "USDComplianceTests");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_USDScene != null)
            {
                m_USDScene.Close();
            }
        }

        [Test]
        public void ExportScene_DefaultMetersPerUnit()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var outputscene = ExportHelpers.InitForSave(m_USDScenePath);

            ExportHelpers.ExportGameObjects(new[] { cube }, outputscene, BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);

            var outmpu = m_USDScene.MetersPerUnit;
            Assert.AreEqual(1.0, outmpu, "Unexpected inconsistency in default MetersPerUnit metadata");
        }

        [Test]
        public void ExportScene_MetersPerUnit()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var outputscene = ExportHelpers.InitForSave(m_USDScenePath);

            var inmpu = 0.01;
            outputscene.MetersPerUnit = inmpu;
            ExportHelpers.ExportGameObjects(new[] { cube }, outputscene, BasisTransformation.SlowAndSafe);

            m_USDScene = Scene.Open(m_USDScenePath);

            var outmpu = m_USDScene.MetersPerUnit;
            Assert.AreEqual(inmpu, outmpu, "Unexpected inconsistency in MetersPerUnit metadata");
        }

        [Test]
        public void ExportScene_USDZ_MetersPerUnit()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var usdzPath = Path.ChangeExtension(m_USDScenePath, ".usdz");
            UsdzExporter.ExportUsdz(usdzPath, cube);

            m_USDScene = Scene.Open(usdzPath);
            var outmpu = m_USDScene.MetersPerUnit;
            Assert.AreEqual(0.01, outmpu, "Unexpected inconsistency in USDZ MetersPerUnit metadata");
        }
    }
}
