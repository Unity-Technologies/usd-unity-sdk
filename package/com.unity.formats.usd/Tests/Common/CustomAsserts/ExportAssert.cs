using UnityEngine;
using NUnit.Framework;

namespace Unity.Formats.USD.Tests
{
    public static class ExportAssert
    {
        public static class Runtime
        {
            public static void IsDataStructureKeptInUsdz(GameObject importedObject, string expectedObjectName, string expectedRootPrimName, string expectedMaterialsName)
            {
                Assert.AreEqual(importedObject.name, expectedObjectName);

                var rootPrim = importedObject.transform.Find(expectedRootPrimName);
                var materials = importedObject.transform.Find(expectedMaterialsName);

                Assert.IsNotNull(rootPrim, string.Format("Root Prim was not found under expected name '{0}'", expectedRootPrimName));
                Assert.IsNotNull(materials, string.Format("Materials was not found under expected name '{0}'", expectedMaterialsName));

                Assert.IsNotNull(rootPrim.GetComponent<UsdPrimSource>(), "Root Prim GameObject did not contain UsdPrimSource component");
                Assert.IsNotNull(materials.GetComponent<UsdPrimSource>(), "Materials GameObject did not contain UsdPrimSource component");

                var materialsData = materials.transform.Find(expectedObjectName);
                var rootPrimData = rootPrim.transform.Find(expectedObjectName);

                Assert.IsNotNull(rootPrimData, string.Format("Root Prim Data was not found under expected name '{0}'", expectedObjectName));
                Assert.IsNotNull(materialsData, string.Format("Materials Data was not found under expected name '{0}'", expectedObjectName));

                Assert.IsNotNull(rootPrimData.GetComponent<UsdPrimSource>(), "Root Prim Data GameObject did not contain UsdPrimSource component");
                Assert.IsNotNull(materialsData.GetComponent<UsdPrimSource>(), "Materials Data GameObject did not contain UsdPrimSource component");
            }
        }
    }
}
