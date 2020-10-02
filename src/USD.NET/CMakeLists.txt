project(USD_NET)

find_package(Unity REQUIRED)
set(USD_NET_DLL USD.NET.dll)
add_custom_command(
    OUTPUT ${USD_NET_DLL}
    COMMAND ${MCS} /noconfig /langversion:4 /nostdlib+
    /warn:4 /optimize+ /out:${USD_NET_DLL} /target:library /unsafe
    ${CMAKE_CURRENT_LIST_DIR}/collections/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/pooling/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/serialization/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/Properties/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/AotAttributes.cs
    ${CMAKE_CURRENT_LIST_DIR}/DiagnosticHandler.cs
    ${CMAKE_SOURCE_DIR}/cmake/generated/*.cs
    /reference:${MONO_MSCORLIB}
    /reference:${MONO_SYSTEM_CORE_LIB}
    /reference:${MONO_SYSTEM_LIB}
    DEPENDS
    ${USD_SWIG_LIBRARY}
    )
add_custom_target(
    USD_NET
    ALL DEPENDS
    ${USD_NET_DLL}
    )
install(
    FILES
    ${CMAKE_CURRENT_BINARY_DIR}/${USD_NET_DLL}
    DESTINATION
    ${USD_PLUGINS_DIR}
    )