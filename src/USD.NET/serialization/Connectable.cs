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

using System;
using System.Collections.Generic;
using pxr;

namespace USD.NET {

  /// <summary>
  /// An interface for type-erased access to a Connectable<T>.
  /// </summary>
  public interface Connectable {
    object GetValue();
    string GetConnectedPath();

    void SetValue(object value);
    void SetConnectedPath(string path);
  }

  /// <summary>
  /// A USD attribute which can have a value or be connected to another attribute of matching type.
  /// </summary>
  /// <typeparam name="T">The underlying type held by the attribute.</typeparam>
  public class Connectable<T> : Connectable {
    public T defaultValue;
    public string connectedPath;

    public bool IsConnected() { return !string.IsNullOrEmpty(connectedPath); }

    public object GetValue() {
      return defaultValue;
    }

    public string GetConnectedPath() {
      return connectedPath;
    }

    public void SetValue(object value) {
      defaultValue = (T)value;
    }

    public void SetConnectedPath(string path) {
      connectedPath = path;
    }
  }
}
