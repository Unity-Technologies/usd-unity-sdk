# USD Unity SDK: USD C# Bindings for Unity

This repository contains a set of libraries designed to support the use of
USD in C#/Unity. The goal of this package is to make it maximally easy to
integrate and explore Universal Scene Description.

![USD header](Images/USD_header.png)

# Getting Started

To get started, install USD via the Unity Package manager, either by using
an official repository, or by browsing for a local package while working with
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

* Windows / OSX
* Unity version: 2019.4 and up
* API Compatibility Level .NET 4.x is no longer required but still provides better performances than .NET 2
   In Edit > Project Settings > Player :   
    ![USD .NET version](Images/USD_.NET_version.png)

## Limitations

* Versions of USD **prior to 19.05** do not handle Alembic playback correctly, resulting significantly slower frame rate than authored in the Alembic cache (24x exactly). This will be fixed with the adoption of the USD 19.05 release and all assets will play back correctly, a priori.

## Samples

The USD package also includes samples to help you get started.

Use Package Manager to import the samples into your Assets folder :

![USD .NET version](Images/USD_samples_import.png)



# Features

The following is a brief listing of currently supported features:

 * Import as GameObject, Prefab, or Timeline Clip
 * USDZ Export
 * Transform Override Export
 * Timeline Playback (Vertex Streaming & Skeletal Animation)
 * Timeline Recording Track
 * Mesh Import & Export
 * Material Import & Export (USD Preview Surface or DisplayColor)
 * Unity Materials: HDRP, Standard and limited LWRP support
 * Material Export Plugins
 * Variant Selection
 * Payload Load/Unload
 * Automatic Lightmap UV Unwrapping
 * Skeletal Animation via UsdSkel
 * Scene Instancing
 * Point Instancing
 * Integration with C# Job System
 * High and Low Level Access to USD API via C#

## Importing Materials

To import materials from USD, import the USD file using the USD menu. Next, click
on the root GameObject and select either DisplayColor or Preview Surface from the
materials dropdown menu.

## Streaming Playback via Timeline

After importing a USD file with either skeletal or point cache animation, open
the Timeline window. Select the root of the USD file.

Create a playable director by clicking the "Create" button in the Timeline window.
Next, drag the root USD file into the Timeline to create a track for this object.
Finally, drag the USD file once more to add a USD clip to the track for plaback.
Scrubbing through time will now update the USD scene by streaming dat from USD.

Timeline playback is multi-threaded using the C# Job System.

## Variants, Models, & Payloads

Access to variant selection, model details, and payload state are all accessible via
the inspector on the game object at which these features were authored.

## Exporting USD files

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

### Exporting via USD menu (Legacy)

A last option to export a USD Composition is to use the legacy export via the Editor main menu :
* Select **USD > Export Selected**.

Note that this option is not recommended as it doesn't offer any export options.
