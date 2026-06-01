#include "ResultHandler.h"
#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/SessionService.h>
#include <armonik/sdk/common/BlobDefinition.h>
#include <armonik/sdk/common/Configuration.h>
#include <armonik/sdk/common/DynamicLibrary.h>
#include <armonik/sdk/common/Properties.h>
#include <armonik/sdk/common/TaskDefinition.h>
#include <armonik/sdk/common/TaskOptions.h>
#include <iostream>

int main() {
  // Setup logging
  armonik::api::common::logger::Logger logger{armonik::api::common::logger::writer_console(),
                                              armonik::api::common::logger::formatter_plain(true)};

  logger.info("Starting DynamicLibrary ArmoniK Client...");

  // Load configuration from file and environment
  ArmoniK::Sdk::Common::Configuration config;
  config.add_json_configuration("./appsettings.json").add_env_configuration();

  // Describe which shared library the worker should load.
  // symbol: optional prefix for armonik_* symbol names (leave empty for default "armonik" prefix).
  ArmoniK::Sdk::Common::DynamicLibrary lib;
  lib.symbol = config.get("Worker__LibrarySymbol"); // optional; leave empty for "armonik" prefix

  // Build task options that encode the library descriptor.
  // The partition_id routes tasks to the DynamicWorker partition.
  ArmoniK::Sdk::Common::TaskOptions task_options("", "", "", "", config.get("PartitionId"));
  task_options.max_retries = 1;

  // Create properties for the session
  ArmoniK::Sdk::Common::Properties properties{config, task_options};

  // Open a session
  ArmoniK::Sdk::Client::SessionService service(properties, logger);

  // Upload the worker library as a blob so the worker can fetch it at runtime.
  service.UploadLibrary(config.get("Worker__LibraryPath"), lib);
  logger.info("Library uploaded, blob ID: " + lib.library_blob_id);

  // Encode the library descriptor (including blob ID) into the task options.
  // SetDynamicLibrary() writes LibraryBlobId, Symbol, and ConventionVersion into the options map.
  // Must be called after UploadLibrary() so library_blob_id is set.
  task_options.SetDynamicLibrary(lib);
  logger.info("Session ID: " + service.getSession());

  auto handler = std::make_shared<ResultHandler>(logger);

  using ArmoniK::Sdk::Common::BlobDefinition;
  using ArmoniK::Sdk::Common::TaskDefinition;

  // Step 1: submit 3*3 and 4*4 as two independent multiply tasks.
  auto mul_tasks = service.Submit(
      {TaskDefinition("multiply", {{"num1", BlobDefinition::FromData("3")},
                                   {"num2", BlobDefinition::FromData("3")}}),
       TaskDefinition("multiply", {{"num1", BlobDefinition::FromData("4")},
                                   {"num2", BlobDefinition::FromData("4")}})},
      handler, task_options);
  logger.info("Multiply tasks submitted: " + mul_tasks[0] + ", " + mul_tasks[1]);

  service.WaitResults();

  // Step 2: submit add task wiring the two multiply results as inputs by blob ID.
  auto add_tasks = service.Submit(
      {TaskDefinition("add", {{"num1", BlobDefinition::FromBlobId(handler->GetResultId(mul_tasks[0]))},
                              {"num2", BlobDefinition::FromBlobId(handler->GetResultId(mul_tasks[1]))}})},
      handler, task_options);
  logger.info("Add task submitted: " + add_tasks[0]);

  service.WaitResults();
  logger.info("Task processing complete.");

  service.CloseSession();
  return 0;
}
