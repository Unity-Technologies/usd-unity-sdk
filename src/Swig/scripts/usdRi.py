# Copyright 2018 Jeremy Cowles. All rights reserved.
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

from pxr import UsdRi

tokenPre = '%typemap(cscode) UsdRiTokens %{'
tokenDecl = '  public static TfToken {name} = new TfToken("{value}");'
tokenPost = '%}'

def genUsdRiTokens(usdPath, copyright):
    tokenPath = usdPath + "usdRi/tokens_Tokens.i"
    toks = [x for x in dir(UsdRi.Tokens) if not x.startswith("_")]
    with open(tokenPath, "w") as f:
        print >> f, copyright
        print >> f, tokenPre
        for t in toks:
            v = UsdRi.Tokens.__dict__[t].fget()
            # TODO: doc doesn't work for some reason, need to investigate
            #doc = UsdRi.Tokens.__dict__[t].__doc__
            #if not doc is None:
            #  print '///<summary>' + doc + '</summary>'
            print >> f, tokenDecl.format(name=t, value=v)
        print >> f, tokenPost
