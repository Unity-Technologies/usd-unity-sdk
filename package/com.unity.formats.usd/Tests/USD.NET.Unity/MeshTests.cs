using NUnit.Framework;
using pxr;
using USD.NET.Tests;

namespace USD.NET.Unity.Tests
{
    class MeshTests : UsdTests
    {
        [Test]
        public static void VisibilityTest()
        {
            var sample = new MeshSample();
            var outSample = new MeshSample();
            sample.visibility = Visibility.Invisible;

            WriteAndRead(ref sample, ref outSample);

            Assert.AreEqual(sample.visibility, outSample.visibility);
        }
    }
}
