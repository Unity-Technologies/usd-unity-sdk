 # USD.NET tests
 project(USD_NET_TESTS)
    set(usd_net_tests_exe Tests.exe)
    add_custom_command(
            OUTPUT ${usd_net_tests_exe}
            COMMAND mcs /noconfig /langversion:4 /nostdlib+
            /warn:4 /optimize+ /out:${usd_net_tests_exe} /target:exe /unsafe
            ${CMAKE_SOURCE_DIR}/src/Tests/*.cs
            ${CMAKE_SOURCE_DIR}/src/Tests/Cases/*.cs
            ${CMAKE_SOURCE_DIR}/src/Tests/Util/*.cs
            /reference:${usd_net_dll}
            /reference:${usd_net_unity_dll}
            #/reference:${usd_swig_library}
            /reference:${CSHARP_UNITYENGINE_LIBRARY}
            /reference:${mono_lib_dir}/mscorlib.dll
            /reference:${mono_lib_dir}/System.Core.dll
            /reference:${mono_lib_dir}/System.dll
            DEPENDS
            ${usd_net_unity} # BUILD_Mono
            )
    add_custom_target(
        usd_net_tests
        ALL DEPENDS
        ${usd_net_tests_exe}
        )
    # TODO: run tests
    #add_custom_command(TARGET usd_net_tests
    #                   COMMENT "Run tests"
    #                   POST_BUILD COMMAND ${CMAKE_BINARY_DIR}/../bin/run_tests.cmd
    #                   WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
    #)