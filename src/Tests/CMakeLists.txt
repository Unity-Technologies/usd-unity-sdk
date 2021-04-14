# USD.NET tests
project(USD_NET_TESTS)
set(USD_NET_TESTS_EXE Tests.exe)

add_custom_command(
        OUTPUT ${USD_NET_TESTS_EXE}
        COMMAND ${MCS_EXECUTABLE} /langversion:4
        /warn:4 /optimize+ /out:${USD_NET_TESTS_EXE} /target:exe /unsafe
        ${CMAKE_CURRENT_LIST_DIR}/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Cases/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Util/*.cs
        /reference:${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/USD.NET.dll
        DEPENDS
        ${USD_NET} ${USD_NET_UNITY}
        )
add_custom_target(
    USD_NET_TESTS
    ALL DEPENDS
    ${USD_NET_TESTS_EXE}
    )
install(
    FILES
    ${CMAKE_CURRENT_BINARY_DIR}/${USD_NET_TESTS_EXE}
    ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/USD.NET.dll
    ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/x86_64/Windows/*.dll
    DESTINATION
    ${CMAKE_BINARY_DIR}
)
# TODO: run tests
add_custom_command(TARGET USD_NET_TESTS
                   COMMENT "Run tests"
                   POST_BUILD COMMAND START ${CMAKE_BINARY_DIR}/${USD_NET_TESTS_EXE}
                   WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
)
