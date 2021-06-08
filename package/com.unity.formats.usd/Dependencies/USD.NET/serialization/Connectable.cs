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
using pxr;

namespace USD.NET
{
    /// <summary>
    /// An interface for type-erased access to a Connectable T.
    /// </summary>
    public interface Connectable
    {
        object GetValue();
        Type GetValueType();
        string GetConnectedPath();
        void SetConnectedPath(string path);
        void SetValue(object value);
    }

    /// <summary>
    /// A USD attribute which can have a value or be connected to another attribute of matching type.
    /// Conceptually, the targeted attribute value will flow into this attribute. The default value
    /// is the value which will be specified when the attribute is not connected or the connection is
    /// broken. The default value is only stored when the Connectable object is serialized and may be
    /// time varying or uniform.
    /// </summary>
    /// <typeparam name="T">The underlying type held by the attribute.</typeparam>
    public class Connectable<T> : Connectable
    {
        public T defaultValue;
        public string connectedPath { get; private set; }

        public Connectable()
        {
        }

        public Connectable(T defaultValue)
        {
            this.defaultValue = defaultValue;
        }

        /// <summary>
        /// Returns true when a path has been specified, but does not check is this is a valid path in
        /// the scene.
        /// </summary>
        public bool IsConnected() { return !string.IsNullOrEmpty(connectedPath); }

        /// <summary>
        /// Returns the C# type of this connectable object.
        /// </summary>
        public Type GetValueType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Gets the local value of the attribute, for use when no connection is established.
        /// </summary>
        public object GetValue()
        {
            return defaultValue;
        }

        /// <summary>
        /// Sets the local value of the attribute, for use when no connection is established.
        /// </summary>
        public void SetValue(object value)
        {
            defaultValue = (T)value;
        }

        /// <summary>
        /// Gets the connection path, which may or may not be a valid connection.
        /// </summary>
        public string GetConnectedPath()
        {
            return connectedPath;
        }

        /// <summary>
        /// Connects this attribute to the targeted path.
        /// </summary>
        public void SetConnectedPath(string path)
        {
            connectedPath = new SdfPath(path).ToString();
        }

        /// <summary>
        /// Connects this attribute to the targeted attribute. Note that the path and attribute must
        /// form a valid SdfPath.
        /// </summary>
        public void SetConnectedPath(string path, string attribute)
        {
            connectedPath = new SdfPath(path).AppendProperty(new TfToken(attribute)).ToString();
        }
    }
}
