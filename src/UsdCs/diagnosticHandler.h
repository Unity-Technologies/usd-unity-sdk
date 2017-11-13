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

/// A reciever of diagnostic messages, sent from USD.
///
/// Only one global handler may be registered to recieve all diagnostic messages. If no handler is
/// assigned, diagnostic messages are printed to stdout and stderr accordingly.
class DiagnosticHandler {
  static DiagnosticHandler* m_instance;
public:

  /// Returns the currently registered global diagnostic handler, may be null.
  static DiagnosticHandler* GetGlobalHandler() {
    return m_instance;
  }

  /// Sets the global diagnostic handler, null to clear the handler.
  static void SetGlobalHandler(DiagnosticHandler* handler) {
    m_instance = handler;
  }

  virtual ~DiagnosticHandler() {}

  /// Informational messages.
  virtual void OnInfo(char const* msg);

  /// Diagnostic warning messages.
  virtual void OnWarning(char const* msg);

  /// Recoverable error messages, which should be treated as non-fatal exceptions.
  virtual void OnError(char const* msg);

  /// Messages recieved here will occur just before the application aborts.
  virtual void OnFatalError(char const* msg);
};