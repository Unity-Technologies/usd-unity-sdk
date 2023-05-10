# Exporting USD files

USD files can be created by exporting GameObjects selected in the Hierarchy using the USD context menu options, or using the Recorder window. Supported USD file formats are:

* USDC (.usd or .usdc)
* USDA (.usda)
* USDZ (.usdz)

The default file format when exporting is .usd with USDC encoding, but changing the file ending in the save menu to .usda will export as USDA.

## Export Selected with Children

On Mac you will see multiple file format options - USDC and USDA.

On Windows the default is .usd with a USDC format, but you can change the file ending in the Save window to specify a different file ending and encoding.


## Export Transform Overrides

Modifications to transforms in USD files can be exported as overrides to the original files. 

Overrides can be exported from the USD menu with the GameObject containing the [UsdAsset](USD-in-the-Editor.md#USD-Asset) component selected, which will export transform overrides for the entire hierarchy. Alternatively, you can also export just the overrides when exporting from the Recorder window by changing the 'Override Setting' to 'Export Transform Overrides Only'.


For full asset pipeline flexibility these override files do not include a reference to the original file, but this can be added by manually adding a sublayer in the header of the resulting USDA file:


```
#usda 1.0
(
    defaultPrim = "myCube"
    endTimeCode = 0
    startTimeCode = 0
    upAxis = "Y"
    subLayers = [
        @c:/path/to/original/myCube.usda@
    ]
)


over "myCube"
{
    ...
}
```


Note: Modifications to the transform of the Root GameObject are not currently reflected in the override, as Unity assumes the root in all USD files is at the origin.

## Export Selected as USDZ
Exports the selected object(s) and all of their children into a USDZ file.


## Exporting via Recorder
Prerequisite: You need to install the [Recorder package](https://docs.unity3d.com/Packages/com.unity.recorder@latest/index.html).

To export a USD composition via Recorder, you can either use the Recorder Window:
* From the Editor main menu, select Window > General > Recorder > Recorder Window,
* Then click on + Add Recorder,
* Select USD Clip.

Or add a Recorder track:

* From the Timeline window, right-click and select UnityEditor.Recorder.Timeline > Recorder Track,
* Then right-click on the track and add Recorder Clip,
* In the selected recorder, choose USD Clip.

In the Recorder window, you can set the export option to only export the transform overrides, the same as above.

Using Recorder is the recommended option *when compatibility with runtime is not required*.

## Exporting via Usd Recorder Track (Legacy)
When compatibility with runtime is required (i.e for a standalone build), the recommended option is to use the USD package Recorder Track:
* From the Timeline window, right-click and select Unity.Formats.USD > Usd Recorder Track,
* Then right-click on the track and add USD Recorder Clip.

Note: This feature has no dependency to and is not based on the Recorder package.
