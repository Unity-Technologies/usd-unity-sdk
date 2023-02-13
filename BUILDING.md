# Building from Source
This document is intended to help developers fully rebuild the USD Unity SDK from source, which includes:

 * Building the USD libraries from source
 * Generating SWIG interface files from Python
 * Generating C# bindings from the SWIG interface
 * Compiling the generated pure-C interface library, used by the C# bindings (UsdCs)
 * Compiling the C# USD.NET and USD.NET.Unity libraries
 * Compiling the final Unity package

The following directories are important to be aware of during the build process.

 * [/bin](/bin) - Build scripts and binding generator helpers.
 * [/src/Swig](/src/Swig) - Hand coded and generated swig inputs.
 * [/src/UsdCs](/src/UsdCs) - The USD C# bindings library, pure C API.
 * [/package](/package) - The source for the Unity package and C# USD.NET source files.
 * [/third_party](/third_party) - Code copyrighted by third parties.
 * [/cmake](/cmake) - CMake build configuration and additional find_module scripts.

## Compiling

Note that currently only Windows, OSX (Intel CPU), and Linux builds are officially supported.

### Generating Bindings

A CMake build configuration is available for all platforms to build the bindings from the usd binaries. It's available through a build script in python
and can build the native library (UsdCs), the C# library (USD.NET) and the tests.

#### Requirements

##### Cross Platforms
 * Python 3.6 (available as python3 in your system PATH, note that Python 3.8 is not supported by USD 20.08)
 * USD 20.08 with python 3.6 support (used to generate type information for the SWIG bindings)
 * USD 20.08 without python support (used to minimize runtime dependencies of the bindings)
 * CMake 3.19 (available in your system PATH)
 * Swig 3.0.12 (available in your system PATH)
 * Mono 5.1x (available in your system PATH). As of v3.0.0 of this package, USD.NET sources are included in the package folder so you only need this if you intend to build USD.NET manually.
 
 ##### Windows
 * Visual Studio 2015, 2017, or 2019
 
 ##### Linux
 * gcc 7
 
 ##### OSX
 * XCode
 
#### Building USD 

We typically use the standard build steps from the [USD repo instructions](https://github.com/PixarAnimationStudios/USD#getting-and-building-the-code). The exact command lines we are using are:
* with python support: `python3 build_scripts/build_usd.py --build-monolithic --alembic --no-imaging --no-examples --no-tutorials ../artifacts/usd-v20.08-python36/usd-v20.08`
* no python support:   `python3 build_scripts/build_usd.py --build-monolithic --alembic --no-python --no-imaging --no-examples --no-tutorials ../artifacts/usd-v20.08-python36/usd-v20.08_no_python`

**Unity developers**: USD binaries are built with each new release and pushed on Stevedore. Instead of building them, see the next section to download them automatically when building the bindings.

#### Building the C# bindings

The bindings are made of two libraries:
* UsdCs: the native USD C# bindings library (C++, SWIG generated)
* USD.NET: the C# library (serialization API)

The simplest way to build the bindings is to use the provided python build script with the corresponding component (usdcs or usdnet).
Note that USD.NET sources are now included in the package folder and will be automatically compiled by Unity. Use the `--component usdnet` argument if you are only interested in the C# bindings.

The build script expects the following folder structure:
* <path/to/artifacts>
    * usd-v20.08-python36
        * usd-v20.08
        * usd-v20.08_no_python

In a terminal, running the following command from the root of the repository will build UsdCs by default:

`python3 bin/build.py --usd_version 20.08 --library_path <path/to/artifacts> --unity_version 2021.2.8f1`

You can specify the component using the --component option. The following command will build USD.NET, using the mono compiler installed on your machine:

`python3 bin/build.py --usd_version 20.08 --library_path <path/to/artifacts> --use_custom_mono --component usdnet`

**Unity developers**: to download the USD binaries from Stevedore instead of building, use the `--download` option.

#### Bindings building steps (advanced)

**UsdCs**
1. Generate type info SWIG interface files by parsing the USD python files (bin/gen_type_info.py)
2. Generate the SWIG bindings, produces C# files and the UsdCs library
3. Decorate the SWIG callbacks in UsdCsPINVOKE.cs to allow IL2CPP support (bin/add_MonoPInvokeCallback_attribute.py)
4. Install the generated C# files in src/USD.NET/generated directory (cmake/install_usd_bindings.cmake)
5. Generate a CMakeLists.txt in src/USD.NET/generated to list the generated C# files 
6. Install the library and its runtime dependencies to package/com.unity.formats.usd/Runtime/Plugins/x86_64/[Platform]/
7. Install the USD plugins files to package/com.unity.formats.usd/Runtime/Plugins

**USD.NET**
1. Build the C# library
2. Install the USD.NET library to package/com.unity.formats.usd/Runtime/Plugins/


## Updating USD

Whenever the USD core libs are updated, the following steps are critical:

 1. Sync the USD tree to the exact release build.
 2. Run a diff against all headers in third_party and resolve changes.
 3. Generate new C# bindings (described above).

If the new libray fails to load, the most common issue is that the library
dependencies were not correctly updated. Using a tool like otool or
DependencyWalker is a great way to inspect the dynamic linkage issues.
