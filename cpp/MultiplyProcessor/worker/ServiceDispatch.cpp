#include "MultiplyService.h"
#include <armonik/sdk/worker/ArmoniKSDKInterface.h>

extern "C" void *armonik_create_service(const char * /*service_namespace*/, const char * /*service_name*/) {
  return new MultiplyService();
}
