cmake_minimum_required(VERSION 3.12)

project(usd-unity-sdk LANGUAGES CXX)
set(CMAKE_CXX_STANDARD 14)

set(dep_path ${CMAKE_BINARY_DIR}/_deps)


find_package(USD REQUIRED)
set(BUILD_USD_NET TRUE CACHE BOOL "Build mono and USD.NET.dll")

if (BUILD_USD_NET)
    find_package(Unity REQUIRED)
#    find_package(MONO 5.20.1.19 REQUIRED)
#    if (NOT DEFINED CSHARP_MONO_FOUND})
#        include(ExternalProject)
#        ExternalProject_Add(
#            BUILD_Mono
#            GIT_REPOSITORY https://github.com/mono/mono.git
#            GIT_TAG "mono-5.20.1.19"
#            GIT_SHALLOW TRUE
#            GIT_PROGRESS TRUE
#            CMAKE_GENERATOR "Visual Studio 15 2017"
#            CONFIGURE_COMMAND ""
#            BUILD_IN_SOURCE TRUE
#            BUILD_COMMAND msbuild msvc\\mono.sln /p:Platform=x64 /p:Configuration=Release /p:MONO_TARGET_GC=sgen /p:MONO_BUILD_DIR_PREFIX=${dep_path} # ./autogen.sh --prefix=${dep_path}
#            INSTALL_COMMAND ""
#        )
#    endif ()
endif ()

find_package(SWIG 3.0.12 REQUIRED)
include(${SWIG_USE_FILE})

# Generate type info from python
add_custom_target(usd_type_info ALL
    COMMAND
        ${CMAKE_SOURCE_DIR}/../bin/gen_type_info.cmd
    COMMENT "Generating type info from USD python"
)
        
if (WIN32)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif ()

include_directories(
    ${USD_INCLUDE_DIR}
    ${CMAKE_SOURCE_DIR}/../src/UsdCs
    ${CMAKE_SOURCE_DIR}/../
    )

link_directories(${USD_LIBRARY_DIR})

set(usd_swig_library UsdCs)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_source_files_properties(
        ${CMAKE_SOURCE_DIR}/../src/Swig/usdCs.i
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503;-DSWIGWORDSIZE64"
        # DEPENDS BUILD_USD
        )
else ()
    set_source_files_properties(
        ${CMAKE_SOURCE_DIR}/../src/Swig/usdCs.i
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503"
        # DEPENDS BUILD_USD
        )
endif ()

set(swig_generated_filedir ${CMAKE_SOURCE_DIR}/generated)
swig_add_library(
    ${usd_swig_library}
    TYPE MODULE
    LANGUAGE csharp
    OUTPUT_DIR ${swig_generated_filedir}
    SOURCES
    ${CMAKE_SOURCE_DIR}/../src/Swig/usdCs.i
    ${CMAKE_SOURCE_DIR}/../src/UsdCs/diagnosticHandler.cpp
    ${CMAKE_SOURCE_DIR}/../src/UsdCs/dll_main.cpp
    )

set(usd_package_directory ${CMAKE_SOURCE_DIR}/../package/com.unity.formats.usd)
set(usd_plugins_directory ${usd_package_directory}/Runtime/Plugins)

set(swig_output_dir ${usd_plugins_directory}/x86_64)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(swig_output_dir ${swig_output_dir}/MacOS)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(swig_output_dir ${swig_output_dir}/Linux)
else ()
    set(swig_output_dir ${swig_output_dir}/Windows)
endif ()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        CXX_STANDARD_REQUIRED 14
        BUNDLE TRUE
        )
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        PREFIX lib
        CXX_STANDARD_REQUIRED 14
        )
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
     set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        CXX_STANDARD_REQUIRED 14
        )
endif ()


swig_link_libraries(
    ${usd_swig_library}
    # Link against USD monolithic
    -Wl,--whole-archive usd_ms -Wl,--no-whole-archive
    Alembic
    Iex-2_2
    IexMath-2_2
    Imath-2_2
    IlmThread-2_2
    IlmImf-2_2
    Half
    tbb
    )

add_dependencies(
    ${usd_swig_library}
    usd_type_info
    )

if(BUILD_USD_NET)
    MESSAGE("BUILD USD NEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEET")
    set(usd_net_dll USD.NET.dll)
    set(mcs C:/Users/julien.dubuisson/scoop/apps/mono/6.10.0.104/bin/mcs)
    set(mono_lib_dir C:/Users/julien.dubuisson/scoop/apps/mono/6.10.0.104/lib/mono/4.5)
    add_custom_command(
        OUTPUT ${usd_net_dll}
        COMMAND ${mcs} /noconfig /langversion:4 /nostdlib+
        /warn:4 /optimize+ /out:${usd_net_dll} /target:library /unsafe
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/collections/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/pooling/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/serialization/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/Properties/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/AotAttributes.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/DiagnosticHandler.cs
        ${CMAKE_SOURCE_DIR}/generated/*.cs
        /reference:${mono_lib_dir}/mscorlib.dll
        /reference:${mono_lib_dir}/System.Core.dll
        /reference:${mono_lib_dir}/System.dll
        DEPENDS
        ${usd_swig_library} # BUILD_Mono
        )

    add_custom_target(
        usd_net
        ALL DEPENDS
        ${usd_net_dll}
        )

    install(
        FILES
        ${CMAKE_BINARY_DIR}/${usd_net_dll}
        DESTINATION
        ${usd_plugins_directory}
        )
        
    # USD.NET.Unity
    set(usd_net_unity_dll USD.NET.Unity.dll)
        add_custom_command(
                OUTPUT ${usd_net_unity_dll}
                COMMAND mcs /noconfig /langversion:4 /nostdlib+
                /warn:4 /optimize+ /out:${usd_net_unity_dll} /target:library /unsafe
                ${CMAKE_SOURCE_DIR}/../src/USD.NET.Unity/*.cs
                ${CMAKE_SOURCE_DIR}/../src/USD.NET.Unity/Geometry/*.cs
                ${CMAKE_SOURCE_DIR}/../src/USD.NET.Unity/Properties/*.cs
                ${CMAKE_SOURCE_DIR}/../src/USD.NET.Unity/Shading/*.cs
                ${CMAKE_SOURCE_DIR}/../src/USD.NET.Unity/Shading/UnityNative/*.cs
                ${CMAKE_SOURCE_DIR}/../src/USD.NET.Unity/Shading/UsdPreviewSurface/*.cs
                /reference:${usd_net_dll}
                /reference:${CSHARP_UNITYENGINE_LIBRARY}
                /reference:${mono_lib_dir}/mscorlib.dll
                /reference:${mono_lib_dir}/System.Core.dll
                /reference:${mono_lib_dir}/System.dll
                DEPENDS
                ${usd_net} # BUILD_Mono
                )
        add_custom_target(
            usd_net_unity
            ALL DEPENDS
            ${usd_net_unity_dll}
            )
        install(
            FILES
            ${CMAKE_BINARY_DIR}/${usd_net_unity_dll}
            DESTINATION
            ${usd_plugins_directory}
        )

    
    # USD.NET tests
    set(usd_net_tests_exe Tests.exe)
    add_custom_command(
            OUTPUT ${usd_net_tests_exe}
            COMMAND mcs /noconfig /langversion:4 /nostdlib+
            /warn:4 /optimize+ /out:${usd_net_tests_exe} /target:exe /unsafe
            ${CMAKE_SOURCE_DIR}/../src/Tests/*.cs
            ${CMAKE_SOURCE_DIR}/../src/Tests/Cases/*.cs
            ${CMAKE_SOURCE_DIR}/../src/Tests/Util/*.cs
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
endif()

set(CMAKE_INSTALL_RPATH "$ORIGIN")
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)
install(
    TARGETS
    ${usd_swig_library}
    DESTINATION
    ${swig_output_dir}
    )

# Install the bindings generated by Swig to the USD.NET folder
install(
    SCRIPT
        ${CMAKE_SOURCE_DIR}/install_usd_bindings.cmake
)

# Tag the generated folders so they are removed when doing `make clean`
set_property(DIRECTORY PROPERTY ADDITIONAL_MAKE_CLEAN_FILES 
    ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated
    ${CMAKE_SOURCE_DIR}/../cmake/generated
)
