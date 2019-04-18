cmake_minimum_required(VERSION 3.8)

project(usd-unity-sdk LANGUAGES CXX)
set (CMAKE_CXX_STANDARD 11)
list(APPEND CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/cmake)

find_package(pxr REQUIRED)
find_package(SWIG 3.0.12 REQUIRED)
find_package(Boost REQUIRED)
find_package(Python COMPONENTS Development Interpreter)
find_package(TBB REQUIRED)
include(${SWIG_USE_FILE})

include_directories(
    ${PXR_INCLUDE_DIRS}
    ${CMAKE_SOURCE_DIR}/src/UsdCs
    ${CMAKE_SOURCE_DIR}
    ${Boost_INCLUDE_DIRS}
    ${Python_INCLUDE_DIRS}
    ${TBB_INCLUDE_DIRS}
)


message(STATUS "PATH: ${PXR_CMAKE_DIR}/lib/python")
message(STATUS "Python ${Python_EXECUTABLE}")

set(usd_swig_library UsdCs)
set_source_files_properties(
    src/Swig/usdCs.i
    PROPERTIES
    CPLUSPLUS ON
    SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503;-DSWIGWORDSIZE64"
)

swig_add_library(
    ${usd_swig_library}
    TYPE SHARED
    LANGUAGE csharp
    OUTPUT_DIR ${CMAKE_SOURCE_DIR}/src/USD.NET
    SOURCES
    src/Swig/usdCs.i
    src/UsdCs/diagnosticHandler.cpp
    src/UsdCs/dll_main.cpp
)

set_target_properties(
    ${usd_swig_library}
    PROPERTIES
    PREFIX lib
    CXX_STANDARD_REQUIRED 11
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/x86_64/Linux
)

swig_link_libraries(
    ${usd_swig_library}
    ${PXR_LIBRARIES}
)

if (NOT UNIX)
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
        COMMAND ${CMAKE_COMMAND} -E env PYTHONPATH=${PXR_CMAKE_DIR}/lib/python
        ${Python_EXECUTABLE} ${CMAKE_SOURCE_DIR}/src/Swig/scripts/gen.py
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
    )
endif()


