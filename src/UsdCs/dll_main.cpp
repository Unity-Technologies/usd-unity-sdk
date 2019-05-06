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

#if defined(_WIN64)
#include <Windows.h>
#endif

#include <iostream>

#include "diagnosticHandler.h"

#include "pxr/pxr.h"

#include <pxr/base/arch/threads.h>
#include <pxr/base/tf/weakPtr.h>
#include <pxr/base/tf/diagnostic.h>
#include <pxr/base/tf/diagnosticMgr.h>

PXR_NAMESPACE_USING_DIRECTIVE

// Low-level diagnostic message handler for USD.
//
// Abstracts the low-level API provided by USD to higher level DiagnosticHandler API, exposed to C#.
// When no C# handler is provided, messages are spewed to stderr/stdout.
class CsharpDelegate : public TfDiagnosticMgr::Delegate {
  static CsharpDelegate* m_instance;

  CsharpDelegate() {
  }

public:

  static CsharpDelegate* GetInstance() {
    if (m_instance == NULL) {
      m_instance = new CsharpDelegate();
    }
    return m_instance;
  }

  virtual ~CsharpDelegate() { }

  /// Called when a \c TfError is posted.
  virtual void IssueError(TfError const &err) {
    // A runtime_error exception handler is specified in pxr.i in the swig source.
    DiagnosticHandler* handler = DiagnosticHandler::GetGlobalHandler();
    if (handler != nullptr) {
      handler->OnError(err.GetCommentary().c_str());
    } else {
      fprintf(stderr, "ERROR: %s\n", err.GetCommentary().c_str());
    }
  }

  /// Called when a \c TF_FATAL_ERROR is issued (or a failed
  /// \c TF_AXIOM).
  virtual void IssueFatalError(TfCallContext const &context, std::string const &msg) {
    DiagnosticHandler* handler = DiagnosticHandler::GetGlobalHandler();
    if (handler != nullptr) {
      handler->OnFatalError(msg.c_str());
    } else {
      fprintf(stderr, "FATAL ERROR: %s\n", msg.c_str());
    }
  }

  /// Called when a \c TF_STATUS() is issued.
  virtual void IssueStatus(TfStatus const &status) {
    DiagnosticHandler* handler = DiagnosticHandler::GetGlobalHandler();
    if (handler != nullptr) {
      handler->OnInfo(status.GetCommentary().c_str());
    } else {
      fprintf(stdout, "STATUS: %s\n", status.GetCommentary().c_str());
    }
  }

  /// Called when a \c TF_WARNING() is issued.
  virtual void IssueWarning(TfWarning const &warning) {
    DiagnosticHandler* handler = DiagnosticHandler::GetGlobalHandler();
    if (handler != nullptr) {
      handler->OnInfo(warning.GetCommentary().c_str());
    } else {
      fprintf(stdout, "WARNING: %s\n", warning.GetCommentary().c_str());
    }
  }
};
CsharpDelegate* CsharpDelegate::m_instance = NULL;

#include <mutex>
std::once_flag reg;

#if defined(_WIN64)
BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved) {
#endif
#if defined(__APPLE__) || defined(__linux__)
__attribute__((constructor)) void DllMain() {
#endif
  std::call_once(reg, [] {
    TfDiagnosticMgr::GetInstance().AddDelegate(CsharpDelegate::GetInstance());
  });
#if defined(_WIN64)
  _CrtSetReportMode(_CRT_ASSERT, _CRTDBG_MODE_DEBUG);
  return true;
#endif
}
