# if UNITY_2023_2_OR_NEWER
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Unity.Formats.USD.Tests
{
    public static class AddPackage
    {
        static AddRequest Request;

        public static IEnumerator Add(string packageName)
        {
            if (Request == null)
            {
                Request = Client.Add(packageName);
                EditorApplication.update += Progress;
                yield return WaitForRequest();
            }
        }

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }
        }

        static IEnumerator WaitForRequest()
        {
            while (Request != null && !Request.IsCompleted)
            {
                yield return null;
            }
        }
    }
}

#endif
