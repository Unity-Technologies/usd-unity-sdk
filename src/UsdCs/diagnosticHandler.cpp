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

#include "diagnosticHandler.h"
#include <iostream>

#include <pxr/base/tf/weakPtr.h>
#include <pxr/base/tf/diagnostic.h>
#include <pxr/base/tf/diagnosticMgr.h>

//Create a callback delegate
void RegisterUsdLogCallback(FuncLogCallBack cb) {
  s_UsdLogCallback = cb;
}

void DiagnosticHandler::_Send(int logType, char const* msg) {
  if (s_UsdLogCallback == nullptr) {
    std::cerr << "USD: " << msg << std::endl;
    return;
  }

  s_UsdLogCallback(logType, msg);
}

/*virtual*/
void DiagnosticHandler::IssueStatus(pxr::TfStatus const &status) {
  _Send(0, status.GetCommentary().c_str());
}

/*virtual*/
void DiagnosticHandler::IssueWarning(pxr::TfWarning const &warning) {
  _Send(1, warning.GetCommentary().c_str());
}

/*virtual*/
void DiagnosticHandler::IssueError(pxr::TfError const &err) {
  _Send(2, err.GetCommentary().c_str());
}

/*virtual*/ 
void DiagnosticHandler::IssueFatalError(pxr::TfCallContext const &context, std::string const &msg) {
  _Send(3, msg.c_str());
}
