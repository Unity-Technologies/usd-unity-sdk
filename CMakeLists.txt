cmake_minimum_required(VERSION 3.8)

project(usd-unity-sdk LANGUAGES CXX)
set (CMAKE_CXX_STANDARD 11)
list(APPEND CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/cmake)

find_package(pxr REQUIRED)
find_package(SWIG 3.0.12 REQUIRED)
find_package(Boost REQUIRED)
find_package(Python COMPONENTS Development)
find_package(TBB REQUIRED)
find_package(Unity REQUIRED)
find_package(CSharpCompiler REQUIRED)
include(${SWIG_USE_FILE})

if(WIN32)
   set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif()

include_directories(
    ${PXR_INCLUDE_DIRS}
    ${CMAKE_SOURCE_DIR}/src/UsdCs
    ${CMAKE_SOURCE_DIR}
    ${Boost_INCLUDE_DIRS}
    ${Python_INCLUDE_DIRS}
    ${TBB_INCLUDE_DIRS}
)

set(usd_swig_library UsdCs)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_source_files_properties(
        src/Swig/usdCs.i
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503;-DSWIGWORDSIZE64"
    endif()
    )
else()
    set_source_files_properties(
        src/Swig/usdCs.i
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503"
    )
endif()

swig_add_library(
    ${usd_swig_library}
    TYPE MODULE
    LANGUAGE csharp
    OUTPUT_DIR ${CMAKE_SOURCE_DIR}/src/USD.NET/generated
    SOURCES
    src/Swig/usdCs.i
    src/UsdCs/diagnosticHandler.cpp
    src/UsdCs/dll_main.cpp
)

set(swig_output_dir ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/x86_64)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
   set(swig_output_dir ${swig_output_dir}/MacOS)
elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
   set(swig_output_dir ${swig_output_dir}/Linux)
else()
   set(swig_output_dir ${swig_output_dir}/Windows)
endif()

if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        CXX_STANDARD_REQUIRED 11
        LIBRARY_OUTPUT_DIRECTORY
        ${swig_output_dir}
        BUNDLE TRUE
    )
elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        PREFIX lib
        CXX_STANDARD_REQUIRED 11
        LIBRARY_OUTPUT_DIRECTORY
        ${swig_output_dir}
    )
else()
    set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        CXX_STANDARD_REQUIRED 11
        LIBRARY_OUTPUT_DIRECTORY_RELEASE
        ${swig_output_dir}
        LIBRARY_OUTPUT_DIRECTORY_DEBUG
        ${swig_output_dir}
    )
endif()


swig_link_libraries(
    ${usd_swig_library}
    ${PXR_LIBRARIES}
)

if(WIN32)
    set(win32_dependencies "")
    string(REPLACE " " ";" PXR_LIBRARIES "${PXR_LIBRARIES}")
    foreach(lib ${PXR_LIBRARIES})
	list(APPEND win32_dependencies "${PXR_${lib}_LIBRARY}")
    endforeach()
    foreach(dep ${win32_dependencies})
        add_custom_command(
            TARGET ${usd_swig_library}
            POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy ${dep} ${swig_output_dir}
        )
    endforeach()
endif()

if(0)
    add_custom_target(
        generate_swig_scripts
        DEPENDS swig_scripts_gen
    )

    add_dependencies(
        ${usd_swig_library}
        generate_swig_scripts
    )

    add_custom_command(
        OUTPUT swig_scripts_gen
        COMMAND
            ${CMAKE_COMMAND} -E env PYTHONPATH=${PXR_CMAKE_DIR}/lib/python
            ${Python_EXECUTABLE} ${CMAKE_SOURCE_DIR}/src/Swig/scripts/gen.py
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
    )
endif()

set(usd_net_dll "${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/USD.NET.dll")
compile_csharp(
    OUTPUT
        ${usd_net_dll}
    EXTRA_ARGS
        "/target:library"
        "/unsafe"
    SOURCES
        "${CMAKE_SOURCE_DIR}/src/USD.NET/collections/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET/pooling/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET/serialization/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET/Properties/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET/sdf/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET/generated/*.cs"
    REFERENCES
        ${CSHARP_MSCORLIB_LIBRARY}
        ${CSHARP_SYSTEM_CORE_LIBRARY}
        ${CSHARP_SYSTEM_LIBRARY}
    DEPENDS
        ${usd_swig_library}
)

add_custom_target(
    usd_net
    ALL DEPENDS
    ${usd_net_dll}
)

set(usd_net_unity_dll "${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/USD.NET.Unity.dll")
compile_csharp(
    OUTPUT
        ${usd_net_unity_dll}
    EXTRA_ARGS
        "/target:library"
        "/unsafe"
    SOURCES
        "${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Geometry/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Properties/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Shading/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Shading/UsdPreviewSurface/*.cs"
        "${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Shading/UnityNative/*.cs"
    REFERENCES
        ${CSHARP_MSCORLIB_LIBRARY}
        ${CSHARP_SYSTEM_CORE_LIBRARY}
        ${CSHARP_SYSTEM_LIBRARY}
        ${CSHARP_UNITYEDITOR_LIBRARY}
        ${CSHARP_UNITYENGINE_LIBRARY}
        ${usd_net_dll}
    DEPENDS
        ${usd_net_dll}
)

add_custom_target(
    usd_net_unity
    ALL DEPENDS
    ${usd_net_unity_dll}
)






