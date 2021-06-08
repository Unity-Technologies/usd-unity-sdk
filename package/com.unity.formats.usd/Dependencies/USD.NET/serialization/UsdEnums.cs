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

namespace USD.NET
{
    public enum Variability
    {
        Varying,
        Uniform,
    }

    public enum Visibility
    {
        Inherited, // Default
        Invisible,
    }

    public enum Purpose
    {
        Default,  // Default
        Render,
        Proxy,
        Guide,
    }

    public enum Orientation
    {
        RightHanded, // Default
        LeftHanded,
    }

    public enum SubdivScheme
    {
        None,     // Default
        CatmullClark,
        Loop,
        Bilinear,
    }

    public enum PrimvarInterpolation
    {
        Constant,
        Uniform,
        Varying,
        Vertex,
        FaceVarying
    }
}
