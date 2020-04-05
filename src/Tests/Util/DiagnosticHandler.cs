using System;

namespace Tests.Util {
  internal class DiagnosticHandler : pxr.DiagnosticHandler {

    public static DiagnosticHandler Instance {get; private set; }

    public string LastError;

    public DiagnosticHandler() : base() {
      Instance = this;
    }

    public override void OnError(string msg) {
      LastError = msg;

      // TODO: Something is not right here, it seems throwing an exception from OnError causes the
      //       diagnostic handler to somehow get disconnected from the underlying handler.
      //throw new System.ApplicationException("USD ERROR: " + msg);
    }

    public override void OnWarning(string msg) {
      Console.WriteLine(msg);
    }

    public override void OnFatalError(string msg) {
      LastError = msg;
      throw new System.Exception("USD FATAL ERROR: " + msg);
    }

    public override void OnInfo(string msg) {
      Console.WriteLine(msg);
    }
  }
}
