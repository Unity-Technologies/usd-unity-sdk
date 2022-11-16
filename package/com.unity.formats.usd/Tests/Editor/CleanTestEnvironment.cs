using NUnit.Framework;

using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using System.IO;

public abstract class CleanTestEnvironment
{
    protected Scene m_UnityScene;
    protected string ArtifactsDirectory = "Assets/Artifacts/";

    [SetUp]
    public void CreateTestArtifactsAndStreamingAssetsDirectories()
    {
        m_UnityScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

        if (!Directory.Exists(ArtifactsDirectory))
        {
            Directory.CreateDirectory(ArtifactsDirectory);
            AssetDatabase.Refresh();
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(ArtifactsDirectory))
        {
            FileUtil.DeleteFileOrDirectory(ArtifactsDirectory);
            FileUtil.DeleteFileOrDirectory(ArtifactsDirectory.TrimEnd('/') + ".meta");
            AssetDatabase.Refresh();
        }
    }

    public string GetUnityScenePath(string sceneName)
    {
        return ArtifactsDirectory + sceneName + ".unity";
    }
}
