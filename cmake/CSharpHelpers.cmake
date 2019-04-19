# ***********************************************************************
# Copyright (c) 2017 Unity Technologies. All rights reserved.
#
# Licensed under the ##LICENSENAME##.
# See LICENSE.md file in the project root for full license information.
# ***********************************************************************

# Just like find_library but specialized to find a dot-net assembly
# (which always ends in .dll regardless of platform).
macro(DOTNET_FIND_LIBRARY)
  set(_platformLibrarySuffix ${CMAKE_FIND_LIBRARY_SUFFIXES})
  set(CMAKE_FIND_LIBRARY_SUFFIXES ".dll")
  find_library(${ARGN})
  set(CMAKE_FIND_LIBRARY_SUFFIXES ${_platformLibrarySuffix})
endmacro()
