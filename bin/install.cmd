mkdir inst

copy /y "src\USD.NET.Unity\bin\x64\Release\USD.NET.dll" "inst\"
copy /y "src\USD.NET.Unity\bin\x64\Release\USD.NET.pdb" "inst\"
copy /y "src\USD.NET.Unity\bin\x64\Release\USD.NET.XML" "inst\"
copy /y "src\USD.NET.Unity\bin\x64\Release\USD.NET.Unity.dll" "inst\"
copy /y "src\USD.NET.Unity\bin\x64\Release\USD.NET.Unity.pdb" "inst\"
copy /y "src\USD.NET.Unity\bin\x64\Release\USD.NET.Unity.XML" "inst\"
copy /y "src\UsdCs\x64\Release\UsdCs.dll" "inst\"
copy /y "src\UsdCs\x64\Release\UsdCs.pdb" "inst\"

xcopy /Y "inst\USD.NET.*" "package\com.unity.formats.usd\Runtime\Plugins\"
xcopy /Y "inst\UsdCs.*" "package\com.unity.formats.usd\Runtime\Plugins\x86_64\"
