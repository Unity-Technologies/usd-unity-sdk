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

using System.Collections.Generic;
using UnityEngine;
using pxr;

namespace Unity.Formats.USD.Examples
{
    /// <summary>
    /// Combines all mesh children of matching Usd Paths into a single child mesh called "CombinedSubmesh."
    /// Deactivates the other child mesh GameObjects.
    /// Implements IImportPostProcessComponents, so it runs after Component instantiation
    /// </summary>
    public class ImportProcessorExample_CombineMeshes : RegexImportProcessor, IImportPostProcessComponents
    {
        const int kVertexLimit = 65534;
        const string kCombinedMeshName = "CombinedSubmesh";
        [Tooltip("If true, prevents vertex count from exceeding 65534 (old 16bit limit)")]
        public bool m_enforceU16VertexLimit;

        // Called by SceneImporter after GameObject hierarchy created and populated with geometry and other components
        public void PostProcessComponents(PrimMap primMap, SceneImportOptions sceneImportOptions)
        {
            Debug.Log("PostProcessComponents:");

            InitRegex();

            foreach (KeyValuePair<SdfPath, GameObject> kvp in primMap)
            {
                if (!IsMatch(kvp.Key)) continue;
                DoCombineMeshes(kvp.Value.transform);
            }

            Debug.Log($"Combined Mesh GameObject created as child of original USD asset <{SampleUtils.SetTextColor(SampleUtils.TextColor.Blue, this.gameObject.name)}> as <{SampleUtils.SetTextColor(SampleUtils.TextColor.Green, kCombinedMeshName)}>");
            Debug.Log("PostProcessComponents End");
        }

        void Reset()
        {
            matchExpression = "Geom";
            isNot = false;
            matchType = EMatchType.Regex;
            compareAgainst = ECompareAgainst.UsdName;
        }

        void DoCombineMeshes(Transform current)
        {
            MeshRenderer[] renderers = current.GetComponentsInChildren<MeshRenderer>();
            List<List<CombineInstance>> listcis = new List<List<CombineInstance>>();
            List<CombineInstance> cis = new List<CombineInstance>();
            listcis.Add(cis);

            List<Material> materials = new List<Material>();
            int vertexCount = 0;
            foreach (MeshRenderer renderer in renderers)
            {
                MeshFilter cmf = renderer.GetComponent<MeshFilter>();
                if (cmf == null) continue;

                int meshVertexCount = cmf.sharedMesh.vertexCount;
                if (m_enforceU16VertexLimit && meshVertexCount > kVertexLimit)
                {
                    Debug.LogError(
                        "Unable to use single mesh with greater than " + kVertexLimit + " vertices" + current);
                    continue;
                }

                vertexCount += meshVertexCount;

                if (m_enforceU16VertexLimit && vertexCount > kVertexLimit)
                {
                    vertexCount = 0;
                    cis = new List<CombineInstance>();
                    listcis.Add(cis);
                }

                CombineInstance ci = new CombineInstance();
                ci.mesh = cmf.sharedMesh;
                ci.transform = current.worldToLocalMatrix * cmf.transform.localToWorldMatrix;
                cis.Add(ci);

                // Assume order is maintained???
                materials.AddRange(renderer.sharedMaterials);
            }

            // Hide current meshes.
            current.gameObject.SetActive(true);
            foreach (Transform child in current)
            {
                if (child.name == "Materials")
                    continue;
                child.gameObject.SetActive(false);
            }

            // Build new meshes.
            bool mergeSubMeshes = true;

            foreach (List<CombineInstance> subcis in listcis)
            {
                // TODO: trim and rearrange materials so there is only one submesh per material?
                GameObject go = new GameObject();
                go.name = kCombinedMeshName;
                Transform sub = go.transform;
                sub.SetParent(current, false);

                MeshFilter mf = sub.GetComponent<MeshFilter>();
                if (mf == null) mf = sub.gameObject.AddComponent<MeshFilter>();

                MeshRenderer mr = sub.gameObject.GetComponent<MeshRenderer>();
                if (mr == null) mr = sub.gameObject.AddComponent<MeshRenderer>();

                var combinedMesh = new Mesh();
                combinedMesh.name = "CombinedMesh";

                mf.mesh = combinedMesh;
                if (vertexCount > kVertexLimit)
                {
                    mf.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }

                if (Application.isPlaying)
                {
                    // mf.mesh.Combine(subcis.ToArray());
                    mf.mesh.CombineMeshes(subcis.ToArray(), mergeSubMeshes: mergeSubMeshes);
                }
                else
                {
                    // mf.sharedMesh.Combine(subcis.ToArray());
                    mf.sharedMesh.CombineMeshes(subcis.ToArray(), mergeSubMeshes: mergeSubMeshes);
                }

                if (mergeSubMeshes)
                {
                    if (materials.Count == 0)
                    {
                        Debug.Log("No material to assign to combined mesh for " + current.name);
                    }
                    else
                    {
                        mr.sharedMaterial = materials[0];
                    }
                }
                else
                {
                    mr.sharedMaterials = materials.ToArray();
                }
            }
        }
    }
}
