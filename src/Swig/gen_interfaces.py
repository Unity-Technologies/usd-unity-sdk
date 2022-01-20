import sys
import os
import pathlib

copyright = """// Copyright 2022 Unity Technologies Inc. All rights reserved.
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


def create_dir_i_files():
    rootname = os.path.basename(root)
    filepath = os.path.join(out, rootname + ".i")
    with open(filepath, 'w') as f:
        f.write(copyright + '\n')
        f.write('%module {0} \n\n'.format(rootname))

        # leaf
        if len(dirs) == 0:

            #f.write('#define {0}_API \n\n'.format(rootname.upper()))


            for file in files:
                s = pathlib.Path(file).stem
                s = s[0].upper() + s[1:]
                f.write('%include "{0}{1}.i" \n'.format(rootname, s))
        # parent
        else:
            for d in dirs:
                f.write('%include "{0}/{0}.i" \n'.format(d))


def create_header_i_files():
    if len(dirs) == 0:
        for file in files:
            s1 = pathlib.Path(file).stem
            s1 = s1[0].upper() + s1[1:]
            filename = os.path.basename(root) + s1
            filepath1 = os.path.join(out, filename + '.i')
            with open(filepath1, 'w') as f1:
                f1.write(copyright + '\n')
                f1.write("%module {0}\n\n".format(filename))
                f1.write("%{\n")

                include = os.path.join(rel, file).replace("\\", "/")
                f1.write('#include "{0}"\n'.format(include))
                f1.write("%}\n\n")
                f1.write('%include "{0}"\n'.format(include))


if __name__ == '__main__':
    # print(default_swig_i.format("test", "hohoho"))
    if len(sys.argv) == 1:
        print("Enter path of directory containing USD .h files (/include/pxr/)")

    include_pxr_dir = sys.argv[1]
    if not os.path.isdir(include_pxr_dir):
        print("Directory does not exists")

    if not os.path.basename(include_pxr_dir) == "pxr":
        print("Not USD pxr/ directory")

    include_dir = pathlib.Path(include_pxr_dir).parent
    out_dir_base = os.path.join(os.getcwd(), "Swig")

    # 1) generating .i files from /include/pxr/
    for root, dirs, files in os.walk(include_pxr_dir):
        #print("\nroot : " + str(root))
        #print("dirs : " + str(dirs))
        #print("files : " + str(files))

        # create_dir_hierarchy
        rel = os.path.relpath(root, include_dir)
        out = os.path.join(out_dir_base, rel)
        os.makedirs(out, exist_ok=True)

        create_dir_i_files()
        create_header_i_files()

    # 2) if hand-made .i files where already existing in previous versions, keep them?

    # if not exist -> copy
        # check if naming format is 'folderFile.i'
    # if exist -> check diff (ignore license)
        # ignore license, check if other SWIG directive beside %include and %module are there?


