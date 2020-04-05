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

#include <mutex>
std::once_flag reg;

#if defined(_WIN64)
BOOL WINAPI DllMain(_In_ HINSTANCE hinstDLL, _In_ DWORD fdwReason, _In_ LPVOID lpvReserved) {
#endif
#if defined(__APPLE__) || defined(__linux__)
__attribute__((constructor)) void DllMain() {
#endif
  std::call_once(reg, [] {
    TfDiagnosticMgr::GetInstance().AddDelegate(new DiagnosticHandler());

  });
#if defined(_WIN64)
  _CrtSetReportMode(_CRT_ASSERT, _CRTDBG_MODE_DEBUG);
  return true;
#endif
}
