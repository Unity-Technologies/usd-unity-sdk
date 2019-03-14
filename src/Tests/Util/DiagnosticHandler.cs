using System;

namespace Tests.Util {
  internal class DiagnosticHandler : pxr.DiagnosticHandler {

    public string LastError;

    public static DiagnosticHandler Instance { get; private set; }

    public DiagnosticHandler() {
      Instance = this;
    }

    public override void OnError(string msg) {
      base.OnError(msg);
      LastError = msg;

      // TODO: Something is not right here, it seems throwing an exception from OnError causes the
      //       diagnostic handler to somehow get disconnected from the underlying handler.
      //throw new System.ApplicationException("USD ERROR: " + msg);
    }

    public override void OnWarning(string msg) {
      base.OnWarning(msg);
      Console.WriteLine(msg);
    }

    public override void OnFatalError(string msg) {
      base.OnFatalError(msg);
      LastError = msg;
      throw new System.Exception("USD FATAL ERROR: " + msg);
    }

    public override void OnInfo(string msg) {
      base.OnInfo(msg);
      Console.WriteLine(msg);
    }
  }
}
