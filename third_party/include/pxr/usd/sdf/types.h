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
#ifndef SDF_TYPES_H
#define SDF_TYPES_H

/// \file sdf/types.h
/// Basic Sdf data types

#include "pxr/pxr.h"
#include "pxr/usd/sdf/api.h"
#include "pxr/usd/sdf/assetPath.h"
#include "pxr/usd/sdf/declareHandles.h"
#include "pxr/usd/sdf/listOp.h"
#include "pxr/usd/sdf/valueTypeName.h"

#include "pxr/base/arch/demangle.h"
#include "pxr/base/arch/inttypes.h"
#include "pxr/base/gf/half.h"
#include "pxr/base/gf/matrix2d.h"
#include "pxr/base/gf/matrix3d.h"
#include "pxr/base/gf/matrix4d.h"
#include "pxr/base/gf/quatd.h"
#include "pxr/base/gf/quatf.h"
#include "pxr/base/gf/quath.h"
#include "pxr/base/gf/vec2d.h"
#include "pxr/base/gf/vec2f.h"
#include "pxr/base/gf/vec2h.h"
#include "pxr/base/gf/vec2i.h"
#include "pxr/base/gf/vec3d.h"
#include "pxr/base/gf/vec3f.h"
#include "pxr/base/gf/vec3h.h"
#include "pxr/base/gf/vec3i.h"
#include "pxr/base/gf/vec4d.h"
#include "pxr/base/gf/vec4f.h"
#include "pxr/base/gf/vec4h.h"
#include "pxr/base/gf/vec4i.h"
#include "pxr/base/tf/enum.h"
#include "pxr/base/tf/preprocessorUtils.h"
#include "pxr/base/tf/staticTokens.h"
#include "pxr/base/tf/token.h"
#include "pxr/base/tf/type.h"
#include "pxr/base/vt/array.h"
#include "pxr/base/vt/dictionary.h"
#include "pxr/base/vt/value.h"

#include <boost/mpl/joint_view.hpp>
#include <boost/mpl/transform_view.hpp>
#include <boost/mpl/vector.hpp>
#include <boost/noncopyable.hpp>
#include <boost/preprocessor/cat.hpp>
#include <boost/preprocessor/list/fold_left.hpp>
#include <boost/preprocessor/list/for_each.hpp>
#include <boost/preprocessor/list/size.hpp>
#include <boost/preprocessor/punctuation/comma.hpp>
#include <boost/preprocessor/selection/max.hpp>
#include <boost/preprocessor/seq/for_each.hpp>
#include <boost/preprocessor/seq/seq.hpp>
#include <boost/preprocessor/seq/size.hpp>
#include <boost/preprocessor/stringize.hpp>
#include <boost/preprocessor/tuple/elem.hpp>
#include <boost/shared_ptr.hpp>
#include <iosfwd>
#include <list>
#include <map>
#include <stdint.h>
#include <string>
#include <typeinfo>
#include <vector>

PXR_NAMESPACE_OPEN_SCOPE

class SdfPath;

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
/// <li><b>SdfSpecifierDef.</b> Defines a concrete prim.
/// <li><b>SdfSpecifierOver.</b> Overrides an existing prim.
/// <li><b>SdfSpecifierClass.</b> Defines an abstract prim.
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

// USD.NET: Remove Boost PP Declarations (196-662)

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

// Write out the string representation of a block.
SDF_API std::ostream& operator<<(std::ostream&, SdfValueBlock const&); 

// A class that represents a human-readable value.  This is used for the special
// purpose of producing layers that serialize field values in alternate ways; to
// produce more human-readable output, for example.
struct SdfHumanReadableValue {
    SdfHumanReadableValue() = default;
    explicit SdfHumanReadableValue(std::string const &text) : _text(text) {}

    bool operator==(SdfHumanReadableValue const &other) const {
        return GetText() == other.GetText();
    }
    bool operator!=(SdfHumanReadableValue const &other) const {
        return !(*this == other);
    }

    std::string const &GetText() const { return _text; }
private:
    std::string _text;
};

SDF_API
std::ostream &operator<<(std::ostream &out, const SdfHumanReadableValue &hrval);

SDF_API
size_t hash_value(const SdfHumanReadableValue &hrval);

PXR_NAMESPACE_CLOSE_SCOPE

#endif // SDF_TYPES_H
