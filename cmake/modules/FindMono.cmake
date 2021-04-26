# - Try to find the mono, mcs, mscorlib.dll, System.Core.dll and System.dll
#
# Defines:
#
# MONO_FOUND             System has Mono dev files, as well as mono, mcs
# MONO_EXECUTABLE        Where to find 'mono'
# MCS_EXECUTABLE         Where to find 'mcs'
# MONO_PKG_CONFIG_PATH   Path for pkg-config files for this mono installation, e.g. /usr/lib/pkgconfig
# MONO_VERSION_MAJOR     The major version number of the Mono dev libraries
# MONO_VERSION_MINOR     The minor version number of the Mono dev libraries
# MONO_VERSION_PATCH     The patch/subminor version number of the Mono dev libraries
#


FIND_PROGRAM(MONO mono)
FIND_PROGRAM(MCS mcs)

GET_FILENAME_COMPONENT(MONO_ROOT ${MONO} DIRECTORY)
find_file(MONO_MSCORLIB mscorlib.dll PATHS ${MONO_ROOT}/..
            PATH_SUFFIXES /lib/mono/4.5/
            NO_DEFAULT_PATH
            )
find_file(MONO_SYSTEM_CORE_LIB System.Core.dll PATHS ${MONO_ROOT}/..
            PATH_SUFFIXES /lib/mono/4.5
            NO_DEFAULT_PATH
            )
find_file(MONO_SYSTEM_LIB System.dll PATHS ${MONO_ROOT}/..
            PATH_SUFFIXES /lib/mono/4.5
            NO_DEFAULT_PATH
            )

MESSAGE(STATUS "Found mono: ${MONO}")
MESSAGE(STATUS "Found mcs: ${MCS}")
MESSAGE(STATUS "Found mscorlib.dll: ${MONO_MSCORLIB}")
MESSAGE(STATUS "Found System.Core.dll: ${MONO_SYSTEM_CORE_LIB}")
MESSAGE(STATUS "Found System.dll: ${MONO_SYSTEM_LIB}")

find_package_handle_standard_args(Mono
    REQUIRED_VARS
        MONO
        MCS
        MONO_MSCORLIB
        MONO_SYSTEM_CORE_LIB
        MONO_SYSTEM_LIB)
