# Changes in usd-unitysdk for Unity

## [Unreleased] - 2020-10-27
### Changed
- Set material name in Unity to match the material name in the USD file on import.

### Bug Fixes
- Fix the scene primMap that was corrupted with invalid prim making the write fail.
- Fix an inability to refresh layers in Editor Mode
- Fixed USDPayload's "Is Loaded" field in the inspector staying at false even when the payload has been loaded.
- Fixed Transform Overrides export extension (USDU-124)
- Fix UsdAsset not saving changes made in the inspector (simple view).

## [1.0.4-preview.1] - 2020-07-24
### Changed
- Updated USD version to 20.08

## [1.0.3-preview.2] - 2020-04-24
### Changed
- Fix regression where the default usd root was ignored, creating broken hierarchies of game objects

## [1.0.3-preview.1] - 2020-04-01
### Changed
- Fix the "Slow and safe as FBX" loading mode to perform the basis change as the FBX importer (#129)
 
## [1.0.2-preview.1] - 2019-11-06
### Added
- Option to handle basis conversion "as FBX" (#129)
- Support for "Sphere" schema (#128)

### Changed
- Fix the "Decompose" function to handle matrices with very small determinant (#130, #142)
- Allow exporting transparency from Standard shader (#131)
- Fix the "ImportMesh" sample to works in edit mode (#128)
- Handle skinned primitive that inherits skeleton data (joint indices and weight) from parent prim but don't define those themselves (#128)


## [1.0.1-preview.1] - 2019-06-30
### Changes

New feature :
- Linux support

Bug fixes :
- NullReferenceException on USD export (#67)
- Path of .usd file does not match textures in the archive (#88)
- Build error when sample is imported
- Timeline: Playback sometimes spews errors (#93)
- Skeleton bindings below the root are not discovered (#92)
- Normals update after opening standard shader (#91)
- UsdSkel export fails when rig root is the model root (#89)
- VariantSets on the root UsdAsset results in errors (#77)
- Bones do not match bindpose (#98)
- Standard shader "pops" when opening the inspector (#102)
- UsdIo: Do not recurse on null objects (#118)

## [1.0.0-preview.4] - 2019-03-15
### Changes
- Update readme images

## [1.0.0-preview.3] - 2019-03-15
### Changes
- CI integration
- Samples Fixes

## [1.0.0-preview.1] - 2019-02-20
### Changes
- Initial creation of the package
