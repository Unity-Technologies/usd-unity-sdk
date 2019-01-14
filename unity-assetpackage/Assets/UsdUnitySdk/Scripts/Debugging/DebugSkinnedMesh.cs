using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class DebugSkinnedMesh : MonoBehaviour {

  [Tooltip("A path to a USD file for comparing weights")]
  public string m_usdFile;

  [Tooltip("A prim path to the skinned mesh in the USD file")]
  public string m_usdMeshPath;

  [Tooltip("How to convert transforms for comparison")]
  public USD.NET.Unity.BasisTransformation m_basisTransform;

  void OnEnable() {
    USD.NET.Examples.InitUsd.Initialize();

    if (string.IsNullOrEmpty(m_usdMeshPath)) {
      m_usdMeshPath = USD.NET.Unity.UnityTypeConverter.GetPath(transform);
    }

    var scene = GetScene();
    var binding = ReadUsdWeights(scene);
    string skelRootPath;
    var skeleton = ReadUsdSkeleton(scene, out skelRootPath);

    if (binding == null) {
      binding = new USD.NET.Unity.SkelBindingSample();
    }

    var mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

    // Process classic four-bone weights first.
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Legacy 4-bone indices: (" + mesh.boneWeights.Length + " * 4)");
    int bone = 0;
    foreach (var weight in mesh.boneWeights) {
      sb.Append("[");
      sb.Append(weight.boneIndex0 + GetUsdBoneData(bone, 0, binding.jointIndices) + ",");
      sb.Append(weight.boneIndex1 + GetUsdBoneData(bone, 1, binding.jointIndices) + ",");
      sb.Append(weight.boneIndex2 + GetUsdBoneData(bone, 2, binding.jointIndices) + ",");
      sb.Append(weight.boneIndex3 + GetUsdBoneData(bone, 3, binding.jointIndices) + "]\n");
      bone++;
    }
    Debug.Log(sb.ToString());

    bone = 0;
    sb = new System.Text.StringBuilder();
    sb.AppendLine("Legacy 4-bone weights: (" + mesh.boneWeights.Length + " * 4)");
    foreach (var weight in mesh.boneWeights) {
      sb.Append("[");
      sb.Append(weight.weight0 + GetUsdBoneData(bone, 0, binding.jointWeights) + ",");
      sb.Append(weight.weight1 + GetUsdBoneData(bone, 1, binding.jointWeights) + ",");
      sb.Append(weight.weight2 + GetUsdBoneData(bone, 2, binding.jointWeights) + ",");
      sb.Append(weight.weight3 + GetUsdBoneData(bone, 3, binding.jointWeights) + "]\n");
      bone++;
    }
    Debug.Log(sb.ToString());

    sb = new System.Text.StringBuilder();
    var bones = GetComponent<SkinnedMeshRenderer>().bones;
    var rootBone = GetComponent<SkinnedMeshRenderer>().rootBone;
    var root = USD.NET.Unity.UnityTypeConverter.GetPath(rootBone);
    sb.AppendLine("Bones: (" + bones.Length + ")");
    sb.AppendLine("Root Bone: " + root);
    int i = 0;
    foreach (var boneXf in bones) {
      sb.AppendLine(USD.NET.Unity.UnityTypeConverter.GetPath(boneXf));
      if (binding.joints != null) {
        sb.AppendLine(root + "\\" + binding.joints[i++] + "\n");
      }
    }
    Debug.Log(sb.ToString());

    sb = new System.Text.StringBuilder();
    sb.AppendLine("Bind Transforms: (" + mesh.bindposes.Length + ")");
    i = -1;
    var options = new USD.NET.Unity.SceneImportOptions();
    options.changeHandedness = m_basisTransform;
    foreach (var boneXf in bones) {
      i++;
      var bindPose = mesh.bindposes[i];
      var bonePath = USD.NET.Unity.UnityTypeConverter.GetPath(boneXf);
      sb.AppendLine("Pose[" + i + "] " + bonePath);
      sb.AppendLine(bindPose.ToString());

      if (skeleton.bindTransforms != null) {
        if (string.IsNullOrEmpty(skelRootPath)) {
          continue;
        }

        bonePath = bonePath.Substring(skelRootPath.Length);
        bonePath = bonePath.TrimStart('/');
        foreach (var joint in skeleton.joints) {
          if (joint == bonePath) {
            var usdMat = skeleton.bindTransforms[i];
            USD.NET.Unity.XformImporter.ImportXform(ref usdMat, options);
            sb.AppendLine(usdMat.ToString() + "\n");
            bonePath = null;
            break;
          }
        }

        if (string.IsNullOrEmpty(bonePath)) { continue; }
        sb.Append("Bone not found in USD: " + bonePath + "\n\n");
      }
    }
    Debug.Log(sb.ToString());

  }

  string GetUsdBoneData<T>(int bone, int weightIndex, USD.NET.Primvar<T[]> primvar) {
    if (primvar == null || primvar.value == null) {
      return string.Empty;
    }

    if (primvar.interpolation == USD.NET.PrimvarInterpolation.Constant) {
      if (weightIndex > primvar.elementSize - 1) {
        return "";
      }
      if (weightIndex > primvar.value.Length - 1) {
        Debug.LogWarning("USD file specifies " + primvar.elementSize
            + " bone elements but array only contains "
            + primvar.value.Length);
        return "";
      }
      return "(" + primvar.value[weightIndex].ToString() + ")  ";
    }

    int usdIndex = bone * primvar.elementSize + weightIndex;
    if (usdIndex > primvar.value.Length - 1) {
      Debug.LogWarning("bone(" + bone + ") weight index(" + weightIndex + ") "
        + "is out of bounds for the USD bone indices which has "
        + "(" + primvar.elementSize + ") values per joint and "
        + "(" + primvar.value.Length + ") total values");
      return "";
    }

    return "(" + primvar.value[usdIndex].ToString() + ")  ";
  }

  USD.NET.Scene GetScene() {
    if (string.IsNullOrEmpty(m_usdFile) && string.IsNullOrEmpty(m_usdMeshPath)) {
      return null;
    }

    if (string.IsNullOrEmpty(m_usdFile) || string.IsNullOrEmpty(m_usdMeshPath)) {
      Debug.LogError("You must spcify both usdFile and usdMeshPath together");
      return null;
    }

    try {
      var scene = USD.NET.Scene.Open(m_usdFile);
      if (scene == null) {
        Debug.LogWarning("Failed to open file: " + m_usdFile);
        return null;
      }

      return scene;
    } catch (System.Exception ex) {
      Debug.LogException(ex);
      return null;
    }
  }

  USD.NET.Unity.SkelBindingSample ReadUsdWeights(USD.NET.Scene scene) {
    if (scene == null) { return null; }
    try {
      var sample = new USD.NET.Unity.SkelBindingSample();
      scene.Read(m_usdMeshPath, sample);
      return sample;
    } catch (System.Exception ex) {
      Debug.LogException(ex);
      return null;
    }
  }

  USD.NET.Unity.SkeletonSample ReadUsdSkeleton(USD.NET.Scene scene, out string skelRootPath) {
    skelRootPath = null;
    if (scene == null) { return null; }

    try {
      var path = new pxr.SdfPath(m_usdMeshPath);
      var absRoot = pxr.SdfPath.AbsoluteRootPath();
      var skelRelName = new pxr.TfToken("skel:skeleton");

      while (path != absRoot) {
        var prim = scene.GetPrimAtPath(path);
        path = path.GetParentPath();

        if (prim.HasRelationship(skelRelName)) {
          var targets = prim.GetRelationship(skelRelName).GetForwardedTargets();
          if (targets.Count == 0) {
            Debug.LogWarning("skel:skeleton has no targets at path: " + prim.GetPath());
            continue;
          }

          var skelTarget = scene.GetPrimAtPath(targets[0]);
          if (skelTarget == null) {
            Debug.LogWarning("prim <" + prim.GetPath() + "> has skel:skeleton with missing target path: " + targets[0]);
            continue;
          }

          skelRootPath = skelTarget.GetPath().ToString();
          var sample = new USD.NET.Unity.SkeletonSample();
          scene.Read(skelTarget.GetPath(), sample);
          return sample;
        }
      }

      Debug.LogWarning("Skeleton not found for path: " + m_usdMeshPath);
      return null;
    } catch (System.Exception ex) {
      Debug.LogException(ex);
      return null;
    }
  }

  private void Update() {
  }

}
