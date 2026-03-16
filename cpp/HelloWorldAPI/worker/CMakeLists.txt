cmake_minimum_required(VERSION 3.22)
project(armonik_cpp_hello VERSION 0.1 LANGUAGES C CXX)

find_package(Threads)
find_package(ArmoniK.Api.Common CONFIG REQUIRED)
find_package(ArmoniK.Api.Worker REQUIRED)

set(SRC_DIR "${CMAKE_CURRENT_SOURCE_DIR}/src")

file(GLOB_RECURSE SRC_FILES ${SRC_DIR}/*.cpp)
file(GLOB_RECURSE HEADER_FILES ${SRC_DIR}/*.h)

add_executable(${PROJECT_NAME} ${SRC_FILES} ${HEADER_FILES})

set_property(TARGET ${PROJECT_NAME} PROPERTY CXX_STANDARD 14)

target_link_options(${PROJECT_NAME} PRIVATE
    -Wl,--copy-dt-needed-entries,--start-group
)

target_link_libraries(${PROJECT_NAME} PUBLIC ArmoniK.Api.Worker ArmoniK.Api.Common)

target_include_directories(${PROJECT_NAME} PUBLIC ${SRC_DIR} "$<INSTALL_INTERFACE:${CMAKE_INSTALL_INCLUDEDIR}>")

install(TARGETS ${PROJECT_NAME}
        EXPORT ${PROJECT_NAME}Targets
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
        ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        INCLUDES DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
        )
