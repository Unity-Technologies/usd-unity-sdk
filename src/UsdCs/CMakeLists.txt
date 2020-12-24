project(usdcs LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 14)
if (WIN32)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif ()
set(CMAKE_INSTALL_RPATH "$ORIGIN")
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)

find_package(SWIG 3.0.12 REQUIRED)
include(${SWIG_USE_FILE})

include(${CMAKE_CURRENT_SOURCE_DIR}/../Swig/CMakeLists.txt)

# Generate type info from python
add_custom_target(USD_TYPE_INFO
    COMMAND gen_type_info.cmd ${PXR_USD_LOCATION_PYTHON_BUILD}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/bin
    BYPRODUCTS ${CMAKE_MODULE_PATH}/../generated/SdfValueTypeNames.cs
    COMMENT "Generating type info from USD python"
)

set(USD_SWIG_LIBRARY UsdCs)
set(SWIG_GENERATED_FILEDIR ${CMAKE_SOURCE_DIR}/cmake/generated)
set(SWIG_OUTPUT_DIR ${USD_PLUGINS_DIR}/x86_64)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/MacOS)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/Linux)
else ()
    # This is consistent with other os, but VS build were done at the root
    # set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/Windows)
endif ()

include_directories(
    ${USD_INCLUDE_DIR}
    # Not sure how to avoid setting the boost include path explicitly
    ${USD_INCLUDE_DIR}/boost-1_70
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CMAKE_SOURCE_DIR}
)

link_directories(${USD_LIBRARY_DIR})

swig_add_library(${USD_SWIG_LIBRARY}
    TYPE MODULE
    LANGUAGE csharp
    OUTPUT_DIR ${SWIG_GENERATED_FILEDIR}
    SOURCES
    ${USD_CS_SWIG_SHIM}
    diagnosticHandler.cpp
    dll_main.cpp
)

if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set_target_properties(${USD_SWIG_LIBRARY}
        PROPERTIES
        CXX_STANDARD_REQUIRED 14
        BUNDLE TRUE
        )
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_target_properties(${USD_SWIG_LIBRARY}
        PROPERTIES
        PREFIX lib
        CXX_STANDARD_REQUIRED 14
        )
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
     set_target_properties(${USD_SWIG_LIBRARY}
        PROPERTIES
        CXX_STANDARD_REQUIRED 14
        # USE_TARGET_INCLUDE_DIRECTORIES TRUE
        )
endif ()


add_dependencies(${USD_SWIG_LIBRARY}
    USD_TYPE_INFO
    )

swig_link_libraries(${USD_SWIG_LIBRARY}
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

# Decorate mono pinvoke calls
add_custom_target(DECORATE_MONO_PINVOKE ALL
    COMMAND ${PYTHON_EXECUTABLE} add_MonoPInvokeCallback_attribute.py
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/bin
    COMMENT "Decorate pinvoke callbacks "
)
add_dependencies(DECORATE_MONO_PINVOKE ${USD_SWIG_LIBRARY})

add_custom_target(USD_CS ALL
    DEPENDS
    ${DECORATE_MONO_PINVOKE}
)

# Install UsdCs lib to the package Runtime/Plugins/... folder
install(
    TARGETS
    ${USD_SWIG_LIBRARY}
    DESTINATION
    ${SWIG_OUTPUT_DIR}
    )
# Install the swig generated cs files to USD_NET
install(SCRIPT ${CMAKE_SOURCE_DIR}/cmake/install_usd_bindings.cmake)

# Tag the generated folders so they are removed when doing `make clean`
set_target_properties(${USD_CS} PROPERTIES ADDITIONAL_CLEAN_FILES
    "${CMAKE_SOURCE_DIR}/cmake/generated;${CMAKE_SOURCE_DIR}/src/USD.NET/generated"
)
