SET(INSTALL_DIR ${CMAKE_SOURCE_DIR}/../package/com.unity.formats.usd/Dependencies/USD.NET/generated)

FILE(GLOB usd_tf ${CMAKE_BINARY_DIR}/generated/Tf*.cs)
FILE(INSTALL ${usd_tf} DESTINATION ${INSTALL_DIR}/pxr/base/tf)
LIST(APPEND SWIG_FILES pxr/base/tf/*.cs)

FILE(GLOB usd_js ${CMAKE_BINARY_DIR}/generated/Js*.cs)
FILE(INSTALL ${usd_js} DESTINATION ${INSTALL_DIR}/pxr/base/js)
LIST(APPEND SWIG_FILES pxr/base/js/*.cs)

FILE(GLOB usd_plug ${CMAKE_BINARY_DIR}/generated/Plug*.cs)
FILE(INSTALL ${usd_plug} DESTINATION ${INSTALL_DIR}/pxr/base/plug)
LIST(APPEND SWIG_FILES pxr/base/plug/*.cs)

FILE(GLOB usd_gf ${CMAKE_BINARY_DIR}/generated/Gf*.cs)
FILE(INSTALL ${usd_gf} DESTINATION ${INSTALL_DIR}/pxr/base/gf)
LIST(APPEND SWIG_FILES pxr/base/gf/*.cs)

FILE(GLOB usd_vt ${CMAKE_BINARY_DIR}/generated/Vt*.cs)
FILE(INSTALL ${usd_vt} DESTINATION ${INSTALL_DIR}/pxr/base/vt)
LIST(APPEND SWIG_FILES pxr/base/vt/*.cs)

FILE(GLOB usd_ar ${CMAKE_BINARY_DIR}/generated/Ar*.cs)
FILE(INSTALL ${usd_ar} DESTINATION ${INSTALL_DIR}/pxr/usd/ar)
LIST(APPEND SWIG_FILES pxr/usd/ar/*.cs)

FILE(GLOB usd_sdf ${CMAKE_BINARY_DIR}/generated/Sdf*.cs)
FILE(INSTALL ${usd_sdf} DESTINATION ${INSTALL_DIR}/pxr/usd/sdf)
LIST(APPEND SWIG_FILES pxr/usd/sdf/*.cs)

FILE(GLOB usd_usdcs ${CMAKE_BINARY_DIR}/generated/UsdCs*.cs)
FILE(INSTALL ${usd_usdcs} DESTINATION ${INSTALL_DIR}/UsdCs)
LIST(APPEND SWIG_FILES UsdCs/*.cs)

FILE(GLOB usd_usdgeom ${CMAKE_BINARY_DIR}/generated/UsdGeom*.cs)
FILE(INSTALL ${usd_usdgeom} DESTINATION ${INSTALL_DIR}/pxr/usd/usdGeom)
LIST(APPEND SWIG_FILES pxr/usd/usdGeom/*.cs)

FILE(GLOB usd_usdshade ${CMAKE_BINARY_DIR}/generated/UsdShade*.cs)
FILE(INSTALL ${usd_usdshade} DESTINATION ${INSTALL_DIR}/pxr/usd/usdShade)
LIST(APPEND SWIG_FILES pxr/usd/usdShade/*.cs)

FILE(GLOB usd_usdskel ${CMAKE_BINARY_DIR}/generated/UsdSkel*.cs)
FILE(INSTALL ${usd_usdskel} DESTINATION ${INSTALL_DIR}/pxr/usd/usdSkel)
LIST(APPEND SWIG_FILES pxr/usd/usdSkel/*.cs)

FILE(GLOB usd_kind ${CMAKE_BINARY_DIR}/generated/Kind*.cs)
FILE(INSTALL ${usd_kind} DESTINATION ${INSTALL_DIR}/pxr/usd/kind)
LIST(APPEND SWIG_FILES pxr/usd/kind/*.cs)

FILE(GLOB usd_ndr ${CMAKE_BINARY_DIR}/generated/Ndr*.cs)
FILE(INSTALL ${usd_ndr} DESTINATION ${INSTALL_DIR}/pxr/usd/ndr)
LIST(APPEND SWIG_FILES pxr/usd/ndr/*.cs)

FILE(GLOB usd_pcp ${CMAKE_BINARY_DIR}/generated/Pcp*.cs)
FILE(INSTALL ${usd_pcp} DESTINATION ${INSTALL_DIR}/pxr/usd/pcp)
LIST(APPEND SWIG_FILES pxr/usd/pcp/*.cs)

FILE(GLOB usd_sdr ${CMAKE_BINARY_DIR}/generated/Sdr*.cs)
FILE(INSTALL ${usd_sdr} DESTINATION ${INSTALL_DIR}/pxr/usd/sdr)
LIST(APPEND SWIG_FILES pxr/usd/sdr/*.cs)

FILE(GLOB usd_usdlux ${CMAKE_BINARY_DIR}/generated/UsdLux*.cs)
FILE(INSTALL ${usd_usdlux} DESTINATION ${INSTALL_DIR}/pxr/usd/usdLux)
LIST(APPEND SWIG_FILES pxr/usd/usdLux/*.cs)

FILE(GLOB usd_usdri ${CMAKE_BINARY_DIR}/generated/UsdRi*.cs)
FILE(INSTALL ${usd_usdri} DESTINATION ${INSTALL_DIR}/pxr/usd/usdRi)
LIST(APPEND SWIG_FILES pxr/usd/usdRi/*.cs)

FILE(GLOB usd_usdutils ${CMAKE_BINARY_DIR}/generated/UsdUtils*.cs)
FILE(INSTALL ${usd_usdutils} DESTINATION ${INSTALL_DIR}/pxr/usd/usdUtils)
LIST(APPEND SWIG_FILES pxr/usd/usdUtils/*.cs)

FILE(GLOB usd_usdvol ${CMAKE_BINARY_DIR}/generated/UsdVol*.cs)
FILE(INSTALL ${usd_usdvol} DESTINATION ${INSTALL_DIR}/pxr/usd/usdVol)
LIST(APPEND SWIG_FILES pxr/usd/usdVol/*.cs)


FILE(GLOB usd_usd ${CMAKE_BINARY_DIR}/generated/Usd*.cs)
LIST(REMOVE_ITEM usd_usd ${usd_usdcs} ${usd_usdgeom} ${usd_usdshade} ${usd_usdskel} ${usd_usdlux} ${usd_usdri} ${usd_usdutils} ${usd_usdvol})
FILE(INSTALL ${usd_usd} DESTINATION ${INSTALL_DIR}/pxr/usd/usd)
LIST(APPEND SWIG_FILES pxr/usd/usd/*.cs)


FILE(GLOB usd_swigtypes ${CMAKE_BINARY_DIR}/generated/SWIGTYPE_*.cs)
FILE(INSTALL ${usd_swigtypes} DESTINATION ${INSTALL_DIR}/SWIG)
FILE(RENAME ${INSTALL_DIR}/SWIG/SWIGTYPE_p_std__pairT_VtDictionary__IteratorT_std__mapT_std__string_VtValue_std__lessT_t_t_p_std__mapT_std__string_VtValue_std__lessT_t_t__iterator_t_bool_t.cs
            ${INSTALL_DIR}/SWIG/SWIGTYPE_pair_VtDictionaryIterator_bool_t.cs)
FILE(RENAME ${INSTALL_DIR}/SWIG/SWIGTYPE_p_VtDictionary__IteratorT_std__mapT_std__string_VtValue_std__lessT_t_t_p_std__mapT_std__string_VtValue_std__lessT_t_t__iterator_t.cs
            ${INSTALL_DIR}/SWIG/SWIGTYPE_p_VtDictionaryIterator.cs)
LIST(APPEND SWIG_FILES SWIG/*.cs)

FILE(GLOB usd_std ${CMAKE_BINARY_DIR}/generated/Std*.cs)
FILE(INSTALL ${usd_std} DESTINATION ${INSTALL_DIR}/std)
LIST(APPEND SWIG_FILES std/*.cs)

FILE(GLOB usdCsCSHARP ${CMAKE_BINARY_DIR}/generated/*.cxx)
FILE(INSTALL ${usdCsCSHARP} DESTINATION ${INSTALL_DIR})

# Create a CMakeLists.txt file to list all the generated files
set(GENERATED_CMAKE ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/CMakeLists.txt)
FILE(REMOVE ${GENERATED_CMAKE})
FILE(APPEND ${GENERATED_CMAKE} "# This file has been automatically generated by the usdcs CMake component.\n")
FILE(APPEND ${GENERATED_CMAKE} "SET(USD_NET_SRC $")
FILE(APPEND ${GENERATED_CMAKE} "{CMAKE_SOURCE_DIR}/package/com.unity.formats.usd/Dependencies/USD.NET/generated)\n")
foreach(src_file ${SWIG_FILES})
    # ugly hack to avoid variable expansion ...
    FILE(APPEND ${GENERATED_CMAKE} "LIST(APPEND ALL_SOURCES $")
    FILE(APPEND ${GENERATED_CMAKE} "{USD_NET_SRC}/${src_file})\n")
endforeach()
# ugly hack to avoid variable expansion ...
FILE(APPEND ${GENERATED_CMAKE} "SET(SWIG_GENERATED_FILES $")
FILE(APPEND ${GENERATED_CMAKE} "{ALL_SOURCES} PARENT_SCOPE)")
