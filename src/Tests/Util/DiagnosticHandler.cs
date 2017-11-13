using System;

namespace Tests.Util {
  internal class DiagnosticHandler : pxr.DiagnosticHandler {

    public override void OnError(string msg) {
      base.OnError(msg);
      throw new System.ApplicationException("USD FATAL ERROR: " + msg);
    }

    public override void OnWarning(string msg) {
      base.OnWarning(msg);
      Console.WriteLine(msg);
    }

    public override void OnFatalError(string msg) {
      base.OnFatalError(msg);
      throw new System.Exception("USD ERROR: " + msg);
    }

    public override void OnInfo(string msg) {
      base.OnInfo(msg);
      Console.WriteLine(msg);
    }
  }
}
