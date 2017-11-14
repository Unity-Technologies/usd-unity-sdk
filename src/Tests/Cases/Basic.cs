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

namespace Tests.Cases {
  class Basic : UnitTest {

    class MinimalSample : USD.NET.SampleBase {
      public int number;
    }

    class IntrinsicsSample : USD.NET.SampleBase {
      public bool bool_;
      public byte byte_;
      public int int_;
      public uint uint_;
      public long long_;
      public ulong ulong_;
      public string string_;
      public float float_;
      public double double_;

      public bool[] boolArray_;
      public byte[] byteArray_;
      public int[] intArray_;
      public uint[] uintArray_;
      public long[] longArray_;
      public ulong[] ulongArray_;
      public string[] stringArray_;
      public float[] floatArray_;
      public double[] doubleArray_;
    }

    public static void SmokeTest() {
      var sample = new MinimalSample();
      var sample2 = new MinimalSample();

      sample.number = 42;
      WriteAndRead(ref sample, ref sample2, true);

      if (sample2.number != sample.number) { throw new Exception("Values do not match"); }
    }

    public static void IntrinsicTypes() {
      var sample = new IntrinsicsSample();
      var sample2 = new IntrinsicsSample();

      sample.boolArray_ = new bool[] { false, true };
      sample.bool_ = true;

      sample.byteArray_ = new byte[] { 1, 2, 3 };
      sample.byte_ = 42;

      sample.doubleArray_ = new double[] { -1.1, 2.2, double.MaxValue, double.MinValue };
      sample.double_ = double.MaxValue;

      sample.floatArray_ = new float[] { -1.1f, 2.2f, float.MaxValue, float.MinValue };
      sample.float_ = float.MaxValue;

      sample.intArray_ = new int[] { -1, 0, 1, 2, int.MaxValue, int.MinValue };
      sample.int_ = int.MaxValue;

      sample.longArray_ = new long[] { -1, 0, 2, long.MaxValue, long.MinValue };
      sample.long_ = long.MinValue;

      sample.stringArray_ = new string[] { "hello", "world" };
      sample.string_ = "foobar";

      sample.uintArray_ = new uint[] { 0, 1, 2, uint.MaxValue, uint.MinValue };
      sample.uint_ = uint.MaxValue;

      sample.ulongArray_ = new ulong[] { 0, 2, ulong.MaxValue, ulong.MinValue };
      sample.ulong_ = ulong.MaxValue;

      WriteAndRead(ref sample, ref sample2, true);

      AssertEqual(sample2.boolArray_,   sample2.boolArray_);
      AssertEqual(sample.byteArray_,    sample2.byteArray_);
      AssertEqual(sample.doubleArray_,  sample2.doubleArray_);
      AssertEqual(sample.floatArray_,   sample2.floatArray_);
      AssertEqual(sample.intArray_,     sample2.intArray_);
      AssertEqual(sample.longArray_,    sample2.longArray_);
      AssertEqual(sample.stringArray_,  sample2.stringArray_);
      AssertEqual(sample.uintArray_,    sample2.uintArray_);
      AssertEqual(sample.ulongArray_,   sample2.ulongArray_);

      AssertEqual(sample2.bool_,        sample2.bool_);
      AssertEqual(sample.byte_,         sample2.byte_);
      AssertEqual(sample.double_,       sample2.double_);
      AssertEqual(sample.float_,        sample2.float_);
      AssertEqual(sample.int_,          sample2.int_);
      AssertEqual(sample.long_,         sample2.long_);
      AssertEqual(sample.string_,       sample2.string_);
      AssertEqual(sample.uint_,         sample2.uint_);
      AssertEqual(sample.ulong_,        sample2.ulong_);
    }

  }
}
