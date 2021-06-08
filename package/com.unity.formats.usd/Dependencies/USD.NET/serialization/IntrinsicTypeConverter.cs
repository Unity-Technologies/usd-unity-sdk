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
using System.Linq;
using pxr;

namespace USD.NET
{
    /// <summary>
    /// Converts intrinsic C# types to/from USD. This is serializaiton infrastructure and should only
    /// be needed when dealing directly with the low level USD API.
    /// </summary>
    [Preserve]
    public class IntrinsicTypeConverter
    {
        static public string MakeValidIdentifier(string unityIdentifier)
        {
            return UsdCs.TfMakeValidIdentifier(unityIdentifier);
        }

        /// <summary>
        /// Constructs namespaced string given two namespace element, if either element is omitted,
        /// the single namespace is returned with no divider. The result is cacehd.
        /// </summary>
        /// <returns>Returns the stringified namespace, never null.</returns>
        /// <example>JoinNamespace("foo", "bar") returns "foo:bar"</example>
        /// <example>JoinNamespace("foo", null) returns "foo"</example>
        static public string JoinNamespace(string first, string second)
        {
            if (string.IsNullOrEmpty(first))
            {
                return second ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(second))
            {
                return first ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(first) && string.IsNullOrEmpty(second))
            {
                return string.Empty;
            }
            else
            {
                return first + ":" + second;
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // string[], List<string> <--> TokenArray
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtTokenArray ToVtArray(string[] input)
        {
            var output = new VtTokenArray((uint)input.Length);
            // PERFORMANCE: this is super inefficient.
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = new pxr.TfToken(input[i]);
            }
            return output;
        }

        [Preserve]
        static public VtTokenArray ListToVtArray(List<string> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<string> ListFromVtArray(VtTokenArray input)
        {
            return FromVtArray(input).ToList();
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public string[] FromVtArray(VtTokenArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<string>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtTokenArray input, ref string[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<string>(input.size());
            }
            // PERFORMANCE: this is super inefficient.
            for (int i = 0; i < input.size(); i++)
            {
                output[i] = input[i];
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // SdfAssetPath[], List<SdfAssetPath> <--> SdfAssetPath
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public SdfAssetPathArray ToVtArray(SdfAssetPath[] input)
        {
            var output = new SdfAssetPathArray((uint)input.Length);
            // PERFORMANCE: this is super inefficient.
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = new SdfAssetPath(input[i].GetAssetPath());
            }
            return output;
        }

        [Preserve]
        static public SdfAssetPathArray ListToVtArray(List<SdfAssetPath> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<SdfAssetPath> ListFromVtArray(SdfAssetPathArray input)
        {
            return FromVtArray(input).ToList();
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public SdfAssetPath[] FromVtArray(SdfAssetPathArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<SdfAssetPath>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(SdfAssetPathArray input, ref SdfAssetPath[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<SdfAssetPath>(input.size());
            }
            // PERFORMANCE: this is super inefficient.
            for (int i = 0; i < input.size(); i++)
            {
                output[i] = input[i];
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // bool[], List<bool> <--> BoolArray
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtBoolArray ToVtArray(bool[] input)
        {
            var output = new VtBoolArray((uint)input.Length);
            unsafe
            {
                fixed(bool* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public bool[] FromVtArray(VtBoolArray input)
        {
            bool[] output = UsdIo.ArrayAllocator.Malloc<bool>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtBoolArray input, ref bool[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<bool>(input.size());
            }
            unsafe
            {
                fixed(bool* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
        }

        [Preserve]
        static public VtBoolArray ListToVtArray(List<bool> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<bool> ListFromVtArray(VtBoolArray input)
        {
            bool[] tmp = UsdIo.ArrayAllocator.Malloc<bool>(input.size());
            unsafe
            {
                fixed(bool* p = tmp)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
            return tmp.ToList();
        }

        // ----------------------------------------------------------------------------------------- //
        // byte[], List<byte> <--> UCharArray
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtUCharArray ToVtArray(byte[] input)
        {
            var output = new VtUCharArray((uint)input.Length);
            unsafe
            {
                fixed(byte* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        [Preserve]
        static public VtUCharArray ListToVtArray(List<byte> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<byte> ListFromVtArray(VtUCharArray input)
        {
            return FromVtArray(input).ToList();
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public byte[] FromVtArray(VtUCharArray input)
        {
            byte[] output = UsdIo.ArrayAllocator.Malloc<byte>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtUCharArray input, ref byte[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<byte>(input.size());
            }
            unsafe
            {
                fixed(byte* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // int[], List<int> <--> IntArray
        // ----------------------------------------------------------------------------------------- //

        [Preserve]
        static public VtIntArray ListToVtArray(List<int> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<int> ListFromVtArray(VtIntArray input)
        {
            return FromVtArray(input).ToList();
        }

        [Preserve]
        static public VtIntArray ToVtArray(int[] input)
        {
            var output = new VtIntArray((uint)input.Length);
            unsafe
            {
                fixed(int* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public int[] FromVtArray(VtIntArray input)
        {
            int[] output = UsdIo.ArrayAllocator.Malloc<int>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtIntArray input, ref int[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<int>(input.size());
            }
            unsafe
            {
                fixed(int* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // uint[], List<uint> <--> UIntArray
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtUIntArray ListToVtArray(List<uint> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<uint> ListFromVtArray(VtUIntArray input)
        {
            return FromVtArray(input).ToList();
        }

        [Preserve]
        static public VtUIntArray ToVtArray(uint[] input)
        {
            var output = new VtUIntArray((uint)input.Length);
            unsafe
            {
                fixed(uint* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public uint[] FromVtArray(VtUIntArray input)
        {
            uint[] output = UsdIo.ArrayAllocator.Malloc<uint>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtUIntArray input, ref uint[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<uint>(input.size());
            }
            unsafe
            {
                fixed(uint* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // long[], List<long> <--> Int64Array
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtInt64Array ListToVtArray(List<long> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<long> ListFromVtArray(VtInt64Array input)
        {
            return FromVtArray(input).ToList();
        }

        [Preserve]
        static public VtInt64Array ToVtArray(long[] input)
        {
            var output = new VtInt64Array((uint)input.Length);
            unsafe
            {
                fixed(long* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public long[] FromVtArray(VtInt64Array input)
        {
            long[] output = UsdIo.ArrayAllocator.Malloc<long>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtInt64Array input, ref long[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<long>(input.size());
            }
            unsafe
            {
                fixed(long* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // ulong[], List<ulong> <--> UInt64Array
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtUInt64Array ListToVtArray(List<ulong> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<ulong> ListFromVtArray(VtUInt64Array input)
        {
            return FromVtArray(input).ToList();
        }

        [Preserve]
        static public VtUInt64Array ToVtArray(ulong[] input)
        {
            var output = new VtUInt64Array((uint)input.Length);
            unsafe
            {
                fixed(ulong* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        // Convenience API: generates garbage, do not use when performance matters.
        [Preserve]
        static public ulong[] FromVtArray(VtUInt64Array input)
        {
            ulong[] output = UsdIo.ArrayAllocator.Malloc<ulong>(input.size());
            FromVtArray(input, ref output);
            return output;
        }

        [Preserve]
        static public void FromVtArray(VtUInt64Array input, ref ulong[] output)
        {
            if (output.Length != input.size())
            {
                output = UsdIo.ArrayAllocator.Malloc<ulong>(input.size());
            }
            unsafe
            {
                fixed(ulong* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
        }

        // ----------------------------------------------------------------------------------------- //
        // float[], List<float> <--> FloatArray
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtFloatArray ListToVtArray(List<float> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<float> ListFromVtArray(VtFloatArray input)
        {
            return FromVtArray(input).ToList();
        }

        [Preserve]
        static public VtFloatArray ToVtArray(float[] input)
        {
            var output = new VtFloatArray((uint)input.Length);
            unsafe
            {
                fixed(float* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        [Preserve]
        static public float[] FromVtArray(VtFloatArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<float>(input.size());
            unsafe
            {
                fixed(float* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
            return output;
        }

        // ----------------------------------------------------------------------------------------- //
        // double[], List<double> <--> DoubleArray
        // ----------------------------------------------------------------------------------------- //
        [Preserve]
        static public VtDoubleArray ListToVtArray(List<double> input)
        {
            return ToVtArray(input.ToArray());
        }

        [Preserve]
        static public List<double> ListFromVtArray(VtDoubleArray input)
        {
            return FromVtArray(input).ToList();
        }

        [Preserve]
        static public VtDoubleArray ToVtArray(double[] input)
        {
            var output = new VtDoubleArray((uint)input.Length);
            unsafe
            {
                fixed(double* p = input)
                {
                    output.CopyFromArray(new IntPtr(p));
                }
            }
            return output;
        }

        [Preserve]
        static public double[] FromVtArray(VtDoubleArray input)
        {
            var output = UsdIo.ArrayAllocator.Malloc<double>(input.size());
            unsafe
            {
                fixed(double* p = output)
                {
                    input.CopyToArray(new IntPtr(p));
                }
            }
            return output;
        }
    }
}
