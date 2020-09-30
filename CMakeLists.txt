cmake_minimum_required(VERSION 3.12)

project(usd-unity-sdk LANGUAGES CXX)
set(CMAKE_CXX_STANDARD 14)        
if (WIN32)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif ()

set(BUILD_USD_NET FALSE CACHE BOOL "Build mono and USD.NET.dll")
set(BUILD_BINDINGS TRUE CACHE BOOL "Build USD C# bindings")
set(BUILD_TESTS FALSE CACHE BOOL "Build USD>NET tests")
set(USD_PKG_DIR ${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd)
set(USD_PLUGINS_DIR ${USD_PKG_DIR}/Runtime/Plugins)

find_package(USD REQUIRED)
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

#set(dep_path ${CMAKE_BINARY_DIR}/_deps)
add_subdirectory(src)


#set(CMAKE_INSTALL_RPATH "$ORIGIN")
#set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)

