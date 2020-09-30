project(USD_NET)

set(USD_NET_DLL USD.NET.dll)
set(MCS C:/Users/julien.dubuisson/scoop/apps/mono/6.10.0.104/bin/mcs)
set(MONO_LIB_DIR C:/Users/julien.dubuisson/scoop/apps/mono/6.10.0.104/lib/mono/4.5)
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
    /reference:${MONO_LIB_DIR}/mscorlib.dll
    /reference:${MONO_LIB_DIR}/System.Core.dll
    /reference:${MONO_LIB_DIR}/System.dll
    DEPENDS
    ${USD_SWIG_LIBRARY} # BUILD_Mono
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