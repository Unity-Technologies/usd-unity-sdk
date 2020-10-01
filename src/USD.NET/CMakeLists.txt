project(USD_NET)

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


set(USD_NET_DLL USD.NET.dll)
set(MCS C:/Users/julien.dubuisson/scoop/apps/mono/6.10.0.104/bin/mcs)
set(MONO_LIB_DIR C:/Users/julien.dubuisson/scoop/apps/mono/6.10.0.104/lib/mono/4.5 CACHE INTERNAL True)
add_custom_command(
    OUTPUT ${USD_NET_DLL}
    COMMAND ${MCS} /noconfig /langversion:4 /nostdlib+
    /warn:4 /optimize+ /out:${USD_NET_DLL} /target:library /unsafe
    ${CMAKE_CURRENT_LIST_DIR}/collections/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/pooling/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/serialization/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/Properties/*.cs
    ${CMAKE_CURRENT_LIST_DIR}/AotAttributes.cs
    ${CMAKE_CURRENT_LIST_DIR}/DiagnosticHandler.cs
    ${CMAKE_SOURCE_DIR}/cmake/generated/*.cs
    /reference:${MONO_LIB_DIR}/mscorlib.dll
    /reference:${MONO_LIB_DIR}/System.Core.dll
    /reference:${MONO_LIB_DIR}/System.dll
    DEPENDS
    ${USD_SWIG_LIBRARY} # BUILD_Mono
    )

add_custom_target(
    USD_NET
    ALL DEPENDS
    ${USD_NET_DLL}
    )

install(
    FILES
    ${CMAKE_CURRENT_BINARY_DIR}/${USD_NET_DLL}
    DESTINATION
    ${USD_PLUGINS_DIR}
    )