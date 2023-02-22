namespace Unity.Formats.USD.Tests
{
    public static class TestAssetData
    {
        public struct Directory
        {
            public const string FolderName = "UsdImportTests";
        }
        public struct FileName
        {
            public const string TexturedOpaque = "TexturedOpaque";
            public const string TexturedTransparent_Cutout = "TexturedTransparent_Cutout";
        }

        public struct Extension
        {
            public const string Usda = ".usda";
        }

        public struct ImportGameObjectName
        {
            public const string Material = "Material";
            public const string RootPrim = "RootPrim";
        }
    }
}
