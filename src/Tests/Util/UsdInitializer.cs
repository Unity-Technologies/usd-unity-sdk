using System;
using System.IO;

namespace Tests.Util {
  /// <summary>
  /// Helper class for initializing USD. The assumption is that the USD DLLs are located at the path
  /// specified by the USD_LOCATION environment variable. The key property of the code below is that
  /// it modifies the system PATH before the USD.NET API is touched.
  /// 
  /// WARNING: The modified PATH environment variable below must happen before the MS CRT is 
  /// initialized, since it will take a snapshot of all environment vars and never update it. It
  /// seems some build configurations will invalidate this requirement, resulting in failed
  /// initialization of UsdCs.dll.
  /// 
  /// For more details, see: https://msdn.microsoft.com/en-us/library/ms235460.aspx
  /// 
  /// </summary>
  internal static class UsdInitializer {
    public static int forceInit = 1;
    static object sm_diagnosticHandler;

    /// <summary>
    /// Static constructor, primary entry point of initialization.
    /// </summary>
    static UsdInitializer() {
      var targetVariables = EnvironmentVariableTarget.Process;
      var usdLocation = GetUsdLocation();
      var path = Environment.GetEnvironmentVariable("PATH", targetVariables);

      if (path.IndexOf(usdLocation) < 0) {
        // Assumes the standard USD build structure, may need to be modified for linux.
        path = path + ";" + Path.Combine(usdLocation, "lib");
        path = path + ";" + Path.Combine(usdLocation, "bin");
        Environment.SetEnvironmentVariable("PATH", path, targetVariables);
      }

      // We want to register the diagnostic handler before registering plugins, since they may spew
      // some useful warnings when things go wrong.
      var handler = new DiagnosticHandler();
      pxr.DiagnosticHandler.SetGlobalHandler(handler);

      // Save a copy so it doesn't get garbage collected while set as the handler.
      sm_diagnosticHandler = handler;

      // Init plugins (e.g. file formats, etc).
      var pluginsLocation = GetUsdPluginPath();
      pxr.PlugRegistry.GetInstance().RegisterPlugins(pluginsLocation);

      // Init USD.NET.Unity type bindings.
      USD.NET.Unity.UnityTypeBindings.RegisterTypes();
    }

    /// <summary>
    /// Helper function to get the location of USD libraries and plugins.
    /// </summary>
    public static string GetUsdLocation() {
      var usdLoc = Environment.GetEnvironmentVariable("USD_LOCATION",
          EnvironmentVariableTarget.Process);
      if (string.IsNullOrEmpty(usdLoc)) {
        throw new Exception("Environment variable 'USD_LOCATION' undefined, cant initialize");
      }
      return usdLoc;
    }

    /// <summary>
    /// Helper function to get the USD plugins location, assuming a standard install.
    /// </summary>
    public static string GetUsdPluginPath() {
      return Path.Combine(GetUsdLocation(), @"share\usd\plugins");
    }
  }
}
