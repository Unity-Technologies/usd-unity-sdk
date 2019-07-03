// using System.IO;
// using UnityEngine;
// using UnityEditor.Experimental.AssetImporters;
// using UnityEngine.Formats.USD.Importer;
// using pxr;


// namespace UnityEditor.Formats.USD.Importer
// {
//     [ScriptedImporter(1, new[] {"usd", "usda", "usdc", "usdz"})]
//     internal class USDImporter : ScriptedImporter
//     {
//         [SerializeField]
//         private USDSettings settings = new USDSettings();
//         public USDSettings Settings
//         {
//             get { return settings; }
//             set { settings = value; }
//         }

//         public override void OnImportAsset(AssetImportContext ctx)
//         {
//             if (ctx == null)
//             {
//                 return;
//             }

//             string assetPath = ctx.assetPath;
//             string fileName = Path.GetFileNameWithoutExtension(assetPath);
//             GameObject gameObject = new GameObject(fileName);

//             Debug.Log("File path: " + assetPath + ", " + File.Exists(assetPath));

//             UsdStage stage = UsdStage.Open(assetPath);
//             if (stage == null)
//             {
//                 Debug.LogError("Can't open a USD stage from the given file: " + assetPath);
//             }
            

//             // foreach (UsdPrim prim in stage.Traverse())
//             // {
//             //     Debug.Log("Prim: " + prim.GetPath());
//             // }

//             ctx.AddObjectToAsset(fileName, gameObject);
//             ctx.SetMainObject(gameObject);
//         }

//     }
// }
