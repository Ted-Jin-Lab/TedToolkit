# TedToolkitCppInteropGenConfig.cmake

get_filename_component(TedToolkit_CPPINTEROPGEN_ROOT "${CMAKE_CURRENT_LIST_DIR}/../.." ABSOLUTE)

set(TedToolkit_CPPINTEROPGEN_INCLUDE_DIR "${TedToolkit_CPPINTEROPGEN_ROOT}/build/native/include")

add_library(TedToolkit::CppInteropGen INTERFACE IMPORTED)

set_target_properties(TedToolkit::CppInteropGen PROPERTIES
    INTERFACE_INCLUDE_DIRECTORIES "${TedToolkit_CPPINTEROPGEN_INCLUDE_DIR}"
)