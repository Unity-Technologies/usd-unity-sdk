# USD Unity SDK: USD C# Bindings for Unity

This repository contains a set of libraries designed to support the use of
USD in C#/Unity. The goal of this package is to make it maximally easy to
integrate and explore Universal Scene Description.

![USD header](Images/USD_header.png)

# Getting Started

To get started, install the USD package via the Unity Package manager. You can do this by either:
* Installing by name, "com.unity.formats.usd";
* Installing by Git URL;
* or browsing for a local package while working with
source.

Once the USD package is installed, a USD menu will appear, enabling you to
easily import and export USD files.

![USD menu](Images/USD_menu.png)

The USD importer works with linear color space only. To ensure colors are imported correctly,
set the project color space to "linear" in Edit > Project Settings > Player:

![USD linear](Images/USD_linear.png)

In Unity 2019, the USD importer supports importing unlimited weights per vertex. However,
to see the effect of more than 4 weights per vertex, this must be enabled in the project
settings under Edit > Project Settings > Quality:

![USD unlimited_weights](Images/USD_unlimited_weights.png)

## Requirements

* Windows / OSX (Intel only)
* Unity version: 2019.4 and up
* API Compatibility Level .NET 4.x is no longer required but still provides better performances than .NET 2
   In Edit > Project Settings > Player :
    ![USD .NET version](Images/USD_.NET_version.png)

## Limitations

* Apple Silicon architectures are not currently supported by this package. To use this package on M1 devices, you must use an x64 install of the Unity Editor.
* The USD plugin bundles are not currently code signed, so will need to be manually signed or removed from standalone player builds.

## Samples

The USD package also includes samples to help you get started.

Use Package Manager to import the samples into your Assets folder :

![USD .NET version](Images/USD_samples_import.png)

Note that Samples will not work as expected when installed from Source. In these cases, you will have to copy the Samples files into the project to use them.

# Features

The following is a brief listing of currently supported features:
 * File:
     * Import as GameObject, Prefab, or Timeline Clip
         * Formats: USD, USDA, USDC, USDZ
     * Export
         * Includes Transform Override Export
         * Formats: USD, USDA, USDC, USDZ
     * Additive Update
     * Destructive Update
     * Round-Trip

 * Geometry:
     * UV Set
     * Variant Selection
     * Vertex Colour

 * Instances:
     * Point Instancing
     * Nested Point Instancing
     * Scene Instancing

 * Primitive Types:
     * Cameras
         * Import and Export
     * Mesh & Material
         * Import and Export
         * HDRP, URP (Limited) and Standard Support
         * Payload Load & Unload
             * Load All & Individual
     * Lights
         * Automatic Lightmap UV Unwrapping

 * Animation:
     * Timeline Playback
         * Skeletal Animation
             * Via USDSkel
         * Animated Mesh
     * Timeline Recording Track
         * Via Unity Recorder Package

 * General:
    * High and Low Level Access to USD API via C#
    * Uses USD Hierarchy within Unity upon Import

## Importing USD Files

To import a USD file into Unity, use the USD context menu, and select one of the 'Import as ...' [options](#importingoptions) as required
Supported USD file formats are:
 * .USD
 * .USDA
 * .USDC
 * .USDZ

 ## Importing Options
 * Import As GameObjects:
     * Imports selected USD file as Unity GameObjects directly into the Unity Scene Hierarchy.

 * Import as Prefab:
     * Imports selected USD file as a Unity Prefab object into the Unity Project.

 * Import as Timeline Clip:
     * Imports selected USD file as a Unity Prefab containing empty Materials and the Timeline Clip.
     * If animation is recorded in the Timeline Clip, that can be used to animate other applicable Unity Objects.


## Importing Materials

To import materials from USD, firstly import the USD file using the USD menu.
Then, to get the materials to render in the scene, change the 'Import Settings' > 'Materials' in the Inspector to 'Import Preview Surface'. Finally re-import the USD file.
You can furthermore import all texture files by changing the 'Import Settings' > 'Payload Policy' in the Inspector to 'Load All' and re-import the USD file.

If upon initial import, the object is not visible, try re-importing the USD file with the above 'Import Settings'.

## Streaming Playback via Timeline

After importing a USD file with either skeletal or point cache animation...
1. Open the Timeline window.
2. Select the root of the USD file.
3. Create a playable director by clicking the "Create" button in the Timeline window.
4. Next, drag the root USD file into the Timeline to create a track for this object.
5. Finally, drag the USD file once more to add a USD clip to the track for plaback.

Scrubbing through time will now update the USD scene by streaming data from USD.
Timeline playback is multi-threaded using the C# Job System.

## Variants, Models, & Payloads

Access to variant selection, model details, and payload state are all accessible via
the inspector on the game object at which these features were authored.

Note that Payloads *are not loaded by default*.
USD files using Payloads must be reloaded after changing the Payload Policy in the Inspector to 'Load All' for them to appear in the Scene.

If upon initial import, the object is not visible, try re-importing the USD file with the Payload Policy set to 'Load All'.

## Exporting USD files

USD files can be created by exporting GameObjects selected in the Hierarchy using the USD context menu options, or using the Recorder.
Supported USD file formats are:
 * .USD
 * .USDA
 * .USDC
 * .USDZ

Our default file format when Exporting is set to .USDC.

### Exporting via Recorder

> **Prerequisite:** You need to install the [Recorder package](https://docs.unity3d.com/Packages/com.unity.recorder@latest/index.html).

To export a USD composition via Recorder, you can either use the Recorder Window :
* From the Editor main menu, select **Window > General > Recorder > Recorder Window**,
* Then click on **+ Add Recorder**,
* And select **USD Clip**.

Or add a Recorder track :
* From the Timeline window, right-click and select **UnityEditor.Recorder.Timeline > Recorder Track**,
* Then right-click on the track and **add Recorder Clip**,
* And in the **selected recorder**, choose **USD Clip**.

Using Recorder is the recommended option *when compatibility with runtime is not required*.

### Exporting via Usd Recorder Track (Legacy)

When compatibility with runtime is required (i.e for a standalone build), the recommended option is to use the USD package Recorder Track :
* From the Timeline window, right-click and select **Unity.Formats.USD > Usd Recorder Track**,
* Then right-click on the track and **add USD Recorder Clip**.

>  **Note:** This feature has no dependency to and is not based on the Recorder package.

## Unsupported
We do not currently support the following:
 * Custom Prims
 * Blend Shapes (Unstable)

# License

The USD Unity SDK is licensed under the terms of the Apache
license. See [LICENSE](LICENSE) for more information.

# Contribute
See [CONTRIBUTING.md](CONTRIBUTING.md)

# Build
See [BUILDING.md](BUILDING.md)
