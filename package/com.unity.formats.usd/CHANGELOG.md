# Changes in usd-unity-sdk for Unity

## [3.0.0-exp.5] - 2023-10-12
### Features
- Added basic URP Import support.

### Bug Fixes
- Fixed export for Prefabs from the Project Window.
- Fixed export of URP base and occlusion maps for Lit shader.
- Fixed USD component removal workflows for Prefabs.
- Fixed transparency import for URP only.
- Fixed exports to apply UsdShadeMaterialBindingAPI/UsdSkelBindingAPI APIs where appropriate.
- Fixed exports to include metersPerUnit stage-level metadata.

### Changed
- Renamed USD.md in the Documentation folder to index.md to match documentation generation standards for landing pages.
- Updated link in README.md to point to newly named index.md

## [3.0.0-exp.4] - 2023-06-21
### Features
- Added EditorAnalytics to track internally which features are most used.

### Bug Fixes
- Fixed the Samples Assembly Definitions compile errors.

### Changed
- Updated AppCollector YAML file to reflect latest info.
- We've removed the USD logo in a few places until we have a formal agreement in place to use it.

## [3.0.0-exp.3] - 2023-06-20
### Features
- The USD Recorder now has an option to export transform overrides.

### Bug Fixes
- "Export Transform Override" now properly exports modified transforms only.
- Fixed a bug where importing materials exported from USD version >= 21.11 would fail.
- Fixed loading of meshes with arbitrary primvars.
- Fixed regression in animated mesh properties.
- Fixed timeout when importing complex meshes.
- Fixed a bug on export where SkelRoot and Skeleton Prim were missing class identifiers.
- Fixed 'ExportSelectedWithChildren' on Mac.
- Fixed texture mapping for exported meshes.
- Fixed  a bug causing PointInstances to be duplicated.
- Fixed regression in JointsMatch().
- Fixed scene not being marked dirty after changing load payload flag.
- Fixed exported Prims not having a type and fixed export for objects with same name.
- Fixed a bug causing an ApplicationException "all sample times in times and baseTime must either all be numeric or all be default" to be thrown when refreshing a PointInstancer.
- Fixed an import bug causing instanced primitives not to be sanitized to fit Unity formats, including converting basis, triangulating and unwinding meshes, and unrolling primvars.

### Changed
- GC allocs reduced by half for Scene.GetAttributeAtPath and Scene.GetRelationshipAtPath.
- Disabled plugins on unsupported platforms.
- Optimized triangulation to reduce the chance of a "Timed out while waiting for thread" error on importing a complex mesh.
- Optimized PointInstance importing.

## [3.0.0-exp.2] - 2021-09-29
### Features
- All interpolation types are now properly supported for Mesh standard attributes (normals, tangents, uvs, colors).

### Bug Fixes
- Fixed the import of facevarying UVs which showed seams on Meshes.
- Fixed an import bug causing abstract primitives to be loaded as Game Objects.
- Fixed the broken Alembic Import.
- Fixed a crash caused by writing to an invalid USD primitive.

## [3.0.0-exp.1] - 2021-06-15
### Features
- New Import/Export API. See the ImportHelpers and ExportHelpers class (#237).
- Added Integration with Recorder (compatible with Recorder 2.2.0 or newer) (#243).

### Changed
- Linux support temporarily disabled (#255).
- USD.NET API is now in the package (#249).
- USD.NET.Unity API is now in the package (#240).
- Building USD.NET can now be done with a standalone Mono (#241).
- Building the bindings is now done through CMake on all platforms. See BUILDING.md (#235).

### Bug Fixes
- Fixed hardcoded package name in InitUSD and BuildPostProcessor (#236).
- Fixed a BuildPostProcess bug caused by permissions issues in OSX (#245).
- Fixed an IL2CPP build bug caused by the inclusion of USD codegen directory (#245).
- Fixed UsdAsset not properly refreshing the asset within the Prefab stage (#246).

## [2.0.0-exp.1] - 2020-12-21
### Features
- Updated USD version to v20.08
- Set material name in Unity to match the material name in the USD file on import (#174).
- Shader exporter: added PBR export support to standard shader (#191, #206).
- Shader exporter: added support for in-memory and render texture export (#125).
- Shader exporter: added support for texture wrap modes (#125).
- Added USDZ export from recorder clip.
- Added Scope import as XForm (#154).

### Bug Fixes
- Fixed a crash happening when calling Scene.Read on non existing prims (#158).
- Fixed an inability to refresh layers in Editor Mode (#155).
- Fixed USDPayload's "Is Loaded" field in the inspector staying at false even when the payload has been loaded.
- Fixed Transform Overrides export extension (USDU-124)
- Fixed UsdAsset not saving changes made in the inspector (simple view).
- Fixed redundant Timesamples created every frame for every prims at export.
- Fixed shader import for emissive color & light baking.
- Fixed wrong texture paths when exporting from packages (#125).
- Fixed prefabs being reset when entering Play mode (#101).
- Fixed export framerate to frames (60FPS) for more stability and improved compatibility with ArQuickLook (#170)

Special thanks to Felix Herbst for his awesome contributions on the materials import/export and USDZ exporters.

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
