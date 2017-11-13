%module UsdShadeMaterial
%{
#include "pxr/usd/usdShade/material.h"
%}

%include <std_pair.i>
%template() std::pair<UsdStagePtr,UsdEditTarget>;

%ignore UsdShadeMaterial::FindBaseMaterialPathInPrimIndex;

%include "pxr/usd/usdShade/material.h"
