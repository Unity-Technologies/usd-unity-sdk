@ECHO OFF
SET ORIGPATH=%PATH%
SET PXR_USD_LOCATION=D:/libs/pixar/usd-v20.08_no_python/lib
SET PATH=%PATH%;C:/Program Files/Unity/Hub/Editor/2019.4.9f1/Editor;%PXR_USD_LOCATION%
START ../build/Tests.exe ^> tests.log
IF NOT %ERRORLEVEL% == 0 (
  echo Tests exited with non-zero error code %ERRORLEVEL%
  EXIT /B
)
set PATH=%ORIGPATH%