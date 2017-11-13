//
// Copyright 2016 Pixar
//
// Licensed under the Apache License, Version 2.0 (the "Apache License")
// with the following modification; you may not use this file except in
// compliance with the Apache License and the following modification to it:
// Section 6. Trademarks. is deleted and replaced with:
//
// 6. Trademarks. This License does not grant permission to use the trade
//    names, trademarks, service marks, or product names of the Licensor
//    and its affiliates, except as required to comply with Section 4(c) of
//    the License and to reproduce the content of the NOTICE file.
//
// You may obtain a copy of the Apache License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the Apache License with the above modification is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the Apache License for the specific
// language governing permissions and limitations under the Apache License.
//


/// An enum that specifies the type of an object. Objects
/// are entities that have fields and are addressable by path.
enum SdfSpecType {
  // The unknown type has a value of 0 so that SdfSpecType() is unknown.
  SdfSpecTypeUnknown = 0,

  // Real concrete types
  SdfSpecTypeAttribute,
  SdfSpecTypeConnection,
  SdfSpecTypeExpression,
  SdfSpecTypeMapper,
  SdfSpecTypeMapperArg,
  SdfSpecTypePrim,
  SdfSpecTypePseudoRoot,
  SdfSpecTypeRelationship,
  SdfSpecTypeRelationshipTarget,
  SdfSpecTypeVariant,
  SdfSpecTypeVariantSet,

  SdfNumSpecTypes
};

/// An enum that identifies the possible specifiers for an
/// SdfPrimSpec. The SdfSpecifier enum is registered as a TfEnum
/// for converting to and from <c>std::string</c>.
///
/// <b>SdfSpecifier:</b>
/// <ul>
/// <li><b>SdfSpecifierDef.</b> Defines a new concrete prim.
///        A prim with the same namespace path must not already
///        have been defined (as a def or class) by a weaker layer.
/// <li><b>SdfSpecifierOver.</b> Defines overrides for an existing prim.
/// <li><b>SdfSpecifierClass.</b> Defines a new abstract prim.
///        A prim with the same namespace path must not already
///        have been defined (as a def or class) by a weaker layer.
/// <li><b>SdfNumSpecifiers.</b> The number of specifiers.
/// </ul>
///
enum SdfSpecifier {
  SdfSpecifierDef,
  SdfSpecifierOver,
  SdfSpecifierClass,
  SdfNumSpecifiers
};

/// Returns true if the specifier defines a prim.
inline
bool
SdfIsDefiningSpecifier(SdfSpecifier spec)
{
  return (spec != SdfSpecifierOver);
}

/// An enum that defines permission levels.
///
/// Permissions control which layers may refer to or express
/// opinions about a prim.  Opinions expressed about a prim, or
/// relationships to that prim, by layers that are not allowed
/// permission to access the prim will be ignored.
///
/// <b>SdfPermission:</b>
/// <ul>
/// <li><b>SdfPermissionPublic.</b> Public prims can be referred to by
///     anything. (Available to any client.)
/// <li><b>SdfPermissionPrivate.</b> Private prims can be referred to
///     only within the local layer stack, and not across references
///     or inherits. (Not available to clients.)
/// <li><b>SdfNumPermission.</b> Internal sentinel value.
/// </ul>
///
enum SdfPermission {
  SdfPermissionPublic,
  SdfPermissionPrivate,

  SdfNumPermissions
};

/// An enum that identifies variability types for attributes.
/// Variability indicates whether the attribute may vary over time and
/// value coordinates, and if its value comes through authoring or
/// or from its owner.
///
/// <b>SdfVariability:</b>
/// <ul>
///     <li><b>SdfVariabilityVarying.</b> Varying attributes may be directly 
///            authored, animated and affected on by Actions.  They are the 
///            most flexible.
///     <li><b>SdfVariabilityUniform.</b> Uniform attributes may be authored 
///            only with non-animated values (default values).  They cannot 
///            be affected by Actions, but they can be connected to other 
///            Uniform attributes.
///     <li><b>SdfVariabilityConfig.</b> Config attributes are the same as 
///            Uniform except that a Prim can choose to alter its collection 
///            of built-in properties based on the values of its Config attributes.
///     <li><b>SdNumVariabilities.</b> Internal sentinel value.
/// </ul>
///
enum SdfVariability {
  SdfVariabilityVarying,
  SdfVariabilityUniform,
  SdfVariabilityConfig,

  SdfNumVariabilities
};


/// \class SdfValueBlock
/// A special value type that can be used to explicitly author an
/// opinion for an attribute's default value or time sample value
/// that represents having no value. Note that this is different
/// from not having a value authored.
///
/// One could author such a value in two ways.
/// 
/// \code
/// attribute->SetDefaultValue(VtValue(SdfValueBlock());
/// ...
/// layer->SetTimeSample(attribute->GetPath(), 101, VtValue(SdfValueBlock()));
/// \endcode
///
struct SdfValueBlock {
  bool operator==(const SdfValueBlock& block) const { return true; }
  bool operator!=(const SdfValueBlock& block) const { return false; }

private:
  friend inline size_t hash_value(const SdfValueBlock &block) { return 0; }
};