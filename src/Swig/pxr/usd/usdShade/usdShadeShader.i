%module UsdShadeShader
%{
#include "pxr/usd/usdShade/shader.h"
#include "pxr/usd/usdShade/connectableApi.h"
%}

%include "pxr/usd/usdShade/shader.h"

%include "std_vector.i"
namespace std {
	%template(UsdShadeShaderVector) vector<UsdShadeShader>;
}