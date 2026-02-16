#include <armonik/sdk/worker/ArmoniKSDKInterface.h>
#include <cstring>
#include <iostream>

#include "HelloService.h"

extern "C" void *armonik_create_service(const char *service_namespace, const char *service_name) {
  std::cout << "Creating service < " << service_namespace << "::" << service_name << " >" << std::endl;
  return new ArmoniK::Sdk::Worker::Examples::HelloService();
}