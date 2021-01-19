FILE(GLOB usd_tf ${CMAKE_BINARY_DIR}/generated/Tf*.cs)
FILE(INSTALL ${usd_tf} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/tf)
LIST(APPEND SWIG_FILES pxr/base/tf/*.cs)

FILE(GLOB usd_js ${CMAKE_BINARY_DIR}/generated/Js*.cs)
FILE(INSTALL ${usd_js} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/js)
LIST(APPEND SWIG_FILES pxr/base/js/*.cs)

FILE(GLOB usd_plug ${CMAKE_BINARY_DIR}/generated/Plug*.cs)
FILE(INSTALL ${usd_plug} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/plug)
LIST(APPEND SWIG_FILES pxr/base/plug/*.cs)

FILE(GLOB usd_gf ${CMAKE_BINARY_DIR}/generated/Gf*.cs)
FILE(INSTALL ${usd_gf} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/gf)
LIST(APPEND SWIG_FILES pxr/base/gf/*.cs)

FILE(GLOB usd_vt ${CMAKE_BINARY_DIR}/generated/Vt*.cs)
FILE(INSTALL ${usd_vt} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/vt)
LIST(APPEND SWIG_FILES pxr/base/vt/*.cs)

FILE(GLOB usd_ar ${CMAKE_BINARY_DIR}/generated/Ar*.cs)
FILE(INSTALL ${usd_ar} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/ar)
LIST(APPEND SWIG_FILES pxr/usd/ar/*.cs)

FILE(GLOB usd_sdf ${CMAKE_BINARY_DIR}/generated/Sdf*.cs)
FILE(INSTALL ${usd_sdf} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/sdf)
LIST(APPEND SWIG_FILES pxr/usd/sdf/*.cs)

FILE(GLOB usd_usdcs ${CMAKE_BINARY_DIR}/generated/UsdCs*.cs)
FILE(INSTALL ${usd_usdcs} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/usdCs)
LIST(APPEND SWIG_FILES usdCs/*.cs)

FILE(GLOB usd_usdgeom ${CMAKE_BINARY_DIR}/generated/UsdGeom*.cs)
FILE(INSTALL ${usd_usdgeom} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdGeom)
LIST(APPEND SWIG_FILES pxr/usd/usdGeom/*.cs)

FILE(GLOB usd_usdshade ${CMAKE_BINARY_DIR}/generated/UsdShade*.cs)
FILE(INSTALL ${usd_usdshade} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdShade)
LIST(APPEND SWIG_FILES pxr/usd/usdShade/*.cs)

FILE(GLOB usd_usdskel ${CMAKE_BINARY_DIR}/generated/UsdSkel*.cs)
FILE(INSTALL ${usd_usdskel} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdSkel)
LIST(APPEND SWIG_FILES pxr/usd/usdSkel/*.cs)

FILE(GLOB usd_kind ${CMAKE_BINARY_DIR}/generated/Kind*.cs)
FILE(INSTALL ${usd_kind} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/kind)
LIST(APPEND SWIG_FILES pxr/usd/kind/*.cs)

FILE(GLOB usd__ndr ${CMAKE_BINARY_DIR}/generated/_Ndr*.cs)
FILE(INSTALL ${usd__ndr} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/ndr)
LIST(APPEND SWIG_FILES pxr/usd/ndr/*.cs)

FILE(GLOB usd_ndr ${CMAKE_BINARY_DIR}/generated/ndr*.cs)
FILE(INSTALL ${usd_ndr} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/ndr)
LIST(APPEND SWIG_FILES pxr/usd/ndr/*.cs)

FILE(GLOB usd_pcp ${CMAKE_BINARY_DIR}/generated/Pcp*.cs)
FILE(INSTALL ${usd_pcp} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/pcp)
LIST(APPEND SWIG_FILES pxr/usd/pcp/*.cs)

FILE(GLOB usd_sdr ${CMAKE_BINARY_DIR}/generated/Sdr*.cs)
FILE(INSTALL ${usd_sdr} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/sdr)
LIST(APPEND SWIG_FILES pxr/usd/sdr/*.cs)

FILE(GLOB usd_usdlux ${CMAKE_BINARY_DIR}/generated/UsdLux*.cs)
FILE(INSTALL ${usd_usdlux} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdLux)
LIST(APPEND SWIG_FILES pxr/usd/usdLux/*.cs)

FILE(GLOB usd_usdri ${CMAKE_BINARY_DIR}/generated/UsdRi*.cs)
FILE(INSTALL ${usd_usdri} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdRi)
LIST(APPEND SWIG_FILES pxr/usd/usdRi/*.cs)

FILE(GLOB usd_usdutils ${CMAKE_BINARY_DIR}/generated/UsdUtils*.cs)
FILE(INSTALL ${usd_usdutils} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdUtils)
LIST(APPEND SWIG_FILES pxr/usd/usdUtils/*.cs)

FILE(GLOB usd_usdvol ${CMAKE_BINARY_DIR}/generated/UsdVol*.cs)
FILE(INSTALL ${usd_usdvol} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdVol)
LIST(APPEND SWIG_FILES pxr/usd/usdVol/*.cs)


FILE(GLOB usd_usd ${CMAKE_BINARY_DIR}/generated/Usd*.cs)
LIST(REMOVE_ITEM usd_usd ${usd_usdcs} ${usd_usdgeom} ${usd_usdshade} ${usd_usdskel} ${usd_usdlux} ${usd_usdri} ${usd_usdutils} ${usd_usdvol})
FILE(INSTALL ${usd_usd} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usd)
LIST(APPEND SWIG_FILES pxr/usd/usd/*.cs)


FILE(GLOB usd_swigtypes ${CMAKE_BINARY_DIR}/generated/SWIGTYPE_*.cs)
FILE(INSTALL ${usd_swigtypes} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/SWIG)
LIST(APPEND SWIG_FILES SWIG/*.cs)

FILE(GLOB usd_std ${CMAKE_BINARY_DIR}/generated/Std*.cs)
FILE(INSTALL ${usd_std} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/std)
LIST(APPEND SWIG_FILES std/*.cs)

# Create a CMakeLists.txt file to list all the generated files
set(GENERATED_CMAKE ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/CMakeLists.txt)
FILE(REMOVE ${GENERATED_CMAKE})
foreach(src_file ${SWIG_FILES})
    # ugly hack to avoid variable expansion ...
    FILE(APPEND ${GENERATED_CMAKE} "LIST(APPEND ALL_SOURCES $")
    FILE(APPEND ${GENERATED_CMAKE} "{CMAKE_CURRENT_SOURCE_DIR}/${src_file})\n")
endforeach()
# ugly hack to avoid variable expansion ...
FILE(APPEND ${GENERATED_CMAKE} "SET(SWIG_GENERATED_FILES $")
FILE(APPEND ${GENERATED_CMAKE} "{ALL_SOURCES} PARENT_SCOPE)")
