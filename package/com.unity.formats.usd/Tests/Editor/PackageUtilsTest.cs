using System.IO;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.Formats.USD.Tests
{
    class PackageUtilsTest
    {
        [Test, Description("Test getting a relative path from a base with no trailing slash")]
        [UnityPlatform(exclude = new[]
            {RuntimePlatform.WindowsEditor})] // URIs require a <drive>:\\ on Windows, see UriFormatException
        public void TestRelativeFolderPathNoTrailingSlash()
        {
            var res = PackageUtils.GetRelativeFolderPath("/path/to/folder", "/path/to/folder/subfolder/file.ext");
            Assert.That(res == "subfolder");
        }

        [Test, Description("Test getting a relative Windows path from a base with no trailing slash")]
        public void TestRelativeFolderPathNoTrailingSlashWindows()
        {
            var res = PackageUtils.GetRelativeFolderPath("C:\\path\\to\\folder",
                "C:\\path\\to\\folder\\subfolder\\file.ext");
            Assert.That(res == "subfolder");
        }

        [Test, Description("Test getting a relative path from a base with a trailing slash")]
        [UnityPlatform(exclude = new[]
            {RuntimePlatform.WindowsEditor})] // URIs require a <drive>:\\ on Windows, see UriFormatException
        public void TestRelativeFolderPathTrailingSlash()
        {
            var res = PackageUtils.GetRelativeFolderPath("/path/to/folder/", "/path/to/folder/subfolder/file.ext");
            Assert.That(res == "subfolder");
        }

        [Test, Description("Test getting a relative Windows path from a base with a trailing slash")]
        public void TestRelativeFolderPathTrailingSlashWindows()
        {
            var res = PackageUtils.GetRelativeFolderPath("C:\\path\\to\\folder\\",
                "C:\\path\\to\\folder\\subfolder\\file.ext");
            Assert.That(res == "subfolder");
        }

        [Test, Description("Test Unrelated paths")]
        [UnityPlatform(exclude = new[]
            {RuntimePlatform.WindowsEditor})] // URIs require a <drive>:\\ on Windows, see UriFormatException
        public void TestUnrelatedPaths()
        {
            var res = PackageUtils.GetRelativeFolderPath("/another/path/to/folder/",
                "/path/to/folder/subfolder/file.ext");
            Assert.That(res == "../../../../path/to/folder/subfolder");
        }

        [Test, Description("Test Unrelated paths on Windows")]
        public void TestUnrelatedPathsWindows()
        {
            const string folderPath = "D:\\path\\to\\folder\\subfolder";
            var res = PackageUtils.GetRelativeFolderPath("C:\\path\\to\\folder\\", folderPath + "\\file.ext");
            Assert.That(res == folderPath.Replace('\\', Path.DirectorySeparatorChar));
        }
    }
}
