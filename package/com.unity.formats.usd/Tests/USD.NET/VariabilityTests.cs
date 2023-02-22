using NUnit.Framework;
namespace USD.NET.Tests
{
    class VariabilityTests : UsdTests
    {
        static protected void TestDefaultTime_NotVarying<T>(T inputSample)
            where T : USD.NET.SampleBase, new()
        {
            // ----------------------------------------------- //
            // Test with default time --> not varying.
            // ----------------------------------------------- //
            string filename = GetTempFile();
            var scene = USD.NET.Scene.Create(filename);
            var outputSample = new T();
            scene.Time = null;

            scene.Write("/Foo", inputSample);

            scene.Save();
            scene.Close();

            var varMap = new USD.NET.AccessMask();
            var scene2 = USD.NET.Scene.Open(filename);
            scene2.AccessMask = varMap;
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.Close();

            Assert.Zero(varMap.Included.Count, "Expected zero dynamic prims and members.");

            System.IO.File.Delete(filename);
        }

        static protected void TestTimeOne_NotVarying<T>(T inputSample)
            where T : USD.NET.SampleBase, new()
        {
            // ----------------------------------------------- //
            // Test with time = 1.0 --> not varying.
            // ----------------------------------------------- //
            var filename = GetTempFile();
            var scene = USD.NET.Scene.Create(filename);
            var outputSample = new T();
            scene.Time = 1.0;

            scene.Write("/Foo", inputSample);

            scene.Save();
            scene.Close();

            var varMap = new USD.NET.AccessMask();
            var scene2 = USD.NET.Scene.Open(filename);
            scene2.AccessMask = varMap;
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.Close();

            Assert.Zero(varMap.Included.Count, "Expected zero dynamic prims and members");
            System.IO.File.Delete(filename);
        }

        static protected void TestTimeOneTwo_SameSample_NotVarying<T>(T inputSample)
            where T : USD.NET.SampleBase, new()
        {
            // ----------------------------------------------- //
            // Test with time = 1.0, 2.0 --> same sample, not varying (sparse writing)
            // ----------------------------------------------- //
            var filename = GetTempFile();
            var scene = USD.NET.Scene.Create(filename);
            var outputSample = new T();

            scene.Time = 1.0;
            scene.Write("/Foo", inputSample);

            scene.Time = 2.0;
            scene.Write("/Foo", inputSample);

            scene.Save();
            scene.Close();

            var varMap = new USD.NET.AccessMask();
            var scene2 = USD.NET.Scene.Open(filename);
            scene2.Time = 1.0;
            scene2.IsPopulatingAccessMask = true;
            scene2.AccessMask = varMap;
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.Close();

            // Expect nothing dynamic, since the same exact values were written at 2 different times
            Assert.Zero(varMap.Included.Count);

            System.IO.File.Delete(filename);
        }

        static protected void TestTimeOneTwo_DifferentSample_Varying<T>(T inputSample, T inputSample2)
            where T : USD.NET.SampleBase, new()
        {
            // ----------------------------------------------- //
            // Test with time = 1.0, 2.0 --> different sample, varying
            // ----------------------------------------------- //
            var filename = GetTempFile();
            var scene = USD.NET.Scene.Create(filename);
            var outputSample = new T();

            scene.Time = 1.0;
            scene.Write("/Foo", inputSample);

            scene.Time = 2.0;
            scene.Write("/Foo", inputSample2);

            scene.Save();
            scene.Close();

            var varMap = new USD.NET.AccessMask();
            var scene2 = USD.NET.Scene.Open(filename);
            scene2.Time = 1.0;
            scene2.AccessMask = varMap;
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);

            // Still expect nothing dynamic, since varMap was never populated.
            Assert.Zero(varMap.Included.Count, "There should be zero dynamics when the variabilityMap is not poppulated.");

            scene2.IsPopulatingAccessMask = true;
            scene2.AccessMask = varMap;
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);

            // Extra reads while populating should result in the same map values.
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.IsPopulatingAccessMask = false;

            // Reading while not populating should not clear the map.
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.Close();

            Assert.NotZero(varMap.Included.Count);
            Assert.NotZero(varMap.Included[new pxr.SdfPath("/Foo")].dynamicMembers.Count);

            foreach (var memberInfo in varMap.Included[new pxr.SdfPath("/Foo")].dynamicMembers)
            {
                var fi = memberInfo as System.Reflection.FieldInfo;
                var pi = memberInfo as System.Reflection.PropertyInfo;
                object vIn = null;
                object vOut = null;
                if (fi != null && fi.FieldType.IsClass)
                {
                    vIn = fi.GetValue(inputSample);
                    vOut = fi.GetValue(outputSample);
                }
                else if (pi != null && pi.PropertyType.IsClass)
                {
                    vIn = pi.GetValue(inputSample, null);
                    vOut = pi.GetValue(outputSample, null);
                }

                AssertEqual(vIn, vOut);
            }

            System.IO.File.Delete(filename);
        }

        static protected void TestTimeOneTwo_SameSample_RefNotPopulated<T>(T inputSample, T inputSample2)
            where T : USD.NET.SampleBase, new()
        {
            // ----------------------------------------------- //
            // Test that reference values are not populated.
            // ----------------------------------------------- //
            var filename = GetTempFile();
            var scene = USD.NET.Scene.Create(filename);
            var outputSample = new T();

            scene.Time = null;
            scene.Write("/Foo", inputSample);

            // Bar IS time varying.
            scene.Time = 1.0;
            scene.Write("/Foo/Bar", inputSample);
            scene.Time = 2.0;
            scene.Write("/Foo/Bar", inputSample2);

            scene.Save();
            scene.Close();

            var scene2 = USD.NET.Scene.Open(filename);
            scene2.Time = 1.0;
            var varMap = new USD.NET.AccessMask();
            scene2.IsPopulatingAccessMask = true;
            scene2.AccessMask = varMap;
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.Read(new pxr.SdfPath("/Foo/Bar"), outputSample);
            scene2.IsPopulatingAccessMask = false;

            // Variability map now has all </Bar> members cached as time-varying.
            outputSample = new T();
            var barSample = new T();
            scene2.Read(new pxr.SdfPath("/Foo"), outputSample);
            scene2.Read(new pxr.SdfPath("/Foo/Bar"), barSample);
            scene2.Close();

            // Assert that all </Foo> values are default.
            var defaultSample = new T();
            Assert.False(varMap.Included.ContainsKey(new pxr.SdfPath("/Foo")));
            var bindFlags = System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance;
            foreach (var memberInfo in typeof(T).GetMembers(bindFlags))
            {
                var fi = memberInfo as System.Reflection.FieldInfo;
                var pi = memberInfo as System.Reflection.PropertyInfo;
                if (fi != null && fi.FieldType.IsClass)
                {
                    AssertEqual(fi.GetValue(outputSample), fi.GetValue(defaultSample));
                }
                else if (pi != null && pi.PropertyType.IsClass)
                {
                    AssertEqual(pi.GetValue(outputSample, null), fi.GetValue(defaultSample));
                }
            }

            // Assert that all </Bar> values are non-default.
            foreach (var memberInfo in varMap.Included[new pxr.SdfPath("/Foo/Bar")].dynamicMembers)
            {
                var fi = memberInfo as System.Reflection.FieldInfo;
                var pi = memberInfo as System.Reflection.PropertyInfo;
                if (fi != null && fi.FieldType.IsClass)
                {
                    AssertEqual(fi.GetValue(barSample), fi.GetValue(inputSample));
                }
                else if (pi != null && pi.PropertyType.IsClass)
                {
                    AssertEqual(pi.GetValue(barSample, null), fi.GetValue(inputSample));
                }
            }

            System.IO.File.Delete(filename);
        }
    }
}
