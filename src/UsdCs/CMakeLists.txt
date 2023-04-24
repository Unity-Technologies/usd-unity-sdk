project(usdcs LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 14)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(CMAKE_INSTALL_RPATH "@loader_path/../Frameworks")
    set(CMAKE_INSTALL_RPATH_USE_LINK_PATH FALSE)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(CMAKE_INSTALL_RPATH "\$ORIGIN:\$ORIGIN/lib")
else ()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DNOMINMAX /bigobj /EHsc")
endif ()

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
set(DEPENDENCIES_INSTALL_DIR ${SWIG_OUTPUT_DIR})
if (${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/MacOS)
    set(DEPENDENCIES_INSTALL_DIR ${SWIG_OUTPUT_DIR}/UsdCs.bundle/Contents/Frameworks)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/Linux)
    set(DEPENDENCIES_INSTALL_DIR ${SWIG_OUTPUT_DIR}/lib)
else ()
    set(SWIG_OUTPUT_DIR ${SWIG_OUTPUT_DIR}/Windows)
    set(DEPENDENCIES_INSTALL_DIR ${SWIG_OUTPUT_DIR})
endif ()

include_directories(
    ${USD_INCLUDE_DIR}
    # Not sure how to avoid setting the boost include path explicitly
    ${USD_INCLUDE_DIR}/boost-1_70
    ${CMAKE_CURRENT_SOURCE_DIR}
    ${CMAKE_SOURCE_DIR}
)

link_directories(${USD_LIBRARY_DIR} ${USD_BIN_DIR})

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
    set(LINK_DEPENDENCIES Alembic.1.7 Iex-2_2.12 IexMath-2_2.12 Imath-2_2.12 IlmThread-2_2.12 IlmImf-2_2.22 Half.12 tbb)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_target_properties(${USD_SWIG_LIBRARY}
        PROPERTIES
        PREFIX lib
        CXX_STANDARD_REQUIRED 14
        )
    set(LINK_DEPENDENCIES Alembic Iex-2_2 IexMath-2_2 Imath-2_2 IlmThread-2_2 IlmImf-2_2 Half tbb)
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
     set_target_properties(${USD_SWIG_LIBRARY}
        PROPERTIES
        CXX_STANDARD_REQUIRED 14
        # USE_TARGET_INCLUDE_DIRECTORIES TRUE
        )
     set(LINK_DEPENDENCIES Alembic Iex-2_2 IexMath-2_2 Imath-2_2 IlmThread-2_2 IlmImf-2_2 Half)
endif ()


if (WIN32)
   add_dependencies(${USD_SWIG_LIBRARY} USD_TYPE_INFO)
else ()
   add_dependencies(${USD_SWIG_LIBRARY}_swig_compilation USD_TYPE_INFO)
endif ()



swig_link_libraries(${USD_SWIG_LIBRARY}
    usd_ms
    ${LINK_DEPENDENCIES}
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
if (${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
    list(APPEND PIXAR_DEPS ${USD_BIN_DIR}/${CMAKE_SHARED_LIBRARY_PREFIX}tbb${CMAKE_SHARED_LIBRARY_SUFFIX})
elseif (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    list(APPEND PIXAR_DEPS ${USD_LIBRARY_DIR}/${CMAKE_SHARED_LIBRARY_PREFIX}tbb${CMAKE_SHARED_LIBRARY_SUFFIX}.2)
endif()

# install(FILES ${PIXAR_DEPS} DESTINATION ${DEPENDENCIES_INSTALL_DIR})
# The above line is enough for windows and OSX but Linux libraries have the soname/linkername/realname setup with symlinks
# so we need to follow the symlink chain
install(CODE "set(PIXAR_DEPS \"${PIXAR_DEPS}\")")
install(CODE "set(DEPENDENCIES_INSTALL_DIR \"${DEPENDENCIES_INSTALL_DIR}\")")
install(CODE [[
#  file(GET_RUNTIME_DEPENDENCIES
#    LIBRARIES $<TARGET_FILE:UsdCs>
#    RESOLVED_DEPENDENCIES_VAR _r_deps
#    UNRESOLVED_DEPENDENCIES_VAR _u_deps
#    DIRECTORIES ${MY_DEPENDENCY_PATHS}
#  )
foreach(_file ${PIXAR_DEPS})
    file(INSTALL
      DESTINATION "${DEPENDENCIES_INSTALL_DIR}"
      TYPE SHARED_LIBRARY
      FOLLOW_SYMLINK_CHAIN
      FILES "${_file}"
    )
endforeach()
]])

# Install the usd schema folder
install(DIRECTORY "${USD_LIBRARY_DIR}/usd" DESTINATION "${SWIG_OUTPUT_DIR}/.."
        PATTERN "codegenTemplates" EXCLUDE)
# Install the usd plugin folder
install(DIRECTORY "${USD_LIBRARY_DIR}/../plugin/usd" DESTINATION "${SWIG_OUTPUT_DIR}/..")

# Tag the generated folders so they are removed when doing calling the clean target
# Doesn't work with the VC generator ...
set_target_properties(${USD_SWIG_LIBRARY} PROPERTIES ADDITIONAL_CLEAN_FILES
    "${CMAKE_SOURCE_DIR}/cmake/generated;${CMAKE_SOURCE_DIR}/../package/com.unity.formats.usd/Dependencies/USD.NET/generated"
)
