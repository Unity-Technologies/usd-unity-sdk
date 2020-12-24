FILE(GLOB usd_tf ${CMAKE_SOURCE_DIR}/../cmake/generated/Tf*.cs)
FILE(INSTALL ${usd_tf} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/tf)

FILE(GLOB usd_js ${CMAKE_SOURCE_DIR}/../cmake/generated/Js*.cs)
FILE(INSTALL ${usd_js} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/js)

FILE(GLOB usd_plug ${CMAKE_SOURCE_DIR}/../cmake/generated/Plug*.cs)
FILE(INSTALL ${usd_plug} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/plug)

FILE(GLOB usd_gf ${CMAKE_SOURCE_DIR}/../cmake/generated/Gf*.cs)
FILE(INSTALL ${usd_gf} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/gf)

FILE(GLOB usd_vt ${CMAKE_SOURCE_DIR}/../cmake/generated/Vt*.cs)
FILE(INSTALL ${usd_vt} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/base/vt)

FILE(GLOB usd_ar ${CMAKE_SOURCE_DIR}/../cmake/generated/Ar*.cs)
FILE(INSTALL ${usd_ar} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/ar)

FILE(GLOB usd_sdf ${CMAKE_SOURCE_DIR}/../cmake/generated/Sdf*.cs)
FILE(INSTALL ${usd_sdf} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/sdf)

FILE(GLOB usd_usdcs ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdCs*.cs)
FILE(INSTALL ${usd_usdcs} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/usdCs)

FILE(GLOB usd_usdgeom ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdGeom*.cs)
FILE(INSTALL ${usd_usdgeom} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdGeom)

FILE(GLOB usd_usdshade ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdShade*.cs)
FILE(INSTALL ${usd_usdshade} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdShade)

FILE(GLOB usd_usdskel ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdSkel*.cs)
FILE(INSTALL ${usd_usdskel} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdSkel)

FILE(GLOB usd_kind ${CMAKE_SOURCE_DIR}/../cmake/generated/Kind*.cs)
FILE(INSTALL ${usd_kind} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/kind)

FILE(GLOB usd__ndr ${CMAKE_SOURCE_DIR}/../cmake/generated/_Ndr*.cs)
FILE(INSTALL ${usd__ndr} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/ndr)

FILE(GLOB usd_ndr ${CMAKE_SOURCE_DIR}/../cmake/generated/ndr*.cs)
FILE(INSTALL ${usd_ndr} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/ndr)

FILE(GLOB usd_pcp ${CMAKE_SOURCE_DIR}/../cmake/generated/Pcp*.cs)
FILE(INSTALL ${usd_pcp} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/pcp)

FILE(GLOB usd_sdr ${CMAKE_SOURCE_DIR}/../cmake/generated/Sdr*.cs)
FILE(INSTALL ${usd_sdr} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/sdr)

FILE(GLOB usd_usdlux ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdLux*.cs)
FILE(INSTALL ${usd_usdlux} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdLux)

FILE(GLOB usd_usdri ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdRi*.cs)
FILE(INSTALL ${usd_usdri} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdRi)

FILE(GLOB usd_usdutils ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdUtils*.cs)
FILE(INSTALL ${usd_usdutils} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdUtils)

FILE(GLOB usd_usdvol ${CMAKE_SOURCE_DIR}/../cmake/generated/UsdVol*.cs)
FILE(INSTALL ${usd_usdvol} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usdVol)


FILE(GLOB usd_usd ${CMAKE_SOURCE_DIR}/../cmake/generated/Usd*.cs)
LIST(REMOVE_ITEM usd_usd ${usd_usdcs} ${usd_usdgeom} ${usd_usdshade} ${usd_usdskel} ${usd_usdlux} ${usd_usdri} ${usd_usdutils} ${usd_usdvol})
FILE(INSTALL ${usd_usd} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/pxr/usd/usd)


FILE(GLOB usd_swigtypes ${CMAKE_SOURCE_DIR}/../cmake/generated/SWIGTYPE_*.cs)
FILE(INSTALL ${usd_swigtypes} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/SWIG)

FILE(GLOB usd_std ${CMAKE_SOURCE_DIR}/../cmake/generated/Std*.cs)
FILE(INSTALL ${usd_std} DESTINATION ${CMAKE_SOURCE_DIR}/../src/USD.NET/generated/std)

