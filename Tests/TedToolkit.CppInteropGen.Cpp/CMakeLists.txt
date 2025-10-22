cmake_minimum_required(VERSION 3.31)
project(ArchiToolkit_CppInteropGen_Cpp)

set(CMAKE_CXX_STANDARD 20)

include_directories(${CMAKE_CURRENT_LIST_DIR}/../../src/libraries/ArchiToolkit.CppInteropGen.Cpp)

add_library(ArchiToolkit_CppInteropGen_Cpp SHARED
        FunctionPointer2_C.cpp)