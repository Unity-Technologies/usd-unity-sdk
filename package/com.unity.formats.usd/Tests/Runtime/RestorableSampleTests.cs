using NUnit.Framework;
using pxr;
using USD.NET;

namespace Unity.Formats.USD.Tests
{
    public class RestorableSampleTests : BaseFixture
    {
        Scene scene;

        class TestRestorableData : IRestorableData
        {
            public float staticValue;
            public string extraData;
        }

        class RestorableSample : SampleBase, IRestorable
        {
            bool isRestored;
            public float dynamicValue; // property that will be animated and always deserialized
            public float staticValue; // property that will be static and only deserialized the first frame
            string extraData; // private data that won't be (de)serialized from/to USD and will persist only through the data

            public string ExtraData
            {
                get => extraData;
                set => extraData = value;
            }

            public bool IsRestoredFromCachedData()
            {
                return isRestored;
            }

            public void FromCachedData(IRestorableData data)
            {
                var testData = data as TestRestorableData;
                staticValue = testData.staticValue;
                extraData = testData.extraData;
            }

            public IRestorableData ToCachedData()
            {
                return new TestRestorableData()
                {
                    staticValue = staticValue,
                    extraData = extraData
                };
            }
        }

        [SetUp]
        public void SetUp()
        {
            scene = Scene.Create();
            var sample = new RestorableSample
            {
                dynamicValue = 1.0f,
                staticValue = 100.0f
            };
            var primPath = new SdfPath("/foo");
            scene.Time = 1;
            scene.Write(primPath, sample);
            scene.Time = 2;
            sample.dynamicValue = 10.0f;
            scene.Write(primPath, sample);
            scene.Save();
        }

        [TearDown]
        public void TearDown()
        {
            scene.Close();
        }

        [Test]
        public void ReadSample_DataIsPreserved()
        {
            var newsample = new RestorableSample();
            var primPath = new SdfPath("/foo");

            // Initialize the access mask and store the data
            scene.Time = 1;
            scene.AccessMask = new AccessMask();
            scene.IsPopulatingAccessMask = true;
            scene.Read(primPath, newsample);
            Assert.AreEqual(newsample.dynamicValue, 1.0f);
            Assert.AreEqual(newsample.staticValue, 100.0f);
            Assert.IsNull(newsample.ExtraData);
            // Fake some sample extra processing
            newsample.ExtraData = "this is not USD data";
            // Save the sample data to the access mask
            scene.AccessMask.Included[primPath].cachedData = newsample.ToCachedData();

            // Now read the second frame
            var anothersample = new RestorableSample();
            scene.Time = 2;
            scene.IsPopulatingAccessMask = false;
            scene.Read(primPath, anothersample);
            // dynamic value has changed
            Assert.AreEqual(anothersample.dynamicValue, 10.0f);
            // static value and extra data are default value
            Assert.AreEqual(anothersample.staticValue, 0.0f);
            Assert.IsNull(anothersample.ExtraData);
            // restore from data
            anothersample.FromCachedData(scene.AccessMask.Included[primPath].cachedData);
            // static value and extra have been restored
            Assert.AreEqual(anothersample.staticValue, 100.0f);
            Assert.AreEqual(anothersample.ExtraData, "this is not USD data");
        }
    }
}
