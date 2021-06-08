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

namespace Unity.Formats.USD
{
    /// <summary>
    /// Compares two skinned meshes in Unity.
    /// </summary>
    [ExecuteInEditMode]
    public class SkinnedMeshUnityDiff : MonoBehaviour
    {
        [Tooltip("The USD skinned mesh to compare")]
        public SkinnedMeshRenderer m_usdMesh;

        [Tooltip("The Unity skinned mesh to compare")]
        public SkinnedMeshRenderer m_unityMesh;

        void OnEnable()
        {
            var usdSmr = m_usdMesh;
            var unitySmr = m_unityMesh;

            if (!usdSmr || !unitySmr)
            {
                return;
            }

            var usdMesh = usdSmr.sharedMesh;
            var unityMesh = unitySmr.sharedMesh;

            Debug.Log("Processing legacy 4-bone rig");

            if (usdMesh.boneWeights.Length != unityMesh.boneWeights.Length)
            {
                Debug.LogWarning("Bone index/weight counts do not match: USD mesh("
                    + usdMesh.boneWeights.Length + ") != Unity mesh("
                    + unityMesh.boneWeights.Length + ")");
            }
            else
            {
                for (int i = 0; i < usdMesh.boneWeights.Length; i++)
                {
                    if (!WeightsMatch(usdMesh.boneWeights[i], unityMesh.boneWeights[i]))
                    {
                        Debug.LogWarning("Bone weights do not match at index(" + i + "):\n"
                            + "USD mesh weights:   "
                            + usdMesh.boneWeights[i].weight0 + ", "
                            + usdMesh.boneWeights[i].weight1 + ", "
                            + usdMesh.boneWeights[i].weight2 + ", "
                            + usdMesh.boneWeights[i].weight3 + "\n"
                            + "Unity mesh weights: "
                            + unityMesh.boneWeights[i].weight0 + ", "
                            + unityMesh.boneWeights[i].weight1 + ", "
                            + unityMesh.boneWeights[i].weight2 + ", "
                            + unityMesh.boneWeights[i].weight3 + "\n");
                    }

                    if (!IndicesMatch(usdMesh.boneWeights[i], unityMesh.boneWeights[i]))
                    {
                        Debug.LogWarning("Bone indices do not match at index(" + i + "):\n"
                            + "USD mesh indices:   "
                            + usdMesh.boneWeights[i].boneIndex0 + ", "
                            + usdMesh.boneWeights[i].boneIndex1 + ", "
                            + usdMesh.boneWeights[i].boneIndex2 + ", "
                            + usdMesh.boneWeights[i].boneIndex3 + "\n"
                            + "Unity mesh indices: "
                            + unityMesh.boneWeights[i].boneIndex0 + ", "
                            + unityMesh.boneWeights[i].boneIndex1 + ", "
                            + unityMesh.boneWeights[i].boneIndex2 + ", "
                            + unityMesh.boneWeights[i].boneIndex3 + "\n");
                    }
                }
            }

            if (usdMesh.bindposes.Length != unityMesh.bindposes.Length)
            {
                Debug.LogWarning("Mesh bind pose counts do not match, USD mesh: "
                    + usdMesh.bindposes.Length + " Unity mesh: "
                    + unityMesh.bindposes.Length);
            }
            else
            {
                for (int i = 0; i < usdMesh.bindposes.Length; i++)
                {
                    if (!Approximately(usdMesh.bindposes[i], unityMesh.bindposes[i]))
                    {
                        Debug.LogWarning("Mesh bind pose does not match at index(" + i + "):\n"
                            + "USD Pose:\n" + usdMesh.bindposes[i].ToString() + " "
                            + "Unity Pose:\n" + unityMesh.bindposes[i].ToString());
                    }
                }
            }

            if (usdSmr.bones.Length != unitySmr.bones.Length)
            {
                Debug.LogWarning("Mesh bone counts do not match: "
                    + "USD mesh:   " + usdSmr.bones.Length + " "
                    + "Unity mesh: " + unitySmr.bones.Length);
            }
            else
            {
                for (int i = 0; i < usdSmr.bones.Length; i++)
                {
                    if (pxr.UsdCs.TfMakeValidIdentifier(usdSmr.bones[i].name) !=
                        pxr.UsdCs.TfMakeValidIdentifier(unitySmr.bones[i].name))
                    {
                        Debug.LogWarning("Mesh bind pose does not match at index(" + i + "): "
                            + "USD bone: " + usdSmr.bones[i].ToString() + " "
                            + "Unity bone: " + unitySmr.bones[i].ToString());
                    }
                }
            }
        }

        bool Approximately(Matrix4x4 rhs, Matrix4x4 lhs)
        {
            for (int i = 0; i < 16; i++)
            {
                if (!Mathf.Approximately(rhs[i], lhs[i]))
                {
                    return false;
                }
            }

            return true;
        }

        bool WeightsMatch(BoneWeight w1, BoneWeight w2)
        {
            return Mathf.Approximately(w1.weight0, w2.weight0)
                && Mathf.Approximately(w1.weight1, w2.weight1)
                && Mathf.Approximately(w1.weight2, w2.weight2)
                && Mathf.Approximately(w1.weight3, w2.weight3);
        }

        bool IndicesMatch(BoneWeight w1, BoneWeight w2)
        {
            return w1.boneIndex0 == w2.boneIndex0
                && w1.boneIndex1 == w2.boneIndex1
                && w1.boneIndex2 == w2.boneIndex2
                && w1.boneIndex3 == w2.boneIndex3;
        }

        private void Update()
        {
        }
    }
}
