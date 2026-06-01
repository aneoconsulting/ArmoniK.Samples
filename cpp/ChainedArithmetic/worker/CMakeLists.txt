cmake_minimum_required(VERSION 3.22)
set(PROJECT_NAME ArmoniK.Samples.Cpp.ChainedArithmetic.Worker)

project(${PROJECT_NAME})

set(CMAKE_CXX_STANDARD 17)

if(NOT TARGET ArmoniK.SDK.Worker)
    find_package(ArmoniK.SDK.Worker CONFIG REQUIRED)
endif()

FILE(GLOB_RECURSE SRC_WORKER_FILES ${CMAKE_CURRENT_SOURCE_DIR}/*.cpp)
FILE(GLOB_RECURSE HEADER_WORKER_FILES ${CMAKE_CURRENT_SOURCE_DIR}/*.h)

add_library(${PROJECT_NAME} SHARED ${SRC_WORKER_FILES} ${HEADER_WORKER_FILES})

target_link_libraries(${PROJECT_NAME}
    PUBLIC "-Wl,--whole-archive" ArmoniK.SDK.Worker "-Wl,--no-whole-archive")

target_include_directories(${PROJECT_NAME}
    PUBLIC "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>")

install(TARGETS ${PROJECT_NAME}
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR})
