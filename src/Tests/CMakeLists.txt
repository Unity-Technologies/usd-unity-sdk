# USD.NET tests
project(USD_NET_TESTS)
set(USD_NET_TESTS_EXE Tests.exe)
    
add_custom_command(
        OUTPUT ${USD_NET_TESTS_EXE}
        COMMAND mcs /noconfig /langversion:4 /nostdlib+
        /warn:4 /optimize+ /out:${USD_NET_TESTS_EXE} /target:exe /unsafe
        ${CMAKE_CURRENT_LIST_DIR}/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Cases/*.cs
        ${CMAKE_CURRENT_LIST_DIR}/Util/*.cs
        /reference:${USD_NET_BINARY_DIR}/USD.NET.dll
        /reference:${USD_NET_UNITY_BINARY_DIR}/USD.NET.Unity.dll
        /reference:${CSHARP_UNITYENGINE_LIBRARY}
        /reference:${MONO_LIB_DIR}/mscorlib.dll
        /reference:${MONO_LIB_DIR}/System.Core.dll
        /reference:${MONO_LIB_DIR}/System.dll
        DEPENDS
        ${USD_NET_UNITY} # BUILD_Mono
        )
add_custom_target(
    USD_NET_TESTS
    ALL DEPENDS
    ${USD_NET_TESTS_EXE}
    )
install(
    FILES
    ${CMAKE_CURRENT_BINARY_DIR}/${USD_NET_TESTS_EXE} ${CSHARP_UNITYENGINE_LIBRARY} ${USD_NET_BINARY_DIR}/USD.NET.dll ${USD_NET_UNITY_BINARY_DIR}/USD.NET.Unity.dll
    DESTINATION
    ${CMAKE_BINARY_DIR}
)
# TODO: run tests
#add_custom_command(TARGET usd_net_tests
#                   COMMENT "Run tests"
#                   POST_BUILD COMMAND ${CMAKE_BINARY_DIR}/../bin/run_tests.cmd
#                   WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
#)