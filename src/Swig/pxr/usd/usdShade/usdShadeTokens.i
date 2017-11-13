%module UsdShadeTokens
%{
#include "pxr/usd/usdShade/tokens.h"
%}

#include "pxr/usd/usdShade/tokens.h"

%include "usdShadeTokens_Tokens.i"

class UsdShadeTokens {
private:
	UsdShadeTokens();
	~UsdShadeTokens();
public:
};