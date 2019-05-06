cmake_minimum_required(VERSION 3.13)

project(usd-unity-sdk LANGUAGES CXX)
set(CMAKE_CXX_STANDARD 11)

set(USD_VERSION "v19.05" CACHE STRING "USD version (git tag)")

if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(boost_url "https://downloads.sourceforge.net/project/boost/boost/1.55.0/boost_1_55_0.tar.gz")
    set(boost_hash SHA1=61ed0e57d3c7c8985805bb0682de3f4c65f4b6e5)
    set(tbb_tag 4.4.6)
#elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    #set(boost_url "https://downloads.sourceforge.net/project/boost/boost/1.61.0/boost_1_61_0.tar.gz")
    #set(boost_hash SHA1=2d2b80771f584f327827193fcf3abf836edcdfaa)
    #set(tbb_tag 2017_U2)
#elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
#    set(boost_url "https://downloads.sourceforge.net/project/boost/boost/1.61.0/boost_1_61_0.tar.gz")
#    set(boost_hash SHA1=2d2b80771f584f327827193fcf3abf836edcdfaa)
#    set(tbb_tag 2017_U2)
else ()
    message(FATAL_ERROR "Unsupported platform")
endif ()

set(dep_path ${CMAKE_BINARY_DIR}/_deps)
set(base_dir ${CMAKE_SOURCE_DIR})

include(ExternalProject)

ExternalProject_Add(
    BUILD_Boost
    URL ${boost_url}
    URL_HASH ${boost_hash}
    BUILD_IN_SOURCE 1
    CONFIGURE_COMMAND ./bootstrap.sh --prefix=${dep_path}
    BUILD_COMMAND ./b2 install --prefix=${dep_path}
    link=static runtime-link=static threading=multi
    --with-atomic
    --with-program_options
    --with-regex
    --with-filesystem
    --with-system
    --with-date_time
    variant=release
    INSTALL_COMMAND ""
    )

file(MAKE_DIRECTORY ${dep_path}/lib)
ExternalProject_Add(
    BUILD_tbb
    GIT_REPOSITORY https://github.com/01org/tbb.git
    GIT_TAG ${tbb_tag}
    GIT_SHALLOW TRUE
    CONFIGURE_COMMAND ""
    BUILD_IN_SOURCE 1
    BUILD_COMMAND make tbb extra_inc=big_iron.inc
    INSTALL_COMMAND ${CMAKE_COMMAND} -E copy_directory <SOURCE_DIR>/include ${dep_path}/include
    )
ExternalProject_Get_Property(BUILD_tbb SOURCE_DIR)
add_custom_command(
    TARGET BUILD_tbb POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy
    ${SOURCE_DIR}/build/*_release/libtbb.a
    ${dep_path}/lib
    BYPRODUCTS
    ${dep_path}/lib/libtbb.a
    )

ExternalProject_Add(
    BUILD_OpenEXR
    GIT_REPOSITORY https://github.com/openexr/openexr.git
    GIT_TAG v2.3.0
    GIT_SHALLOW TRUE
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${dep_path}
    -DOPENEXR_BUILD_PYTHON_LIBS=OFF
    -DOPENEXR_ENABLE_TESTS=OFF
    -DOPENEXR_BUILD_UTILS=OFF
    -DOPENEXR_BUILD_VIEWERS=OFF
    -DCMAKE_BUILD_TYPE=Release
    -DCMAKE_PREFIX_PATH=${dep_path}
    -DOPENEXR_BUILD_SHARED=OFF
    -DOPENEXR_BUILD_STATIC=ON
    # The OpenEXR CMake appends _s to static libraries
    # This breaks finding/linking later, so patch to remove
    PATCH_COMMAND patch -p1 < ${base_dir}/ilm_static_lib.patch
    )

ExternalProject_Add(
    BUILD_Alembic
    GIT_REPOSITORY https://github.com/alembic/alembic.git
    GIT_TAG 1.7.7
    GIT_SHALLOW TRUE
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${dep_path}
    -DUSE_BINARIES=OFF
    -DUSE_TESTS=OFF
    -DUSE_HDF5=OFF
    -DCMAKE_BUILD_TYPE=Release
    -DCMAKE_PREFIX_PATH=${dep_path}
    -DUSE_STATIC_BOOST=ON
    -DALEMBIC_ILMBASE_LINK_STATIC=ON
    -DALEMBIC_SHARED_LIBS=OFF
    DEPENDS BUILD_OpenEXR BUILD_Boost
    )

ExternalProject_Add(
    BUILD_USD
    GIT_REPOSITORY "https://github.com/PixarAnimationStudios/USD.git"
    GIT_TAG ${USD_VERSION}
    GIT_SHALLOW TRUE
    GIT_PROGRESS TRUE
    CMAKE_ARGS -DPXR_ENABLE_PYTHON_SUPPORT=FALSE
    -DPXR_BUILD_DOCUMENTATION=OFF
    -DPXR_BUILD_USD_IMAGING=OFF
    -DPXR_BUILD_IMAGING=OFF
    -DPXR_BUILD_MONOLITHIC=ON
    -DPXR_BUILD_USDVIEW=OFF
    -DPXR_BUILD_TESTS=OFF
    -DCMAKE_INSTALL_PREFIX=${dep_path}
    -DPXR_BUILD_ALEMBIC_PLUGIN=ON
    -DPXR_ENABLE_HDF5_SUPPORT=OFF
    -DBUILD_SHARED_LIBS=OFF
    -DCMAKE_BUILD_TYPE=Release
    -DCMAKE_PREFIX_PATH=${dep_path}
    DEPENDS
    BUILD_Boost BUILD_OpenEXR BUILD_Alembic BUILD_tbb
    )
ExternalProject_Get_Property(BUILD_USD BINARY_DIR)

set(BUILD_USD_NET FALSE CACHE BOOL "Build mono and USD.NET.dll")

if(BUILD_USD_NET)
    ExternalProject_Add(
        BUILD_Mono
        GIT_REPOSITORY https://github.com/mono/mono.git
        GIT_TAG "mono-5.20.1.19"
        GIT_SHALLOW TRUE
        GIT_PROGRESS TRUE
        BUILD_IN_SOURCE TRUE
        CONFIGURE_COMMAND ./autogen.sh --prefix=${dep_path}
        )
endif()

find_package(SWIG 3.0.12 REQUIRED)
include(${SWIG_USE_FILE})

if (WIN32)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif ()

include_directories(
    ${dep_path}/include
    ${CMAKE_SOURCE_DIR}/../src/UsdCs
    ${CMAKE_SOURCE_DIR}/../
    )

link_directories(
    ${dep_path}/lib
    ${BINARY_DIR}/pxr
    )

set(usd_swig_library UsdCs)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_source_files_properties(
        ${CMAKE_SOURCE_DIR}/../src/Swig/usdCs.i
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503;-DSWIGWORDSIZE64"
        DEPENDS BUILD_USD
        )
else ()
    set_source_files_properties(
        ${CMAKE_SOURCE_DIR}/../src/Swig/usdCs.i
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503"
        DEPENDS BUILD_USD
        )
endif ()

swig_add_library(
    ${usd_swig_library}
    TYPE MODULE
    LANGUAGE csharp
    OUTPUT_DIR ${CMAKE_SOURCE_DIR}/generated
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
        CXX_STANDARD_REQUIRED 11
        BUNDLE TRUE
        )
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_target_properties(
        ${usd_swig_library}
        PROPERTIES
        PREFIX lib
        CXX_STANDARD_REQUIRED 11
        )
#elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
     #set_target_properties(
        #${usd_swig_library}
        #PROPERTIES
        #CXX_STANDARD_REQUIRED 11
#)
endif ()


swig_link_libraries(
    ${usd_swig_library}
    # Link against USD monolithic
    -Wl,--whole-archive usd_m -Wl,--no-whole-archive
    Alembic
    Iex-2_3
    IexMath-2_3
    Imath-2_3
    IlmThread-2_3
    IlmImf-2_3
    Half-2_3
    tbb
    )

add_dependencies(
    ${usd_swig_library}
    BUILD_USD
    )

if(BUILD_USD_NET)
    set(usd_net_dll USD.NET.dll)
    set(mono_lib_dir ${dep_path}/lib/mono/4.5)
    add_custom_command(
        OUTPUT ${usd_net_dll}
        COMMAND ${dep_path}/bin/mcs /noconfig /langversion:4 /nostdlib+
        /warn:4 /optimize+ /out:${usd_net_dll} /target:library /unsafe
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/collections/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/pooling/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/serialization/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/Properties/*.cs
        ${CMAKE_SOURCE_DIR}/../src/USD.NET/sdf/*.cs
        ${CMAKE_SOURCE_DIR}/generated/*.cs
        /reference:${mono_lib_dir}/mscorlib.dll
        /reference:${mono_lib_dir}/System.Core.dll
        /reference:${mono_lib_dir}/System.dll
        DEPENDS
        ${usd_swig_library} BUILD_Mono
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
endif()

set(CMAKE_INSTALL_RPATH "$ORIGIN")
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)
install(
    TARGETS
    ${usd_swig_library}
    DESTINATION
    ${swig_output_dir}
    )

