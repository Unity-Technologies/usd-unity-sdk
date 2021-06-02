%module UsdSkelSkinningQuery
%{
#include "pxr/usd/usdSkel/blendShapeQuery.h"
#include "pxr/base/vt/array.h"
%}

// TODO: Need type AnimMapperRefPtr.
%ignore UsdSkelBlendShapeQuery::GetMapper;

%include "pxr/usd/usdSkel/blendShapeQuery.h"
