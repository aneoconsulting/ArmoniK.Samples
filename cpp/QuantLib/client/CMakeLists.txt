cmake_minimum_required(VERSION 3.22)
set(PROJECT_NAME ArmoniK.Samples.Cpp.QuantLib.Client)

project(${PROJECT_NAME})

find_package(ArmoniK.Api.Client CONFIG REQUIRED)
find_package(nlohmann_json CONFIG REQUIRED)

if(NOT TARGET ArmoniK.SDK.Common)
	find_package(ArmoniK.SDK.Common CONFIG REQUIRED)
endif()
if(NOT TARGET ArmoniK.SDK.Client)
	find_package(ArmoniK.SDK.Client CONFIG REQUIRED)
endif()

add_executable(${PROJECT_NAME} main.cpp)

target_link_libraries(${PROJECT_NAME} PUBLIC ArmoniK.Api.Common ArmoniK.Api.Client ArmoniK.SDK.Common ArmoniK.SDK.Client nlohmann_json::nlohmann_json)

install(TARGETS ${PROJECT_NAME}
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        ARCHIVE DESTINATION ${CMAKE_INSTALL_BINDIR})
