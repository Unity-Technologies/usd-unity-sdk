# Building from Source
This document is intended for developers intending to build USD from source.
Start by understanding the layout of the source code:

 * [/bin](/bin) - binaries and scripts for maintaining the repo (generating bindings, etc).
 * [/src](/src) - code from which all projects are generated.
 * [/src/Swig](/src/Swig) - hand coded and generated swig inputs.
 * [/src/Tests](/src/Tests) - unit tests for USD.NET and USD.NET.Unity.
 * [/src/USD.NET](/src/USD.NET) - generated USD bindings and serialization foundation.
 * [/src/USD.NET.Unity](/src/USD.NET.Unity) - Unity-specific support.
 * [/package](/package) - The source for the Unity package.
 * [/third_party](/third_party) - code copyrighted by third parties.
 * [/cmake](/cmake) - CMake build configuration.

## Compiling

Note that currently only Windows and OSX builds are officially supported.

USD.NET.sln is a Visual Studio solution which includes all projects. The
primary requirement is to setup the library and include paths for the
C component of the build (UsdCs):

 * Create a new environment variable USD_LOCATION pointing to your USD install root (contains /lib and /include)

Similarly for OSX an XCode project is provided in the src directory, called
UsdCs.xcodeproj. The OSX build will produce a .bundle file, into which all
dependent dylibs must be manually copied, as well as the USD plugInfo.json
files. See the existing bundle as an example of the correct structure.

## Generating Bindings

C# The following instructions assume a Windows build environment, though
the same process should work on OSX as well.

There are two main steps to code generation, the first is a sequence of
Python scripts which generate type-specific SWIG shims (.i files). The
second step is the SWIG code generator itself.

 * The Python step has been tested with Ptyhon 2.7 and requires the USD
   Python files to be importable via PYTHONPATH.
 * The SWIG step requires the SWIG 3.0.12 executable on your system PATH.

By setting USD_LOCATION_PYTHON to the root install directory of a USD
python build, all scripts will work without setting the system PATH or
PYTHONPATH. The variable USD_LOCATION should be set to a USD build without
python, to minimize runtime dependencies of the final plugin.

Once these two requirements are met, [build.cmd](build.cmd) can be run
from a cmd prompt from the root of the UsdBindings directory. It will
generate the additional SWIG inputs via Python, run SWIG, and then copy
the outputs into the correct locations in the source tree. If this script
fails at any step, execution will stop so the error can be observed.

The full build process is:

 1. Clone a USD repository
 2. Check out the desired version to a new branch (e.g. git checkout -b v0.8.4)
 3. Build USD with python enabled (this is required to generate C# bindings)
 4. Build USD to a different directory with python disabled (to minimize runtime dependencies)
 5. Set the environment variable USD_LOCATION_PYTHON to the path used in step (3)
 6. Set the environment variable USD_LOCATION to the path used in step (4)
 7. If upgrading USD to a newer version, diff third_party includes vs newly distributed header files.
 8. The "generated" source folder should also be deleted so it can be regenerated in the next step.
 9. Run bin\build.bat to generate Swig bindings
 10. Open USD.NET.sln in Visual Studio 2015 (only VS 2015 is currently supported)
 11. If the source was upgraded or if the "generated" folder was deleted in step (7), update this folder in the solution by removing missing files and adding new additions
 12. Build the solution
 13. Hit play to run tests
 14. Run bin\install to copy USD.NET DLLs to the Unity asset package
 15. Distribute C++ DLLs to the unity asset package. After upgrading USD, its highly recommended to use a tool like DependencyWalker (64-bit) to collect a minimal set of dependencies.
 16. If upgrading USD, run bin\diff-plugins to merge / verify USD plugin changes. Note that the library paths are intentionally different.
 17. Test the asset package
 18. Export a new Unity asset package
 19. Test the exported asset package

The following is an example of valid environment variables:

SET USD_LOCATION=C:\src\usd\builds\v19.01\monolithic_no-python\
SET USD_LOCATION_PYTHON=C:\src\usd\builds\v19.01\monolithic\

The following are the USD build commands used to generate the two build paths noted above:

python build_scripts\build_usd.py --build-monolithic --alembic --no-python --no-imaging C:\src\usd\builds\v19.01\monolithic_no-python

python build_scripts\build_usd.py --build-monolithic --alembic --openimageio C:\src\usd\builds\v19.01\usd_monolithic

## Updating USD

Whenever the USD core libs are updated, the following steps are critical:

 1. Sync the USD tree to the exact release build.
 2. Run a diff against all headers in third_party and resolve changes.
 3. Generate new C# bindings (described above).
 4. Deploy new DLLs and plugInfo.json files for each platform.

If the new libray fails to load, the most common issue is that the library
dependencies where not correctly updated. Using a tool like otool or
DependencyWalker is a great way to inspect the dynamic linkage issues.

If the new library loads, but complains about an unknown file format (e.g.
cannot open .usd files), this is likely due to plugins failing to load,
which typically is a problem with the plugInfo.json files. This can happen
if the files changed in the USD repository but were not correctly updated
in the Unity package.

## CMake Build (experimental)

On Linux, a CMake build configuration is available, which is a self-contained build of USD dependencies, and the Unity plugin.

From a build/ subdirectory inside the git repository, run `cmake ../cmake`.  Optionally, specify `-DUSD_VERSION=` with the version tag from the USD repository (currently v19.05)
Running `make intall` will build USD, Alembic, OpenEXR, TBB and boost requirements statically, and link them into `libUsdCs.so` and install the plugin into the proper location in the package structure

Optionally, specify `-DBUILD_USD_NET=TRUE` to build the USD.NET.dll (this will also build Mono, which can be quite time consuming)

Currently, the CMake configuration cannot be used to build the USD.NET.Unity.dll.


