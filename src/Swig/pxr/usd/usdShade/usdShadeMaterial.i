%module UsdShadeMaterial
%{
#include "pxr/usd/usdShade/material.h"
%}

%include <std_pair.i>
%template() std::pair<UsdStagePtr,UsdEditTarget>;

%ignore UsdShadeMaterial::FindBaseMaterialPathInPrimIndex;

%ignore UsdShadeMaterial::CreateMaterialBindSubset;

//%include "pxr/usd/usdShade/material.h"

%include "third_party/include/pxr/usd/usdShade/material.h"