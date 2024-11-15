
>[!IMPORTANT]
> # Update - November 15 2024
> The `com.unity.formats.usd` package is now deprecated and will no longer be supported or updated. 
>
> Please use the supported USD packages that are listed in the table below. 
> These packages can be installed [by name](https://docs.unity3d.com/2023.3/Documentation/Manual/upm-ui-quick.html) via Unity's [Package Manager](https://docs.unity3d.com/2023.3/Documentation/Manual/Packages.html).
>
> | Package name | What it does | Further Details |
> | :--- | :--- | :--- |
> | **com.unity.importer.usd** | USD Import | [manual](https://docs.unity3d.com/Packages/com.unity.importer.usd@1.0/manual/index.html) |
> | **com.unity.exporter.usd** | USD Export | [manual](https://docs.unity3d.com/Packages/com.unity.exporter.usd@1.0/manual/index.html) |
> | **com.unity.usd.core**<sup>1</sup> | USD C# SDK | [manual](https://docs.unity3d.com/Packages/com.unity.usd.core@1.0/manual/index.html) |
>
> Please note that **com.unity.usd.core** is installed automatically when either the importer or exporter are installed. You only need to install this by name if you are not using either import or export.



# USD Unity SDK: USD Experimental Package for Unity

This repository contains the source code for the `com.unity.formats.usd` package. It includes a set of libraries designed to support the use of USD in C#, as well as code to import and export USD files into the editor. The goal of this package is to make it maximally easy to integrate and explore Universal Scene Description.

![Animal Logic's ALab USD Sample in the Unity Editor](package/com.unity.formats.usd/Documentation~/Images/USD_header.png)

*Animal Logic's [ALab USD Sample](https://animallogic.com/alab/) in the Unity Editor.*

## Documentation

For full documentation of this package, including usage, see the [package docs](package/com.unity.formats.usd/Documentation~/index.md).

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

* Apple Silicon (MacOS users must use Intel Editor)
* Geometry:
    * Multiple UV Sets
    * Importing mesh UVs for a prim without a material attached
* Composition:
    * Purposes
* Primitive Types:
    * Camera
        * We do not currently import and export all physical camera settings
    * Materials:
        * Transparency Settings eg Alpha Clipping, Double Sided are not imported for HDRP and Built in (these can be set manually)
        * Imported maps override imported single values, eg presence of ColorMap will stop single Color being imported, same for Smoothness/ Roughness.
    * Lights
* Animation:
    * Blend Shapes
* General:
    * Custom prims

Due to conflicting USD plugins, this package may have unexpected errors when installed side-by-side with NVidia's Omniverse Connector package.

# License

The USD Unity SDK is licensed under the terms of the Apache
license. See [LICENSE](LICENSE) for more information.

# Contribute

See [CONTRIBUTING.md](CONTRIBUTING.md)

# Build
See [BUILDING.md](BUILDING.md)
