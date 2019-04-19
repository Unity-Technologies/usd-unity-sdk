# ***********************************************************************
# Copyright (c) 2017 Unity Technologies. All rights reserved.
#
# Licensed under the ##LICENSENAME##.
# See LICENSE.md file in the project root for full license information.
# ***********************************************************************
if (CSharpCompiler_FOUND)
  return()
endif()

include(CSharpHelpers)

# The list of .NET versions to look for.
# The first of these is the most-preferred.
set(NET_COMPILER_VERSIONS "4.5" "4.0.30319" "4.0")
set(NET_REFERENCE_ASSEMBLIES_VERSIONS "3.5" "2.0")
set(NET_LIB_VERSIONS "2.0.50727" "2.0")

# Platform-specific code.
if(${CMAKE_SYSTEM_NAME} STREQUAL "Darwin")
  if (DEFINED MONO_ROOT_PATH)
    message("Using Mono in ${MONO_ROOT_PATH}")
  else()
    # If MONO_ROOT_PATH isn't set then look for it in Unity.
    #
    # Since Unity 2018.1, Mono is bundled with Unity.app
    # Before 2018.1 Mono was bundled with Monodevelop.app which was
    # installed next to Unity.app. Try them both.
    # UNITY_EDITOR_DLL_PATH is Unity.app/Contents/Managed
    #
    # Note: we rely on the system mcs because the path and executable
    # name of mono vary so much.
    find_package(Unity REQUIRED)
    list(APPEND MONO_ROOT_PATH "${UNITY_EDITOR_DLL_PATH}/../Mono")
    list(APPEND MONO_ROOT_PATH "${UNITY_EDITOR_DLL_PATH}/../../../Monodevelop.app/Contents/Frameworks/Mono.framework/Versions/Current")
  endif()

  foreach(MONO ${MONO_ROOT_PATH})
      foreach(VERSION ${NET_REFERENCE_ASSEMBLIES_VERSIONS})
          list(APPEND CSHARP_DLL_PATHS "${MONO}/lib/mono/${VERSION}")
          list(APPEND REFERENCE_ASSEMBLIES_PATHS "${MONO}/lib/mono/${VERSION}")
      endforeach()
  endforeach()

  find_program(CSHARP_COMPILER mcs)

elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
  message("Using .Net versions ${NET_COMPILER_VERSIONS}")

  set(DOT_NET_PATH "C:/Windows/Microsoft.NET/Framework")
  set(REFERENCE_ASSEMBLIES_PATH "C:/Program Files \(x86\)/Reference Assemblies/Microsoft/Framework")

  foreach(VERSION ${NET_COMPILER_VERSIONS})
    list(APPEND CSHARP_COMPILER_PATHS "${DOT_NET_PATH}/v${VERSION}")
  endforeach()

  foreach(VERSION ${NET_REFERENCE_ASSEMBLIES_VERSIONS})
    list(APPEND REFERENCE_ASSEMBLIES_PATHS "${REFERENCE_ASSEMBLIES_PATH}/v${VERSION}")
  endforeach()

  foreach(VERSION ${NET_LIB_VERSIONS})
    list(APPEND CSHARP_DLL_PATHS "${DOT_NET_PATH}/v${VERSION}")
  endforeach()

  find_program(CSHARP_COMPILER csc mcs PATHS ${CSHARP_COMPILER_PATHS} NO_DEFAULT_PATH)
  find_program(CSHARP_COMPILER csc mcs PATHS ${CSHARP_COMPILER_PATHS})

elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
  find_package(Unity REQUIRED)
  list(APPEND MONO_ROOT_PATH "${UNITY_EDITOR_DLL_PATH}/../Mono")

  foreach(MONO ${MONO_ROOT_PATH})
      foreach(VERSION ${NET_REFERENCE_ASSEMBLIES_VERSIONS})
          list(APPEND CSHARP_DLL_PATHS "${MONO}/lib/mono/${VERSION}")
          list(APPEND REFERENCE_ASSEMBLIES_PATHS "${MONO}/lib/mono/${VERSION}")
      endforeach()
  endforeach()

  find_program(CSHARP_COMPILER mcs)
endif()

# message("Looking for mscorlib.dll in ${CSHARP_DLL_PATHS}")
dotnet_find_library(CSHARP_MSCORLIB_LIBRARY mscorlib.dll PATHS ${CSHARP_DLL_PATHS} NO_DEFAULT_PATH)
dotnet_find_library(CSHARP_MSCORLIB_LIBRARY mscorlib.dll PATHS ${CSHARP_DLL_PATHS})
dotnet_find_library(CSHARP_SYSTEM_LIBRARY System.dll PATHS ${CSHARP_DLL_PATHS} NO_DEFAULT_PATH)
dotnet_find_library(CSHARP_SYSTEM_LIBRARY System.dll PATHS ${CSHARP_DLL_PATHS})

# message("Looking for System.Core.dll in ${REFERENCE_ASSEMBLIES_PATHS}")
dotnet_find_library(CSHARP_SYSTEM_CORE_LIBRARY System.Core.dll PATHS ${REFERENCE_ASSEMBLIES_PATHS} NO_DEFAULT_PATH)
dotnet_find_library(CSHARP_SYSTEM_CORE_LIBRARY System.Core.dll PATHS ${REFERENCE_ASSEMBLIES_PATHS})

# Standard code to report whether we found the package or not.
#include(${CMAKE_CURRENT_LIST_DIR}/FindPackageHandleStandardArgs.cmake)
#FIND_PACKAGE_HANDLE_STANDARD_ARGS(CSharpCompiler DEFAULT_MSG
        #CSHARP_COMPILER
        #CSHARP_MSCORLIB_LIBRARY
        #CSHARP_SYSTEM_LIBRARY
        #CSHARP_SYSTEM_CORE_LIBRARY)

###########################################################################
# Now that we found everything, add a macro to compile a csharp assembly.
if(${CMAKE_SYSTEM_NAME} STREQUAL "Windows")
  set(_csharpCompile_warnings="/nowarn:1701,1702,2008")
else()
  set(_csharpCompile_warnings="/nowarn:1701,1702")
endif()

macro(compile_csharp)
  cmake_parse_arguments(_csharpCompile
      ""
      "OUTPUT"
      "SOURCES;REFERENCES;DEPENDS;EXTRA_ARGS"
      ${ARGN})

  file(TO_NATIVE_PATH "${_csharpCompile_SOURCES}" _csharpCompile_SOURCES)

  foreach(_csharpCompileRef ${_csharpCompile_REFERENCES})
    list(APPEND _csharpCompile_REFERENCE_ARGS "/reference:${_csharpCompileRef}")
  endforeach()

  add_custom_command(OUTPUT ${_csharpCompile_OUTPUT}
        COMMAND "${CSHARP_COMPILER}"
                /noconfig
                /langversion:4
                /nostdlib+
                /warn:4
                ${_csharpCompile_warnings}
                /optimize+
                /out:${_csharpCompile_OUTPUT}
                ${_csharpCompile_EXTRA_ARGS}
                ${_csharpCompile_REFERENCE_ARGS}
                ${_csharpCompile_SOURCES}
        DEPENDS ${_csharpCompile_DEPENDS})
endmacro()
