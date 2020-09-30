# USD.NET.Unity
project(USD_NET_UNITY)

include(${CMAKE_SOURCE_DIR}/src/USD.NET/CMakeLists.txt)

set(USD_NET_UNITY_DLL USD.NET.Unity.dll)
add_custom_command(
        OUTPUT ${USD_NET_UNITY_DLL}
        COMMAND mcs /noconfig /langversion:4 /nostdlib+
        /warn:4 /optimize+ /out:${USD_NET_UNITY_DLL} /target:library /unsafe
        ${CMAKE_CURRENT_LIST_DIR}/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Geometry/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Properties/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Shading/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Shading/UnityNative/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Shading/UsdPreviewSurface/*.cs
        /reference:${USD_NET_DLL}
        /reference:${CSHARP_UNITYENGINE_LIBRARY}
        /reference:${MONO_LIB_DIR}/mscorlib.dll
        /reference:${MONO_LIB_DIR}/System.Core.dll
        /reference:${MONO_LIB_DIR}/System.dll
        DEPENDS
        ${USD_NET} # BUILD_Mono
        )
add_custom_target(
    USD_NET_UNITY
    ALL DEPENDS
    ${USD_NET_UNITY_DLL}
    )
install(
    FILES
    ${CMAKE_CURRENT_BINARY_DIR}/${USD_NET_UNITY_DLL}
    DESTINATION
    ${USD_PLUGINS_DIR}
        )