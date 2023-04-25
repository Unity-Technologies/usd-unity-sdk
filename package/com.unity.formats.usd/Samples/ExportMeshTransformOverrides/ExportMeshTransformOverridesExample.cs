using pxr;
using System.IO;
using Unity.Formats.USD;
using UnityEditor;
using UnityEngine;
using USD.NET;

public class ExportMeshTransformOverridesExample : MonoBehaviour
{
    private const string k_exampleUsdFileGUID = "82266c9d3cfb61d49906dfe90e9c060a";

    public static GameObject m_exampleImportedUsdObject;

    public void ImportInitialUsdFile()
    {
        var usdPath = Path.GetFullPath(AssetDatabase.GUIDToAssetPath(k_exampleUsdFileGUID));
        var stage = UsdStage.Open(usdPath, UsdStage.InitialLoadSet.LoadNone);
        var scene = Scene.Open(stage);
        m_exampleImportedUsdObject = ImportHelpers.ImportSceneAsGameObject(scene, null, new SceneImportOptions() { materialImportMode = MaterialImportMode.ImportPreviewSurface });
        scene.Close();
    }

    public void ChangeExampleTransformData()
    {
        var cube = m_exampleImportedUsdObject.transform.GetChild(0).GetChild(2); // The Middle cube
        cube.transform.position = Random.insideUnitSphere / 2 * Random.Range(-1, 2);
        cube.localScale = Random.insideUnitSphere;
        cube.rotation = Random.rotation;

        SampleUtils.FocusConsoleWindow();
        Debug.Log($"For <{cube.name}> the following have changed:");
        Debug.Log($"The new position is now: {cube.transform.position.ToString("F4")}");
        Debug.Log($"The new scale is now: {cube.transform.localScale.ToString("F4")}");
        Debug.Log($"The new rotation is now: {cube.transform.rotation.eulerAngles.ToString("F4")}");
    }

    // Utilizes the same method when doing Export Overrides through:
    // Menu Bar -> USD -> Exported Transform Overrides
    // Will save the Transform Override file in the project 'Assets' folder
    public void ExportTransformOverride()
    {
        var newFileName = m_exampleImportedUsdObject.name + "_overs.usda";
        var overs = ExportHelpers.InitForSave(Path.Combine(Application.dataPath, newFileName));
        m_exampleImportedUsdObject.GetComponent<UsdAsset>().ExportOverrides(overs);

        SampleUtils.FocusConsoleWindow();
        Debug.Log($"<color=#00FF00>Transform Overrides have been saved in the Project 'Assets' folder as <<b>{newFileName}</b>></color>");
    }
}
