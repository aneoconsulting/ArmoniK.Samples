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

int main() {
  // Setup logging
  armonik::api::common::logger::Logger logger{armonik::api::common::logger::writer_console(),
                                              armonik::api::common::logger::formatter_plain(true)};

  logger.info("Starting Hello World ArmoniK Client...");

  // Load configuration from file and environment
  ArmoniK::Sdk::Common::Configuration config;
  config.add_json_configuration("./appsettings.json").add_env_configuration();

  // Setup task options for Hello Service
  ArmoniK::Sdk::Common::TaskOptions session_task_options("libArmoniK.Samples.Cpp.Hello.SDK.Worker.so",
                                                         config.get("WorkerLib__Version"), "Examples", "HelloService",
                                                         config.get("PartitionId"));
  session_task_options.max_retries = 1;

  // Create properties for the service
  ArmoniK::Sdk::Common::Properties properties{config, session_task_options};

  // Initialize session service
  ArmoniK::Sdk::Client::SessionService service(properties, logger);

  // Create a session and prepare a simple "Hello World" task
  logger.info("Session ID: " + service.getSession());
  std::string inputData = "Hello,"; // The input for our task

  // Create a task payload
  ArmoniK::Sdk::Common::TaskPayload task_payload("HelloService", inputData);

  // Create the handler
  auto handler = std::make_shared<HelloServiceHandler>(logger);

  // Submit a task
  auto tasks = service.Submit({task_payload}, handler);

  logger.info("Task Submitted: " + tasks[0]);

  // Wait for results
  service.WaitResults();

  logger.info("Task Processing Complete.");

  // Close sesssion
  service.CloseSession();

  return 0;
}
