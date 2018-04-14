@ECHO OFF
SET ORIGPATH=%PATH%

ECHO Generating type info from Python...
ECHO Switching PATH to non-python build USD_LOCATION_PYTHON
ECHO %USD_LOCATION_PYTHON%
SET PATH=%USD_LOCATION_PYTHON%\lib;%USD_LOCATION_PYTHON%\bin;%ORIGPATH%

@python .\src\Swig\scripts\gen.py
IF NOT %ERRORLEVEL% == 0 (
  EXIT /B
)

ECHO Switching PATH to non-python build USD_LOCATION
ECHO %USD_LOCATION%
SET PATH=%USD_LOCATION%\lib;%USD_LOCATION%\bin;%ORIGPATH%

ECHO.
ECHO Running SWIG code generation...
swig.exe -w401 -w516 -w503 -namespace pxr -I%USD_LOCATION%\include -I.\src\UsdCs\ -I.\ -c++ -csharp -outdir .\src\USD.NET -o .\src\UsdCs\usdCs_wrap.cpp .\src\Swig\usdCs.i
IF NOT %ERRORLEVEL% == 0 (
  EXIT /B
)

ECHO.
ECHO Copying files...

copy .\src\Swig\usdDefines.h   .\src\UsdCs

md src\USD.NET\generated\UsdCs 2> nul
move src\USD.NET\TaskCallback.cs        .\src\USD.NET\generated\UsdCs\
move src\USD.NET\TaskCallbackVector.cs  .\src\USD.NET\generated\UsdCs\
move src\USD.NET\DiagnosticHandler.cs   .\src\USD.NET\generated\UsdCs\

md src\USD.NET\generated\pxr\base\tf 2> nul
md src\USD.NET\generated\pxr\base\js 2> nul
md src\USD.NET\generated\pxr\base\plug 2> nul
md src\USD.NET\generated\pxr\base\gf 2> nul
md src\USD.NET\generated\pxr\base\vt 2> nul
move src\USD.NET\Tf*.cs        .\src\USD.NET\generated\pxr\base\tf
move src\USD.NET\Js*.cs        .\src\USD.NET\generated\pxr\base\js
move src\USD.NET\Plug*.cs      .\src\USD.NET\generated\pxr\base\plug
move src\USD.NET\Gf*.cs        .\src\USD.NET\generated\pxr\base\gf
move src\USD.NET\Vt*.cs        .\src\USD.NET\generated\pxr\base\vt

md src\USD.NET\generated\pxr\usd\ar 2> nul
md src\USD.NET\generated\pxr\usd\sdf 2> nul
md src\USD.NET\generated\pxr\usd\usd 2> nul
md src\USD.NET\generated\pxr\usd\usdGeom 2> nul
md src\USD.NET\generated\pxr\usd\usdShade 2> nul
move src\USD.NET\Ar*.cs        .\src\USD.NET\generated\pxr\usd\ar
move src\USD.NET\Sdf*.cs       .\src\USD.NET\generated\pxr\usd\sdf
move src\USD.NET\UsdCs*.cs     .\src\USD.NET\generated\usdCs
move src\USD.NET\UsdGeom*.cs   .\src\USD.NET\generated\pxr\usd\usdGeom
move src\USD.NET\UsdShade*.cs  .\src\USD.NET\generated\pxr\usd\usdShade
move src\USD.NET\Usd*.cs       .\src\USD.NET\generated\pxr\usd\usd

md src\USD.NET\generated\SWIG 2> nul
move src\USD.NET\SWIGTYPE_*.cs .\src\USD.NET\generated\SWIG

md src\USD.NET\generated\std 2> nul
move src\USD.NET\Std*.cs       .\src\USD.NET\generated\std
