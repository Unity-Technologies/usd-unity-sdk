project(usdnet)

SET(USD_NET_SRC ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Dependencies/USD.NET)
add_subdirectory(generated)

set(USD_NET_DLL USD.NET.dll)
add_custom_command(
    OUTPUT ${USD_NET_DLL}
    COMMAND ${MCS} -noconfig -langversion:4 -nostdlib+
    -warn:4 -optimize+ -debug -out:${USD_NET_DLL} -target:library -unsafe
    ${SWIG_GENERATED_FILES}
    ${USD_NET_SRC}/collections/*.cs
    ${USD_NET_SRC}/pooling/*.cs
    ${USD_NET_SRC}/serialization/*.cs
    ${USD_NET_SRC}/Properties/*.cs
    ${USD_NET_SRC}/*.cs
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

install(
    FILES
    ${CMAKE_CURRENT_BINARY_DIR}/USD.NET.dll
    DESTINATION
    ${CMAKE_CURRENT_LIST_DIR}
    )
