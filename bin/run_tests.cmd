@ECHO OFF
SET ORIGPATH=%PATH%
SET PATH=%PATH%;C:/Program Files/Unity/Hub/Editor/2019.4.9f1/Editor
START ../build/Tests.exe
IF NOT %ERRORLEVEL% == 0 (
  echo Tests exited with non-zero error code %ERRORLEVEL%
  EXIT /B
)
set PATH=%ORIGPATH%