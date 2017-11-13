# Copyright 2017 Google Inc. All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from pxr import Sdf

values = Sdf.ValueTypeNames.__dict__

accessorPre = "%inline %{"
accessor = """    SdfValueTypeName SdfGetValueType{valueTypeName}() {{ return SdfValueTypeNames->{valueTypeName}; }}"""
accessorPost = "%}"

klassPre = """
using System;
using pxr;
public static class SdfValueTypeNames {"""
klass = """    static public SdfValueTypeName {valueTypeName} = UsdCs.SdfGetValueType{valueTypeName}();"""
klassPost = "}"

def genSdfValueTypeNames(usdPath, usdInstPath, copyright):
  sdfValueTypes = usdPath + "sdf/sdfValueTypeNames_Types.i"
  sdfClass = usdInstPath + "SdfValueTypeNames.cs"

  values = [k for k in Sdf.ValueTypeNames.__dict__ if isinstance(Sdf.ValueTypeNames.__dict__[k],
            type(Sdf.ValueTypeNames.__dict__["Token"]))]
  with open(sdfValueTypes, "w") as f:
    print >> f, copyright
    print >> f, accessorPre
    for vtn in values:
      print >> f, accessor.format(valueTypeName=vtn)
    print >> f, accessorPost

  with open(sdfClass, "w") as f:
    print >> f, copyright
    print >> f, klassPre
    for vtn in values:
      print >> f, klass.format(valueTypeName=vtn)
    print >> f, klassPost
