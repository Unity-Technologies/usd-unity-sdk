# Generate type info from python
add_custom_target(usd_type_info ALL
    COMMAND gen_type_info.cmd
    WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}/bin
    COMMENT "Generating type info from USD python"
)

set(USD_CS_SWIG_SHIM ${CMAKE_CURRENT_LIST_DIR}/usdCs.i)
if (${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set_source_files_properties(
        ${USD_CS_SWIG_SHIM}
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503;-DSWIGWORDSIZE64"
        # DEPENDS BUILD_USD
        )
else ()
    set_source_files_properties(
        ${USD_CS_SWIG_SHIM}
        PROPERTIES
        CPLUSPLUS ON
        SWIG_FLAGS "-namespace;pxr;-w401;-w516;-w503"
        # DEPENDS BUILD_USD
        )
endif ()