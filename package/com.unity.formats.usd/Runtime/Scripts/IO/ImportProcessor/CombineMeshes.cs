using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using pxr;

//possibly move this to Examples
// also not sure how well this will work with reloads or anim..
namespace Unity.Formats.USD {
    public class CombineMeshes : RegexImportProcessor, IImportPostProcessGeometry {
        const int VERTEX_LIMIT = 65534;
        public bool enforceU16VertexLimit;

        public void PostProcessGeometry(PrimMap primMap)
        {
            InitRegex();

            foreach (KeyValuePair<SdfPath, GameObject> kvp in primMap)
            {
                if (!IsMatch(kvp.Key)) continue;
                DoCombineMeshes(kvp.Value.transform);
            }
        }

        
        void Reset()
        {
            matchExpression = "Geom";
            isNot = false;
            matchType = EMatchType.Wildcard;
            compareAgainst = ECompareAgainst.UsdName;
        }

        bool DoCombineMeshes(Transform current)
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
                if (enforceU16VertexLimit && meshVertexCount > VERTEX_LIMIT)
                {
                    Debug.LogError("Unable to use single mesh with greater than " + VERTEX_LIMIT + " vertices" + current);
                    continue;
                }

                vertexCount += meshVertexCount;

                if (enforceU16VertexLimit && vertexCount > VERTEX_LIMIT)
                {
                    vertexCount = 0;
                    cis = new List<CombineInstance>();
                    listcis.Add(cis);
                }

                CombineInstance ci = new CombineInstance();
                ci.mesh = cmf.sharedMesh;
                ci.transform = current.worldToLocalMatrix * cmf.transform.localToWorldMatrix;
                cis.Add(ci);
                //assume order is maintained???
                materials.AddRange(renderer.sharedMaterials);
            }

            // hide current meshes
            current.gameObject.SetActive(true);
            foreach (Transform child in current)
            {
                child.gameObject.SetActive(false);
            }

            //build new meshes
            bool mergeSubMeshes = true; //options.materialAssign == Options.EMaterialAssign.DefaultOnly;

            foreach (List<CombineInstance> subcis in listcis)
            {
                //TODO trim and rearrange materials so there is only one submesh per material?
                GameObject go = new GameObject();
                go.name = "CombinedSubmesh";
                Transform sub = go.transform;
                sub.SetParent(current, false);
                MeshFilter mf = sub.GetComponent<MeshFilter>();
                if (mf == null) mf = sub.gameObject.AddComponent<MeshFilter>();
                MeshRenderer mr = sub.gameObject.GetComponent<MeshRenderer>();
                if (mr == null) mr = sub.gameObject.AddComponent<MeshRenderer>();

                mf.mesh = new Mesh();
                if (vertexCount > VERTEX_LIMIT)
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
                        Debug.LogError("No material to assign to combined mesh for " + current.name);
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

            return false;

        }
    }
}