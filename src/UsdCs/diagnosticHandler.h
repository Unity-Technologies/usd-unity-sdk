// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#pragma once

//#include <stdio.h>
#include <string>
//#include <stdio.h>
//#include <sstream>
#include <pxr/base/tf/weakPtr.h>
#include <pxr/base/tf/diagnostic.h>
#include <pxr/base/tf/diagnosticMgr.h>
#include <pxr/base/arch/export.h>



extern "C"
{
  //Create a callback delegate
  typedef void(*FuncLogCallBack)(int logType, char const* message);
  static FuncLogCallBack s_UsdLogCallback = nullptr;
  ARCH_EXPORT void RegisterUsdLogCallback(FuncLogCallBack cb);
}

/// A reciever of diagnostic messages, sent from USD.
///
/// Only one global handler may be registered to recieve all diagnostic messages. If no handler is
/// assigned, diagnostic messages are printed to stdout and stderr accordingly.
class DiagnosticHandler : public pxr::TfDiagnosticMgr::Delegate {
public:
  virtual ~DiagnosticHandler() {}

  /// Informational messages.
  ARCH_EXPORT virtual void IssueStatus(pxr::TfStatus const &status);

  /// Diagnostic warning messages.
  ARCH_EXPORT virtual void IssueWarning(pxr::TfWarning const &warning);

  /// Recoverable error messages, which should be treated as non-fatal exceptions.
  ARCH_EXPORT virtual void IssueError(pxr::TfError const &err);

  /// Messages recieved here will occur just before the application aborts.
  ARCH_EXPORT virtual void IssueFatalError(pxr::TfCallContext const &context, std::string const &msg);

private:
  void _Send(int logType, char const* msg);
};
