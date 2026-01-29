if(NOT TARGET AREngineInterop::AREngineInterop)
add_library(AREngineInterop::AREngineInterop SHARED IMPORTED)
set_target_properties(AREngineInterop::AREngineInterop PROPERTIES
    IMPORTED_LOCATION "C:/Users/sarai/.gradle/caches/8.11/transforms/241d65e0de33ebf335b099331ace5ee9/transformed/jetified-AREngineInterop/prefab/modules/AREngineInterop/libs/android.arm64-v8a/libAREngineInterop.so"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/sarai/.gradle/caches/8.11/transforms/241d65e0de33ebf335b099331ace5ee9/transformed/jetified-AREngineInterop/prefab/modules/AREngineInterop/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

