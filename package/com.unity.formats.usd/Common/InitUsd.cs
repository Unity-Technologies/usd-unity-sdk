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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor;
using USD.NET;
using USD.NET.Unity;
using Debug = UnityEngine.Debug;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Unity.Formats.USD
{
    public static class InitUsd
    {
        private static bool m_usdInitialized;
        private static bool m_usdInitializeFailed = false;
        private static DiagnosticHandler m_handler;

        public static bool Initialize()
        {
            if (m_usdInitializeFailed)
            {
                return false;
            }

            if (m_usdInitialized)
            {
                return true;
            }

            m_usdInitialized = true;

            Stopwatch analyticsTimer = new Stopwatch();
            analyticsTimer.Start();

            try
            {
                // Initializes native USD plugins and ensures plugins are discoverable on the system path.
                SetupUsdPath();

                // The TypeBinder will generate code at runtime as a performance optimization, this must
                // be disabled when IL2CPP is enabled, since dynamic code generation is not possible.
#if ENABLE_IL2CPP
                TypeBinder.EnableCodeGeneration = false;
                Debug.Log("USD: Dynamic code generation disabled for IL2CPP.");
#endif

                // Type registration enables automatic conversion from Unity-native types to USD types (e.g.
                // Vector3[] -> VtVec3fArray).
                UnityTypeBindings.RegisterTypes();

                // The DiagnosticHandler propagates USD native errors, warnings and info up to C# exceptions
                // and Debug.Log[Warning] respectively.
                m_handler = new DiagnosticHandler();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                analyticsTimer.Stop();
                UsdEditorAnalytics.SendUsageEvent(false, analyticsTimer.Elapsed.TotalMilliseconds);
                m_usdInitializeFailed = true;
                return false;
            }

            analyticsTimer.Stop();
            UsdEditorAnalytics.SendUsageEvent(true, analyticsTimer.Elapsed.TotalMilliseconds);
            return true;
        }

        // USD has several auxillary C++ plugin discovery files which must be discoverable at run-time
        // We store those libs in Support/ThirdParty/Usd and then set a magic environment variable to let
        // USD's libPlug know where to look to find them.
        private static void SetupUsdPath([CallerFilePath] string sourceFilePath = "")
        {
#if UNITY_EDITOR
            // The current build of the plugin does not support ARM64 architectures. Give a useful warning depending on platform.
            bool isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

            if (isArm64)
            {
#if (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
                throw new System.NotSupportedException("The USD plugin built for this package does not currently support Apple Silicon. " +
                                                       "Please use an Intel-based editor with Rosetta emulation to use the USD plugin.");
#elif (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
                throw new System.NotSupportedException("The USD plugin built for this package does not currently support ARM-64 architectures. " +
                                                       "Please use an x64 based editor with x64 emulation to use the USD plugin.");
#endif
            }

            var fileInfo = new System.IO.FileInfo(sourceFilePath);
            var supPath = System.IO.Path.Combine(fileInfo.DirectoryName, "..", "Runtime", "Plugins");
#else
            var supPath = UnityEngine.Application.dataPath.Replace("\\", "/") + "/Plugins";
#endif

#if (UNITY_EDITOR_WIN)
            supPath += @"/x86_64/usd/";
#elif (UNITY_EDITOR_OSX)
            supPath += @"/x86_64/usd/";
#elif (UNITY_EDITOR_LINUX)
            supPath += @"/x86_64/usd/";
#elif (UNITY_STANDALONE_WIN)
            supPath += @"/usd/";
#elif (UNITY_STANDALONE_OSX)
            supPath += @"/usd/";
#elif (UNITY_STANDALONE_LINUX)
            supPath += @"/usd/";
#endif

            Debug.LogFormat("Registering plugins: {0}", supPath);
            pxr.PlugRegistry.GetInstance().RegisterPlugins(supPath);
        }
    }
}
