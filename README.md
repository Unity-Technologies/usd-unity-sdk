# USD Unity SDK: USD C# Bindings for Unity

This repository contains a set of libraries designed to support the use of
USD in C#/Unity. The primary goal is to make it maximally easy to integrate USD
using native Unity & C# data types with a familliar serialization paradigm and
little prior knowledge of USD.

In addition to the high-level API, the raw USD API has been translated as
faithfully as possible and is exposed via the convenient API, following Kay's
adage: "Simple things should be simple, complex things should be possible."

## Using the Convenience API

The convenience API is accessed via USD.NET.Scene object. The general
pattern of data access is to define a C# class to represent your data using
native C# and Unity data types. Once a data structure has been defined,
serialization works by calling Scene.Read and Scene.Write as follows:

    // Initialize native Unity data type maps.
    USD.NET.Unity.UnityTypeBindings.RegisterTypes();
    
    // Declare the data structure.
    class MyCustomData : USD.NET.SampleBase {
      public string aString;
      public int[] anArrayOfInts;
      public UnityEngine.Bounds aBoundingBox;
    }
  
    // Populate Values.
    var value = new MyCustomData();  
    value.aString = "Foo";
    value.anArrayOfInts = new int[] { 1, 2, 3, 4 };
    value.aBoundingBox = new UnityEngine.Bounds();
    
    // Write the value.
    var scene = USD.NET.Scene.Create("C:/path/to/sceneFile.usd");
    scene.Write("/scenegraph/path/to/data", value); 
    scene.Close();
    
    // Read the value.
    value = new MyCustomData();
    scene = USD.NET.Scene.Open("C:/path/to/sceneFile.usd");
    scene.Read("/scenegraph/path/to/data", value); 
    scene.Close();

The use of USD.NET.SampleBase is required and provides the underlying
support for reflection based serialization. In addition to creating your own
Sample types, some core USD types have been provided for convenience:

 * [XformSample](/src/USD.NET.Unity/XformSample.cs) - Equivalent of UsdGeomXform,
 provides support for writing a Matrix4x4 as a USD transform.
 
 * [MeshSample](/src/USD.NET.Unity/MeshSample.cs) - Equivalent of UsdGeomMesh and
 maps directly onto UnityEngine.Mesh.
 
 * [MeshSampleBase](/src/USD.NET.Unity/MeshSampleBase.cs) - Exposes only the 
 properties required to populate a UnityEngine.Mesh. This sample type can be used
 to improve mesh I/O performance for read-only use cases.

While a reflection based API may seem ineffecient, it has been implemented
carefully to maximize performance and to minimize main-thread stalls. The
example above is simple, but can be extended to support asynchronous I/O
and pooled memory access, in less than 10 additional lines of code.

## Using the Raw USD API

The raw USD API has been exposed as close to the C++ API as possible,
for example, the C++ pxr.UsdStage class is exposed as pxr.UsdStage. Using
this API directly requires understanding the C++ Usd library and only the
native C++ data types are supported.

To use a combination of the convenience API and the low level API, a reference
to the current UsdStage can be obtained via USD.NET.Scene.Stage.

## Source Map

 * [/bin](/bin) - binaries and scripts for maintaining the repo (generating bindings, etc).
 * [/src](/src) - code from which all projects are generated.
 * [/src/Swig](/src/Swig) - hand coded and generated swig inputs.
 * [/src/Tests](/src/Tests) - unit tests for USD.NET and USD.NET.Unity.
 * [/src/USD.NET](/src/USD.NET) - generated USD bindings and serialization foundation.
 * [/src/USD.NET.Unity](/src/USD.NET.Unity) - Unity-specific support.
 * [/unity-assetpackage](/usd-unity-sdk-assetpackage) - A source project for generating the Unity asset package.
 * [/third_party](/third_party) - code copyrighted by third parties.
 

## Compiling

Note that currently only Windows builds are officially supported.

USD.NET is a Visual Studio solution which includes all projects. The
primary requirement is to setup the library and include paths for the
C component of the build (UsdCs):

 * Create a new environment variable USD_LOCATION pointing to your USD install root (contains /lib and /include)

## Generating Bindings

There are two steps to code generation, the first is a sequence of
Python scripts which generate type-specific SWIG shims (.i files). The
second step is the SWIG code generator itself.

 * The Python step has been tested with Ptyhon 2.7 and requires the USD
   Python files to be importable via PYTHONPATH.
 * The SWIG step requires the SWIG 3.0.12 executable on your system PATH.

By setting USD_LOCATION_PYTHON to the root install directory of a USD
python build, all scripts will work without setting the system PATH or
PYTHONPATH.

Once these two requirements are met, [build.cmd](build.cmd) can be run
from a cmd prompt from the root of the UsdBindings directory. It will
generate the additional SWIG inputs via Python, run SWIG, and then copy
the outputs into the correct locations in the source tree. If this script
fails at any step, execution will stop so the error can be observed.

## License

UsdBindings is licensed under the terms of the Apache
license. See [LICENSE](LICENSE) for more information.

This is not an official Google product.
