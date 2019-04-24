cmake_minimum_required(VERSION 3.8)

project(usd-unity-sdk LANGUAGES CXX)
set (CMAKE_CXX_STANDARD 11)
list(APPEND CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/cmake)

set(USD_VERSION "v19.01" CACHE STRING "USD version (git tag)")
set(dep_path ${CMAKE_SOURCE_DIR}/_deps)

include(ExternalProject)

ExternalProject_Add(
   Boost
   URL "https://downloads.sourceforge.net/project/boost/boost/1.55.0/boost_1_55_0.tar.gz"
   URL_HASH SHA1=61ed0e57d3c7c8985805bb0682de3f4c65f4b6e5
   BUILD_IN_SOURCE 1
   CONFIGURE_COMMAND ./bootstrap.sh --prefix=${dep_path}/boost/
   BUILD_COMMAND ./b2 install --prefix=${dep_path}/boost link=shared runtime-link=shared threading=multi --with-atomic --with-program_options --with-regex --with-filesystem --with-system variant=release
   INSTALL_COMMAND ""
)

file(MAKE_DIRECTORY ${dep_path}/tbb/lib)
ExternalProject_Add(
   tbb
   GIT_REPOSITORY https://github.com/01org/tbb.git
   GIT_TAG 4.4.6
   GIT_SHALLOW TRUE
   CONFIGURE_COMMAND ""
   BUILD_IN_SOURCE 1
   BUILD_COMMAND ${MAKE_EXE}
   INSTALL_COMMAND ""
)
ExternalProject_Get_Property(tbb SOURCE_DIR)
add_custom_command(
   TARGET tbb POST_BUILD
   COMMAND ${CMAKE_COMMAND} -E copy
      ${SOURCE_DIR}/build/*_release/libtbb*
      ${dep_path}/tbb/lib
)
add_custom_command(
   TARGET tbb POST_BUILD
   COMMAND ${CMAKE_COMMAND} -E copy_directory
      ${SOURCE_DIR}/include
      ${dep_path}/tbb/include
)

ExternalProject_Add(
   OpenEXR
   GIT_REPOSITORY https://github.com/openexr/openexr.git
   GIT_TAG v2.3.0
   GIT_SHALLOW TRUE
   CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${dep_path}/OpenEXR -DOPENEXR_BUILD_PYTHON_LIBS=OFF -DOPENEXR_ENABLE_TESTS=OFF -DCMAKE_BUILD_TYPE=Release
)

ExternalProject_Add(
   Alembic
   GIT_REPOSITORY https://github.com/alembic/alembic.git
   GIT_TAG 1.7.7
   GIT_SHALLOW TRUE
   CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${dep_path}/Alembic -DUSE_BINARIES=OFF -DUSE_TESTS=OFF -DCMAKE_BUILD_TYPE=Release
)

set(USD_EXE_LINKER_FLAGS -L${dep_path}/Alembic/lib)
list(APPEND USD_EXE_LINKER_FLAGS -lAlembic)
ExternalProject_Add(
   USD
   GIT_REPOSITORY "https://github.com/PixarAnimationStudios/USD.git"
   GIT_TAG ${USD_VERSION}
   GIT_SHALLOW TRUE
   GIT_PROGRESS TRUE
   CMAKE_ARGS -DTBB_ROOT_DIR=${dep_path}/tbb -DBOOST_ROOT=${dep_path}/boost -DPXR_ENABLE_PYTHON_SUPPORT=FALSE -DTBB_LIBRARY=${dep_path}/tbb/lib -DPXR_BUILD_IMAGING=OFF -DPXR_BUILD_MONOLITHIC=ON -DPXR_BUILD_USDVIEW=OFF -DPXR_BUILD_TESTS=OFF -DOPENEXR_LOCATION=${dep_path}/OpenEXR -DCMAKE_INSTALL_PREFIX=${dep_path}/USD/ -DPXR_BUILD_ALEMBIC_PLUGIN=FALSE -DALEMBIC_DIR=${dep_path}/Alembic -DPXR_ENABLE_HDF5_SUPPORT=FALSE -DCMAKE_BUILD_TYPE=Release
   DEPENDS Boost tbb OpenEXR Alembic
)


find_package(SWIG 3.0.12 REQUIRED)
include(${SWIG_USE_FILE})

if(WIN32)
   set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif()

include_directories(
    ${dep_path}/USD/include
    ${dep_path}/boost/include
    ${dep_path}/tbb/include
    ${CMAKE_SOURCE_DIR}/src/UsdCs
    ${CMAKE_SOURCE_DIR}
)

link_directories(
   ${dep_path}/USD/lib
)

set(usd_swig_library UsdCs)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_source_files_properties(
	src/Swig/usdCs.i
	PROPERTIES
	CPLUSPLUS ON
	SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503;-DSWIGWORDSIZE64"
	DEPENDS USD
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
    usd_ms
)

#if(WIN32)
    #set(win32_dependencies "")
    #string(REPLACE " " ";" PXR_LIBRARIES "${PXR_LIBRARIES}")
    #foreach(lib ${PXR_LIBRARIES})
	#list(APPEND win32_dependencies "${PXR_${lib}_LIBRARY}")
    #endforeach()
    #foreach(dep ${win32_dependencies})
        #add_custom_command(
            #TARGET ${usd_swig_library}
            #POST_BUILD
            #COMMAND ${CMAKE_COMMAND} -E copy ${dep} ${swig_output_dir}
        #)
    #endforeach()
#endif()

#if(0)
    #add_custom_target(
        #generate_swig_scripts
        #DEPENDS swig_scripts_gen
    #)

    #add_dependencies(
        #${usd_swig_library}
        #generate_swig_scripts
    #)

    #add_custom_command(
        #OUTPUT swig_scripts_gen
        #COMMAND
            #${CMAKE_COMMAND} -E env PYTHONPATH=${PXR_CMAKE_DIR}/lib/python
            #${Python_EXECUTABLE} ${CMAKE_SOURCE_DIR}/src/Swig/scripts/gen.py
        #WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
    #)
#endif()

#set(usd_net_dll "${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/USD.NET.dll")
#compile_csharp(
    #OUTPUT
        #${usd_net_dll}
    #EXTRA_ARGS
        #"/target:library"
        #"/unsafe"
    #SOURCES
        #"${CMAKE_SOURCE_DIR}/src/USD.NET/collections/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET/pooling/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET/serialization/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET/Properties/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET/sdf/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET/generated/*.cs"
    #REFERENCES
        #${CSHARP_MSCORLIB_LIBRARY}
        #${CSHARP_SYSTEM_CORE_LIBRARY}
        #${CSHARP_SYSTEM_LIBRARY}
    #DEPENDS
        #${usd_swig_library}
#)

#add_custom_target(
    #usd_net
    #ALL DEPENDS
    #${usd_net_dll}
#)

#set(usd_net_unity_dll "${CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Runtime/Plugins/USD.NET.Unity.dll")
#compile_csharp(
    #OUTPUT
        #${usd_net_unity_dll}
    #EXTRA_ARGS
        #"/target:library"
        #"/unsafe"
    #SOURCES
        #"${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Geometry/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Properties/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Shading/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Shading/UsdPreviewSurface/*.cs"
        #"${CMAKE_SOURCE_DIR}/src/USD.NET.Unity/Shading/UnityNative/*.cs"
    #REFERENCES
        #${CSHARP_MSCORLIB_LIBRARY}
        #${CSHARP_SYSTEM_CORE_LIBRARY}
        #${CSHARP_SYSTEM_LIBRARY}
        #${CSHARP_UNITYEDITOR_LIBRARY}
        #${CSHARP_UNITYENGINE_LIBRARY}
        #${usd_net_dll}
    #DEPENDS
        #${usd_net_dll}
#)

#add_custom_target(
    #usd_net_unity
    #ALL DEPENDS
    #${usd_net_unity_dll}
#)






