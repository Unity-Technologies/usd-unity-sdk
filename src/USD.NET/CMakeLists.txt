project(usdnet)

# The USDNET_SRC variable doesn't seem to work well as an argument to the custom command ...
file(GLOB_RECURSE USDNET_SRC ${CMAKE_CURRENT_LIST_DIR}/generated/*.cs)

set(USD_NET_DLL USD.NET.dll)
add_custom_command(
    OUTPUT ${USD_NET_DLL}
    COMMAND ${MCS} -noconfig -langversion:4 -nostdlib+
    -warn:4 -optimize+ -debug -out:${USD_NET_DLL} -target:library -unsafe
    #${USDNET_SRC}
    ${CMAKE_CURRENT_LIST_DIR}/generated/SWIG/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/base/gf/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/base/js/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/base/plug/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/base/tf/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/base/vt/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/ar/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/kind/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/ndr/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/pcp/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/sdf/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/sdr/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usd/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdGeom/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdLux/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdRi/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdShade/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdSkel/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdUtils/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/pxr/usd/usdVol/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/std/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/usdCs/UsdCs.cs
    ${CMAKE_CURRENT_LIST_DIR}/generated/usdCs/UsdCsPINVOKE.cs
    ${CMAKE_CURRENT_LIST_DIR}/collections/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/pooling/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/serialization/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/Properties/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/*.cs
    /reference:${MONO_MSCORLIB}
    /reference:${MONO_SYSTEM_CORE_LIB}
    /reference:${MONO_SYSTEM_LIB}
    COMMAND_EXPAND_LISTS
    DEPENDS ${USD_CS}
    )
add_custom_target(
    USD_NET
    ALL DEPENDS
    ${USD_NET_DLL}
    )

file(GLOB ALLFILES ${CMAKE_CURRENT_BINARY_DIR}/USD.NET* )
install(
    FILES
    ${ALLFILES}
    DESTINATION
    ${USD_PLUGINS_DIR}
    )
