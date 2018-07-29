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

%module(directors="1") Callback

%{
#include "pxr/base/work/loops.h"
#include <functional>
#include "diagnosticHandler.h"
%}

%feature("director") TaskCallback;
%feature("director") DiagnosticHandler;

%include "diagnosticHandler.h"

%inline %{
  class TaskCallback {
  public:
    virtual ~TaskCallback() {}
    virtual void Run(size_t start, size_t end) {}
  };

  void TestCall(TaskCallback& cb, int start, int end) {
    cb.Run(start, end);
  }

  typedef std::vector<TaskCallback> TaskCallbackVector;
  void ParallelForN(size_t n, TaskCallback& dispatch) {
    WorkParallelForN(n, [&dispatch](size_t start, size_t end) { dispatch.Run(start, end); });
  }

%}

%include "std_vector.i"
namespace std {
  %template(TaskCallbackVector) vector<TaskCallback>;
}