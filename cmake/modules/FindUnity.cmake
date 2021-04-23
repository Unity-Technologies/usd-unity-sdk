# ***********************************************************************
# Copyright (c) 2017 Unity Technologies. All rights reserved.
#
# Licensed under the ##LICENSENAME##.
# See LICENSE.md file in the project root for full license information.
# ***********************************************************************

# Find Unity and set things up to be able to compile Unity.
# Skip out now if we already found Unity.
if (Unity_FOUND)
  return()
endif()
# include(CSharpHelpers)


# If UNITY is set to the root of the Unity install, we use it, otherwise we set
# it to the default install location.
if (NOT DEFINED UNITY)
    if(${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
      set(UNITY "/Applications/Unity/Hub/Editor/${UNITY_VERSION}")
    elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
      set(CMAKE_FIND_LIBRARY_SUFFIXES ".dll")
      set(UNITY "C:/Program Files/Unity")
    elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
      set(UNITY "/opt/Unity")
    endif()
endif()

# Be generous about how to interpret UNITY:
# it can be the directory that includes the Unity executable,
# or the root of the Unity installation. On mac it can be the app bundle.
list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}")
if(${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
  list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}/Unity.app/Contents")
  list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}/Unity.app/Contents/MacOS")
elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
  list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}/Editor")
  list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}/Editor/${UNITY_VERSION}/Editor")
  list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}/Hub/Editor/${UNITY_VERSION}/Editor")
  list(APPEND UNITY_EXECUTABLE_PATHS "$ENV{UserProfile}/.Editor")
elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
  list(APPEND UNITY_EXECUTABLE_PATHS "${UNITY}/Editor")
endif()

# Find the unity binary.
find_program(UNITY_EDITOR_PATH Unity PATHS ${UNITY_EXECUTABLE_PATHS})

# Find the Unity assemblies. Where they are, relative to the unity binary, depends on platform.
if (DEFINED UNITY_EDITOR_DLL_PATH)
    message("Using Unity assemblies in ${UNITY_EDITOR_DLL_PATH}")
else()
    if(${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
        # The editor is   Unity.app/Contents/MacOS/Unity
        # The dlls are in Unity.app/Contents/Managed/*.dll
        get_filename_component(UNITY_EDITOR_DLL_PATH "${UNITY_EDITOR_PATH}" PATH)
        get_filename_component(UNITY_EDITOR_DLL_PATH "${UNITY_EDITOR_DLL_PATH}" DIRECTORY)
        set(UNITY_EDITOR_DLL_PATH "${UNITY_EDITOR_DLL_PATH}/Managed")

    else()
        # Windows and linux:
        # The editor is   .../Unity.exe (no extension on linux)
        # The dlls are in .../Data/Managed/*.dll
        get_filename_component(UNITY_EDITOR_DLL_PATH "${UNITY_EDITOR_PATH}" PATH)
        set(UNITY_EDITOR_DLL_PATH "${UNITY_EDITOR_DLL_PATH}/Data/Managed")
    endif()
endif()

# Just like find_library but specialized to find a dot-net assembly
# (which always ends in .dll regardless of platform).
macro(DOTNET_FIND_LIBRARY)
  set(_platformLibrarySuffix ${CMAKE_FIND_LIBRARY_SUFFIXES})
  set(CMAKE_FIND_LIBRARY_SUFFIXES ".dll")
  find_library(${ARGN})
  set(CMAKE_FIND_LIBRARY_SUFFIXES ${_platformLibrarySuffix})
endmacro()

dotnet_find_library(CSHARP_UNITYEDITOR_LIBRARY UnityEditor.dll PATHS ${UNITY_EDITOR_DLL_PATH})
dotnet_find_library(CSHARP_UNITYENGINE_LIBRARY UnityEngine.dll PATHS ${UNITY_EDITOR_DLL_PATH})

# Find Unity mono
find_program(MCS mcs PATHS ${UNITY_EXECUTABLE_PATHS}
            PATH_SUFFIXES Data/MonoBleedingEdge/bin MonoBleedingEdge/bin
            NO_DEFAULT_PATH
            )
find_file(MONO_MSCORLIB mscorlib.dll PATHS ${UNITY_EXECUTABLE_PATHS}
            PATH_SUFFIXES /Data/MonoBleedingEdge/lib/mono/4.5/ MonoBleedingEdge/lib/mono/4.5/
            NO_DEFAULT_PATH
            )
find_file(MONO_SYSTEM_CORE_LIB System.Core.dll PATHS ${UNITY_EXECUTABLE_PATHS}
            PATH_SUFFIXES /Data/MonoBleedingEdge/lib/mono/4.5 MonoBleedingEdge/lib/mono/4.5
            NO_DEFAULT_PATH
            )
find_file(MONO_SYSTEM_LIB System.dll PATHS ${UNITY_EXECUTABLE_PATHS}
            PATH_SUFFIXES /Data/MonoBleedingEdge/lib/mono/4.5 MonoBleedingEdge/lib/mono/4.5
            NO_DEFAULT_PATH
            )

# Check whether we found everything we needed.
message(STATUS ${UNITY_EDITOR_PATH})
message(STATUS ${CSHARP_UNITYEDITOR_LIBRARY})
message(STATUS ${CSHARP_UNITYENGINE_LIBRARY})
message(STATUS ${MCS})
message(STATUS ${MONO_MSCORLIB})
message(STATUS ${MONO_SYSTEM_CORE_LIB})
message(STATUS ${MONO_SYSTEM_LIB})

find_package_handle_standard_args(Unity
    REQUIRED_VARS
        UNITY_EDITOR_PATH
        CSHARP_UNITYEDITOR_LIBRARY
        CSHARP_UNITYENGINE_LIBRARY
        MCS
        MONO_MSCORLIB
        MONO_SYSTEM_CORE_LIB
        MONO_SYSTEM_LIB)
