cmake_minimum_required(VERSION 3.22)
set(PROJECT_NAME ArmoniK.Samples.Cpp.Hello.SDK.Client)

project(${PROJECT_NAME})

SET(SRC_CLIENT_FILES "main.cpp")
SET(HEADER_CLIENT_FILES "HelloServiceHandler.h")

find_package(ArmoniK.Api.Client CONFIG REQUIRED)

if(NOT TARGET ArmoniK.SDK.Common)
	find_package(ArmoniK.SDK.Common CONFIG REQUIRED)
endif()
if(NOT TARGET ArmoniK.SDK.Client)
	find_package(ArmoniK.SDK.Client CONFIG REQUIRED)
endif()

add_executable(${PROJECT_NAME} ${SRC_CLIENT_FILES})

target_link_libraries(${PROJECT_NAME} PUBLIC ArmoniK.Api.Common ArmoniK.Api.Client ArmoniK.SDK.Common ArmoniK.SDK.Client)
target_include_directories(${PROJECT_NAME}
		PUBLIC
		"$<BUILD_INTERFACE:${HEADER_FILES_DIR}>"
)

if(POLICY CMP0135)
	cmake_policy(SET CMP0135 OLD)
endif()

install(TARGETS ${PROJECT_NAME}
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        ARCHIVE DESTINATION ${CMAKE_INSTALL_BINDIR})
