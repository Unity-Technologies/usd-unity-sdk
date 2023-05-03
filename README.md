# USD Unity SDK: USD Experimental Package for Unity

This repository contains the source code for the `com.unity.formats.usd` package. It includes a set of libraries designed to support the use of USD in C#, as well as code to import and export USD files into the editor. The goal of this package is to make it maximally easy to integrate and explore Universal Scene Description.

![Pixar's USD kitchen Set in the Unity Editor](package/com.unity.formats.usd/Documentation~/Images/USD_header.png)

*Pixar's [USD Kitchen Set](https://openusd.org/release/dl_kitchen_set.html) in the Unity Editor.*

## Documentation

For full documentation of this package, including usage, see the [package docs](package/com.unity.formats.usd/Documentation~/USD.md).

## Features

The following is a brief listing of currently supported features:

* Importing and exporting USD:
    * Import as GameObject, Prefab, or Timeline Clip
        * File formats: .usd, .usda, .usdc, .usdz
    * Export Game Objects to USD
        * File formats: .usd, .usda, .usdc, .usdz
        * Export of Transform Overrides
    * Export via Recorder package
* Composition:
    * Variant Selection
    * Payloads
        * Load All and Individual
    * Layer stacks
* Geometry:
    * UV Set
    * Vertex Colour
* Instancing:
    * Point Instancing
    * Scenegraph Instancing
* Primitive Types:
    * Meshes:
        * Arbitrary Primvars
        * Vertex Colors
    * Materials:
        * Standard Shader and Limited HDRP and URP Support
* Cameras
* Lightmaps:
    * Automatic Lightmap UV Unwrapping
* Animation:
    * Timeline Playback:
        * Skeletal Animation via USDSkel
        * Animated Meshes
    * Timeline Recording Track via Unity Recorder Package
* General:
    * High and Low Level Access to USD API via C#

## Known Limitations

We do not currently support the following:

* Geometry:
    * Multiple UV Sets
    * Importing mesh UVs for a prim without a material attached
* Composition:
    * Purposes
* Primitive Types:
    * Camera
        * We do not currently import and export all physical camera settings
    * Materials:
        * Transparency Settings (these can be set manually)
        * Some values are unassigned when importing URP
    * Lights
* Animation:
    * Blend Shapes
* General:
    * Custom prims

# License

The USD Unity SDK is licensed under the terms of the Apache
license. See [LICENSE](LICENSE) for more information.

# Contribute

See [CONTRIBUTING.md](CONTRIBUTING.md)

# Build
See [BUILDING.md](BUILDING.md)