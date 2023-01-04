using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using USD.NET;
using USD.NET.Unity;
using System.Reflection;
using Unity.Formats.USD;
using System.Runtime.CompilerServices;

public class InitUsdTests
{
    [Test]
    [Ignore("[USDU-249]")]
    public void SetupUsdPath_InvalidPath_Error()
    {
        // SetupUsdPath function is a private function
        var setUpMethod = GetMethod("SetupUsdPath");

        try
        {
            setUpMethod.Invoke(null, new object[] { "\\NonExisting\\Path\\" });
        }
        catch (TargetInvocationException e)
        {
            Assert.AreEqual(e.InnerException.GetType(), typeof(FileNotFoundException));
            return;
        }

        Assert.Fail("Exception was expected but none was thrown");
    }

    [Test]
    public void SetupUsdPath_EmptyPath_Error()
    {
        // SetupUsdPath function is a private function
        var setUpMethod = GetMethod("SetupUsdPath");

        try
        {
            setUpMethod.Invoke(null, new object[] { "" });
        }
        catch (TargetInvocationException e)
        {
            Assert.AreEqual(e.InnerException.GetType(), typeof(System.ArgumentException));
            return;
        }

        Assert.Fail("Exception was expected but none was thrown");
    }

    [Test]
    public void InitUsd_Initialize()
    {
        // Reset 'm_usdInitialized' for accurate testing
        var isUsdInitialized = typeof(InitUsd).GetField("m_usdInitialized", (BindingFlags.Static | BindingFlags.NonPublic));
        isUsdInitialized.SetValue(null, false);

        Assert.True(InitUsd.Initialize());
    }

    private MethodInfo GetMethod(string methodName)
    {
        var method = typeof(InitUsd).GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Static));

        if (method == null)
            Assert.Fail(string.Format("{0} method not found", methodName));

        return method;
    }
}
