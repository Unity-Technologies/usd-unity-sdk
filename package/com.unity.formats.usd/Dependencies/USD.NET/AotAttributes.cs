// Copyright 2020 Unity Technologies All rights reserved.
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

/// <summary>
/// Add this tag to make callback compatible with IL2CPP
/// </summary>
///
/// <remarks>
/// Why does this class exist?
/// See: https://github.com/Unity-Technologies/usd-unity-sdk/issues/100#issuecomment-512134215
///      https://github.com/Unity-Technologies/usd-unity-sdk/issues/100#issuecomment-589834716
///
/// SWIG Callbacks must be explicitly tagged as mono callbacks so the IL2CPP backend can correctly
/// generate C++ code. This is hack to work around the fact that USD.NET doesn't reference the
/// Mono runtime libraries.
///
/// Also see: https://docs.microsoft.com/en-us/dotnet/api/objcruntime.monopinvokecallbackattribute?view=xamarin-ios-sdk-12
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
class MonoPInvokeCallbackAttribute : Attribute
{
}

/// <summary>
/// Prevent a method, class, field, or property from being stripped by bytecode optimization.
/// </summary>
/// <remarks>
/// When IL2CPP optimizes the generated IL, unused code will be stripped. Any methods only called by reflection will
/// also be stripped in this way. To prevent this, this attribute is copied from Unity.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
class PreserveAttribute : Attribute
{
}
