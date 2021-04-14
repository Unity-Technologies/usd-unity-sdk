project(usdnet)

add_subdirectory(generated)

set(USD_NET_DLL USD.NET.dll)
add_custom_command(
    OUTPUT ${USD_NET_DLL}
    COMMAND ${MCS_EXECUTABLE} -langversion:4
    -warn:4 -optimize+ -debug -out:${USD_NET_DLL} -target:library -unsafe
    ${SWIG_GENERATED_FILES}
    ${CMAKE_CURRENT_LIST_DIR}/collections/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/pooling/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/serialization/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/Properties/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/*.cs
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
    ${USD_PLUGINS_DIR}
    )
