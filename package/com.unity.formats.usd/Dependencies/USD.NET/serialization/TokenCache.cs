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

using System.Collections.Generic;

namespace USD.NET
{
    /// <summary>
    /// USD TfTokens are expensive to create and destroy, both in C# and in C++. This class provides a
    /// thread safe caching mechanism to avoid TfToken churn.
    /// </summary>
    internal class TokenCache
    {
        private Dictionary<string, Dictionary<string, pxr.TfToken>> m_cache =
            new Dictionary<string, Dictionary<string, pxr.TfToken>>();

        /// <summary>
        /// Internal helper function to access the token cache.
        /// </summary>
        /// <param name="ns">The namespace prefix for the token, may be null.</param>
        /// <param name="token">The string to tokenize, may be null.</param>
        /// <returns>A TfToken holding the namespaced token string.</returns>
        /// <example>GetCache("foo:bar", "baz") returns TfToken("foo:bar:baz")</example>
        private pxr.TfToken GetCache(string ns, string token)
        {
            if (ns == null) { ns = string.Empty; }
            if (token == null) { token = string.Empty; }

            pxr.TfToken val;
            lock (this) {
                // TODO: the lock and nested lookup here could be more efficient.

                // First lookup the namespace cache.
                Dictionary<string, pxr.TfToken> nsCache;
                if (!m_cache.TryGetValue(ns, out nsCache))
                {
                    nsCache = new Dictionary<string, pxr.TfToken>();
                    m_cache[ns] = nsCache;
                }

                // Then lookup the token in the namespace.
                if (!nsCache.TryGetValue(token, out val))
                {
                    val = new pxr.TfToken(IntrinsicTypeConverter.JoinNamespace(ns, token));
                    nsCache[token] = val;
                }
            }
            return val;
        }

        /// <summary>
        /// Returns a TfToken representing the given string. The result is cached for future use.
        /// </summary>
        public pxr.TfToken this[string token]
        {
            get { return GetCache(string.Empty, token); }
        }

        /// <summary>
        /// Returns a Tftoken representing the given namespace and string. The result is cached for
        /// future use.
        /// </summary>
        public pxr.TfToken this[string usdNamespace, string token]
        {
            get { return GetCache(usdNamespace, token); }
        }
    }
}
