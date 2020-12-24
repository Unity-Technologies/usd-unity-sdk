if(BUILD_USDCS)
    add_subdirectory(UsdCs)
endif()

if(BUILD_USD_NET)
    add_subdirectory(USD.NET)
    add_subdirectory(USD.NET.Unity)
endif()

if(BUILD_TESTS)
    add_subdirectory(Tests)
endif()