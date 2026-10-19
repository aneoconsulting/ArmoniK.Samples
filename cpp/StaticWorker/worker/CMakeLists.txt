cmake_minimum_required(VERSION 3.22)
set(PROJECT_NAME ArmoniK.Samples.Cpp.StaticWorker.Worker)

project(${PROJECT_NAME})

set(CMAKE_CXX_STANDARD 17)

# Needed until the installed ArmoniK.SDK.Common package finds it itself (ArmoniK.Extensions.Cpp#106)
find_package(nlohmann_json REQUIRED)
find_package(ArmoniK.Api.Worker CONFIG REQUIRED)
if(NOT TARGET ArmoniK.SDK.Common)
    find_package(ArmoniK.SDK.Common CONFIG REQUIRED)
endif()
if(NOT TARGET ArmoniK.SDK.Worker)
    find_package(ArmoniK.SDK.Worker CONFIG REQUIRED)
endif()

add_executable(${PROJECT_NAME} main.cpp ArithmeticService.h)

target_link_libraries(${PROJECT_NAME} PRIVATE ArmoniK.SDK.Worker ArmoniK.SDK.Common ArmoniK.Api.Worker)

install(TARGETS ${PROJECT_NAME}
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR})
