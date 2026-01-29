if(NOT TARGET EasyAR::EasyAR)
add_library(EasyAR::EasyAR SHARED IMPORTED)
set_target_properties(EasyAR::EasyAR PROPERTIES
    IMPORTED_LOCATION "C:/Users/sarai/.gradle/caches/8.11/transforms/fd68e801cc1ad1e8575ea79866a3ff1a/transformed/jetified-EasyAR/prefab/modules/EasyAR/libs/android.arm64-v8a/libEasyAR.so"
    INTERFACE_INCLUDE_DIRECTORIES "C:/Users/sarai/.gradle/caches/8.11/transforms/fd68e801cc1ad1e8575ea79866a3ff1a/transformed/jetified-EasyAR/prefab/modules/EasyAR/include"
    INTERFACE_LINK_LIBRARIES ""
)
endif()

