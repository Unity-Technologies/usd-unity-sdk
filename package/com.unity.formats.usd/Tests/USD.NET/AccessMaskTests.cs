using NUnit.Framework;
using pxr;
using USD.NET;
using USD.NET.Tests;

namespace USD.NET.Tests
{
    class AccessMaskTests : UsdTests
    {
        Scene scene;

        class TestState : IConversionState
        {
            public string something;
        }

        class MySample : SampleBase
        {
            public float myValue;
        }

        [SetUp]
        public void SetUp()
        {
            scene = Scene.Create();
            var sample = new MySample
            {
                myValue = 1.0f
            };
            var primPath = new SdfPath("/foo");
            scene.Time = 1;
            scene.Write(primPath, sample);
            scene.Time = 2;
            sample.myValue = 10.0f;
            scene.Write(primPath, sample);
            scene.Save();
        }

        [TearDown]
        public void TearDown()
        {
            scene.Close();
        }

        [Test]
        public void ReadSample_WriteState_StateIsPreserved()
        {
            var newsample = new MySample();
            var primPath = new SdfPath("/foo");

            var testState = new TestState() { something = "not null" };

            // Initialize the access mask and store the state
            scene.Time = 1;
            scene.AccessMask = new AccessMask();
            scene.IsPopulatingAccessMask = true;
            scene.Read(primPath, newsample);
            scene.AccessMask.Included[primPath].state = testState;

            // Read a different frame and check the state
            scene.Time = 2;
            scene.IsPopulatingAccessMask = false;
            scene.Read(primPath, newsample);
            var state = scene.AccessMask.Included[primPath].state;
            Assert.NotNull(state);
            Assert.AreSame(testState, state);
        }
    }
}
