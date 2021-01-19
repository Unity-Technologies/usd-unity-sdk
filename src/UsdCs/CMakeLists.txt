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
# TODO: I'd prefer to have a custom_command and a custom_target in order to have a well defined output but I can't make it run that way
add_custom_target(USD_TYPE_INFO
    COMMAND ${PYTHON_EXECUTABLE} gen_type_info.py ${PXR_USD_LOCATION_PYTHON_BUILD} ${CMAKE_BINARY_DIR}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/bin
    COMMENT "-[ Generating type info from USD python ]-"
    DEPENDS ${USD_INCLUDE_DIR}
)

set(USD_SWIG_LIBRARY UsdCs)
set(SWIG_GENERATED_FILEDIR ${CMAKE_BINARY_DIR}/generated)
set(SWIG_OUTPUT_DIR ${USD_PLUGINS_DIR}/x86_64)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/MacOS)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/Linux)
else ()
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/Windows)
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

set(LINK_DEPENDENCIES Alembic Iex-2_2 IexMath-2_2 Imath-2_2 IlmThread-2_2 IlmImf-2_2 Half)
swig_link_libraries(${USD_SWIG_LIBRARY}
    # Link against USD monolithic
    -Wl,--whole-archive usd_ms -Wl,--no-whole-archive
    ${LINK_DEPENDENCIES}
    tbb
    )

# Decorate mono pinvoke calls
add_custom_target(DECORATE_MONO_PINVOKE ALL
    COMMAND ${PYTHON_EXECUTABLE} add_MonoPInvokeCallback_attribute.py ${SWIG_GENERATED_FILEDIR}
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/bin
    COMMENT "-[ Decorate pinvoke callbacks ]-"
)
add_dependencies(DECORATE_MONO_PINVOKE ${USD_SWIG_LIBRARY})

add_custom_target(USD_CS_DLL ALL
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

# Install the debug info
if (WIN32)
    install(FILES $<TARGET_PDB_FILE:${USD_SWIG_LIBRARY}> DESTINATION ${SWIG_OUTPUT_DIR} OPTIONAL)
endif()

# Install the swig generated cs files to USD_NET
install(SCRIPT ${CMAKE_SOURCE_DIR}/cmake/install_usd_bindings.cmake)

# Install the runtime dependencies (usd and its dependencies)
set(PIXAR_DEPS usd_ms ${LINK_DEPENDENCIES})
list(TRANSFORM PIXAR_DEPS PREPEND ${USD_LIBRARY_DIR}/${CMAKE_SHARED_LIBRARY_PREFIX})
list(TRANSFORM PIXAR_DEPS APPEND ${CMAKE_SHARED_LIBRARY_SUFFIX})
install(FILES ${PIXAR_DEPS} DESTINATION ${SWIG_OUTPUT_DIR})

# Install the usd plugin folder
install(DIRECTORY "${USD_LIBRARY_DIR}/usd" DESTINATION "${SWIG_OUTPUT_DIR}/..")

# Tag the generated folders so they are removed when doing calling the clean target
# Doesn't work with the VC generator ...
set_target_properties(${USD_SWIG_LIBRARY} PROPERTIES ADDITIONAL_CLEAN_FILES
    "${CMAKE_SOURCE_DIR}/cmake/generated;${CMAKE_SOURCE_DIR}/src/USD.NET/generated"
)
