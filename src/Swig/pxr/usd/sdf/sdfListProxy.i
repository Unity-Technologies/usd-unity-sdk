// Copyright 2018 Jeremy Cowles. All rights reserved.
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

%module sdfListProxy

%{
#include "pxr/usd/sdf/listProxy.h"
#include "pxr/usd/sdf/proxyTypes.h"
%}

// Rather than trying to expose the all-singing all-dancing template, just wrap
// a minimal, simplified, explicit interface for SdfSublayerProxy.

class SdfSubLayerProxy {
private:
    // Don't allow construction from C#.
    // Note that this is not an accurate reflection of the C++ class.
    SdfSubLayerProxy();

public:
    typedef std::string value_type;

    void push_back(const value_type& layerPath);
    size_t size();

    SdfPath GetPath() const;
    size_t Count(const value_type& value) const;
    size_t Find(const value_type& value) const;
    void Insert(int index, const value_type& value);
    void Remove(const value_type& value);
    void Replace(const value_type& oldValue, const value_type& newValue);
    void Erase(size_t index);

    bool IsExpired() const;
};