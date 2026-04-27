cmake_minimum_required(VERSION 3.22)
project(ArmoniK.Samples.C.Hello.Worker C)

include(GNUInstallDirs)

add_library(${PROJECT_NAME} SHARED hello_worker.c)

install(TARGETS ${PROJECT_NAME}
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR})
