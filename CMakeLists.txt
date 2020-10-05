cmake_minimum_required(VERSION 3.12)

project(usd-unity-sdk)

set(BUILD_USD_NET FALSE CACHE BOOL "Build USD.NET.dll and USD.NET.Unity.dll")
set(BUILD_BINDINGS TRUE CACHE BOOL "Build USD C# bindings")
set(BUILD_TESTS FALSE CACHE BOOL "Build USD.NET tests")

message("${PXR_USD_LOCATION_PYTHON_BUILD}")
if (NOT PXR_USD_LOCATION_PYTHON_BUILD)
    message(FATAL_ERROR "PXR_USD_LOCATION_PYTHON_BUILD is not set.")
endif ()
    
set(USD_PKG_DIR ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd)
set(USD_PLUGINS_DIR ${USD_PKG_DIR}/Runtime/Plugins)

find_package(USD REQUIRED)
find_package(PythonInterp 3.7 REQUIRED COMPONENTS Interpreter)
message("Python exec ${Python3_FOUND}   ${Python3_EXECUTABLE} ${Python3_INTERPRETER_ID}")

add_subdirectory(src)


