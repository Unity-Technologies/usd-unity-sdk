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

import sys, os
import vt, sdf
import usdGeom, usdShade, usdSkel, usdLux, usdRi, usdVol, kind

basePath = "src/Swig/pxr/base/"
usdPath = "src/Swig/pxr/usd/"
usdInstPath = "src/USD.NET/"

copyright = """// Copyright 2017 Google Inc. All rights reserved.
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
"""

if __name__ == "__main__":
    if not os.path.exists(basePath):
        print "Output path does not exist: " + basePath
        sys.exit(1)
    if not os.path.exists(usdPath):
        print "Output path does not exist: " + usdPath
        sys.exit(1)

    print "Generating Sdf "
    sdf.genSdfValueTypeNames(usdPath, usdInstPath, copyright)

    print "Generating Vt "
    vt.genVtValue(basePath, copyright)

    print "Generating UsdGeom "
    usdGeom.genUsdGeomTokens(usdPath, copyright)

    print "Generating UsdLux "
    usdLux.genUsdLuxTokens(usdPath, copyright)

    print "Generating UsdRi "
    usdRi.genUsdRiTokens(usdPath, copyright)

    print "Generating UsdShade "
    usdShade.genUsdShadeTokens(usdPath, copyright)

    print "Generating UsdSkel "
    usdSkel.genUsdSkelTokens(usdPath, copyright)

    print "Generating Kind "
    kind.genKindTokens(usdPath, copyright)

    # Disabled until this issue is resolved:
    # https://github.com/PixarAnimationStudios/USD/issues/658
    #
    #print "Generating UsdVol "
    #usdVol.genUsdVolTokens(usdPath, copyright)


