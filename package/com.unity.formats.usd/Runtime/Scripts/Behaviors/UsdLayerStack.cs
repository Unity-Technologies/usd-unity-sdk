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
using pxr;
using Scene = USD.NET.Scene;

namespace Unity.Formats.USD
{
    /// <summary>
    /// A mechanism for managing USD layers in a multi-layer shot context.
    /// </summary>
    [RequireComponent(typeof(UsdAsset))]
    public class UsdLayerStack : MonoBehaviour
    {
        public string m_targetLayer;
        public string[] m_layerStack;
        public string[] m_mutedLayers;

        /// <summary>
        /// Initialize a layer as a subLayer to be compatible with the parentLayer.
        /// </summary>
        private void SetupNewSubLayer(Scene parentScene, Scene subLayerScene)
        {
            if (parentScene == null)
            {
                throw new NullReferenceException("ParentScene is null");
            }

            subLayerScene.WriteMode = Scene.WriteModes.Over;
            subLayerScene.UpAxis = parentScene.UpAxis;
        }

        /// <summary>
        /// Ensure each layer path is expressed in the USD sub layer stack of the given scene,
        /// creating the sublayer USD files if needed.
        /// </summary>
        public void SaveLayerStack(Scene scene, string[] layerStack)
        {
            if (scene == null)
            {
                throw new NullReferenceException("Null scene provided to SaveLayerStack");
            }

            SdfSubLayerProxy subLayers = scene.Stage.GetRootLayer().GetSubLayerPaths();
            for (int i = 0; i < m_layerStack.Length; i++)
            {
                string absoluteLayerPath = m_layerStack[i];
                string relativeLayerPath = ImporterBase.MakeRelativePath(scene.FilePath, absoluteLayerPath);
                if (!System.IO.File.Exists(absoluteLayerPath))
                {
                    var newSubLayer = Scene.Create(absoluteLayerPath);
                    SetupNewSubLayer(scene, newSubLayer);
                    newSubLayer.Save();
                    newSubLayer.Close();
                }

                if (subLayers.Count(relativeLayerPath) == 0)
                {
                    subLayers.push_back(relativeLayerPath);
                }
            }

            scene.Save();
        }

        /// <summary>
        /// Writes overrides to the currently targeted subLayer.
        /// </summary>
        public void SaveToLayer()
        {
            var stageRoot = GetComponent<UsdAsset>();

            Scene subLayerScene = Scene.Create(m_targetLayer);
            if (subLayerScene == null)
            {
                throw new NullReferenceException("Could not create layer: " + m_targetLayer);
            }

            Scene rootScene = Scene.Open(stageRoot.usdFullPath);
            if (rootScene == null)
            {
                throw new NullReferenceException("Could not open base layer: " + stageRoot.usdFullPath);
            }

            SetupNewSubLayer(rootScene, subLayerScene);

            rootScene.Close();
            rootScene = null;

            try
            {
                SceneExporter.Export(stageRoot.gameObject,
                    subLayerScene,
                    stageRoot.m_changeHandedness,
                    exportUnvarying: false,
                    zeroRootTransform: false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
            finally
            {
                if (subLayerScene != null)
                {
                    subLayerScene.Save();
                    subLayerScene.Close();
                    subLayerScene = null;
                }
            }
        }
    }
}
