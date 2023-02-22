// Copyright 2022 Unity Technologies. All rights reserved.
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
using System;
using UnityEngine.SceneManagement;

namespace Unity.Formats.USD.Tests
{
    public class BaseFixtureRuntime : BaseFixture
    {
        [SetUp]
        public void EnsureCleanScene()
        {
            // Unload all scenes except the one created by the test framework
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
            {
                if (!SceneManager.GetSceneAt(sceneIndex).name.StartsWith("InitTestScene"))
                {
#pragma warning disable 0618
                    SceneManager.UnloadScene(SceneManager.GetSceneAt(sceneIndex));
#pragma warning restore 0618
                }
            }

            // Ensure the test is running on a clean scene
            var uniqueSceneName = Guid.NewGuid().ToString();
            m_UnityScene = SceneManager.CreateScene(uniqueSceneName);
            SceneManager.SetActiveScene(m_UnityScene);
        }
    }
}
