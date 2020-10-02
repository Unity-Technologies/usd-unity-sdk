# USD.NET.Unity
project(USD_NET_UNITY)

set(USD_NET_UNITY_DLL USD.NET.Unity.dll)
add_custom_command(
        OUTPUT ${USD_NET_UNITY_DLL}
        COMMAND ${MCS} /noconfig /langversion:4 /nostdlib+
        /warn:4 /optimize+ /out:${USD_NET_UNITY_DLL} /target:library /unsafe
        ${CMAKE_CURRENT_LIST_DIR}/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Geometry/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Properties/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Shading/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Shading/UnityNative/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Shading/UsdPreviewSurface/*.cs
        /reference:${USD_NET_BINARY_DIR}/USD.NET.dll
        /reference:${CSHARP_UNITYENGINE_LIBRARY}
        /reference:${MONO_MSCORLIB}
        /reference:${MONO_SYSTEM_CORE_LIB}
        /reference:${MONO_SYSTEM_LIB}
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