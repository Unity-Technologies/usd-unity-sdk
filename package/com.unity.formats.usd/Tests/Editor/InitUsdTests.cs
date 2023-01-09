using System.IO;
using NUnit.Framework;
using System.Reflection;
using Unity.Formats.USD;
using UnityEngine;

public class InitUsdTests
{
    [Test]
    [Ignore("[USDU-249]")]
    public void SetupUsdPath_InvalidPath_Error()
    {
        var invalidFilePath = "\\NonExisting\\Path\\";

        // SetupUsdPath function is a private function
        var setUpMethod = GetMethod("SetupUsdPath");

        var expectedError = Assert.Throws<TargetInvocationException>(() => setUpMethod.Invoke(null, new object[] { invalidFilePath }), "Error was expected but not thrown");
        Assert.IsInstanceOf<FileNotFoundException>(expectedError.InnerException);
        Assert.AreEqual(string.Format("Could not find file '{0}'.", invalidFilePath), expectedError.InnerException.Message, "Unexpected error message was given");
    }

    [Test]
    public void SetupUsdPath_EmptyPath_Error()
    {
        // SetupUsdPath function is a private function
        var setUpMethod = GetMethod("SetupUsdPath");

        var expectedError = Assert.Throws<TargetInvocationException>(() => setUpMethod.Invoke(null, new object[] { "" }), "Error was expected but not thrown");
        Assert.IsInstanceOf<System.ArgumentException>(expectedError.InnerException);
        Assert.AreEqual("The specified path is not of a legal form (empty).", expectedError.InnerException.Message, "Unexpected error message was given");
    }

    [Test]
    public void InitUsd_Initialize()
    {
        // Reset 'm_usdInitialized' for accurate testing
        ResetInitUsd();

        Assert.True(InitUsd.Initialize(), "USD Initialize failed");
    }

    [TearDown]
    public void ResetInitUsd()
    {
        var isUsdInitialized = typeof(InitUsd).GetField("m_usdInitialized", (BindingFlags.Static | BindingFlags.NonPublic));
        isUsdInitialized.SetValue(null, false);
    }

    private MethodInfo GetMethod(string methodName)
    {
        var method = typeof(InitUsd).GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Static));

        if (method == null)
        {
            Assert.Fail(string.Format("{0} method not found", methodName));
        }

        return method;
    }
}
