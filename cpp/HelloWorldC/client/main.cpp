#include "HelloServiceHandler.h"
#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/IServiceInvocationHandler.h>
#include <armonik/sdk/client/SessionService.h>
#include <armonik/sdk/common/Configuration.h>
#include <armonik/sdk/common/Properties.h>
#include <armonik/sdk/common/TaskPayload.h>
#include <iostream>

static void usage(const char *prog) {
  std::cerr << "Usage: " << prog << " [--error | --retry]\n"
            << "  (no option)  Submit a normal hello task; worker replies with 'Hello, World!'\n"
            << "  --error      Worker returns a permanent error (task is not retried)\n"
            << "  --retry      Worker returns a transient error (task is retried until max_retries)\n";
}

int main(int argc, char *argv[]) {
  armonik::api::common::logger::Logger logger{armonik::api::common::logger::writer_console(),
                                              armonik::api::common::logger::formatter_plain(true)};

  // Parse the single optional flag
  std::string function_name = "hello";
  int max_retries = 1;

  for (int i = 1; i < argc; ++i) {
    std::string arg(argv[i]);
    if (arg == "--error") {
      function_name = "error";
    } else if (arg == "--retry") {
      function_name = "retry";
      max_retries = 3;
    } else {
      usage(argv[0]);
      return 1;
    }
  }

  logger.info("Starting Hello World ArmoniK Client (pure C worker), mode: " + function_name);

  ArmoniK::Sdk::Common::Configuration config;
  config.add_json_configuration("./appsettings.json").add_env_configuration();

  // The worker is a pure C shared library — no SDK version suffix in the filename
  ArmoniK::Sdk::Common::TaskOptions session_task_options("libArmoniK.Samples.C.Hello.Worker.so", "", "Examples",
                                                         "HelloService", config.get("PartitionId"));
  session_task_options.max_retries = max_retries;

  ArmoniK::Sdk::Common::Properties properties{config, session_task_options};

  ArmoniK::Sdk::Client::SessionService service(properties, logger);

  logger.info("Session ID: " + service.getSession());

  ArmoniK::Sdk::Common::TaskPayload task_payload(function_name, "Hello,");

  auto handler = std::make_shared<HelloServiceHandler>(logger);

  auto tasks = service.Submit({task_payload}, handler);

  logger.info("Task Submitted: " + tasks[0]);

  service.WaitResults();

  logger.info("Task Processing Complete.");

  service.CloseSession();

  return 0;
}
