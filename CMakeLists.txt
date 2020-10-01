cmake_minimum_required(VERSION 3.12)

project(usd-unity-sdk)

set(BUILD_USD_NET FALSE CACHE BOOL "Build USD.NET.dll and USD.NET.Unity.dll")
set(BUILD_BINDINGS TRUE CACHE BOOL "Build USD C# bindings")
set(BUILD_TESTS FALSE CACHE BOOL "Build USD.NET tests")

set(USD_PKG_DIR ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd)
set(USD_PLUGINS_DIR ${USD_PKG_DIR}/Runtime/Plugins)

find_package(USD REQUIRED)

add_subdirectory(src)


