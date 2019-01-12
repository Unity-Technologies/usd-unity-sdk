mkdir inst-dbg

copy /y "src\USD.NET.Unity\bin\x64\Debug\USD.NET.dll" "inst-dbg\"
copy /y "src\USD.NET.Unity\bin\x64\Debug\USD.NET.pdb" "inst-dbg\"
copy /y "src\USD.NET.Unity\bin\x64\Debug\USD.NET.XML" "inst-dbg\"
copy /y "src\USD.NET.Unity\bin\x64\Debug\USD.NET.Unity.dll" "inst-dbg\"
copy /y "src\USD.NET.Unity\bin\x64\Debug\USD.NET.Unity.pdb" "inst-dbg\"
copy /y "src\USD.NET.Unity\bin\x64\Debug\USD.NET.Unity.XML" "inst-dbg\"
copy /y "src\UsdCs\x64\Debug\UsdCs.dll" "inst-dbg\"
copy /y "src\UsdCs\x64\Debug\UsdCs.pdb" "inst-dbg\"

xcopy /Y "inst-dbg\USD.NET.*" "unity-assetpackage\Assets\UsdUnitySdk\Plugins\"
xcopy /Y "inst-dbg\UsdCs.*" "unity-assetpackage\Assets\UsdUnitySdk\Plugins\x86_64\"
