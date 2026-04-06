#include "ResultHandler.h"
#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/SessionService.h>
#include <armonik/sdk/common/Configuration.h>
#include <armonik/sdk/common/DynamicLibrary.h>
#include <armonik/sdk/common/Properties.h>
#include <armonik/sdk/common/TaskOptions.h>
#include <armonik/sdk/common/TaskPayload.h>
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
  // This avoids hardcoding a filesystem path that may not exist inside the worker container.
  const auto blob_id = service.UploadLibrary(config.get("Worker__LibraryPath"));
  lib.library_blob_id = blob_id;
  logger.info("Library uploaded, blob ID: " + blob_id);

  // Encode the library descriptor (including blob ID) into the task options.
  // SetDynamicLibrary() writes LibraryBlobId, Symbol, and ConventionVersion into the options map.
  // Must be called after UploadLibrary() so library_blob_id is set.
  task_options.SetDynamicLibrary(lib);
  logger.info("Session ID: " + service.getSession());

  // Build the convention task payload.
  // method_name: name of the function to call in the worker library.
  // inputs:      named string values forwarded to the worker function as-is.
  // outputs:     named slots whose values the worker function is expected to fill.
  ArmoniK::Sdk::Common::TaskPayload payload;
  payload.method_name = "multiply";
  payload.inputs = {{"num1", "2"}, {"num2", "3"}};
  payload.outputs = {{"result", ""}};

  auto handler = std::make_shared<ResultHandler>(logger);

  // Submit the task using the convention path (JSON-serialized TaskPayload).
  // Pass task_options explicitly so the library blob ID is included.
  auto tasks = service.Submit({payload}, handler, task_options);
  logger.info("Task submitted: " + tasks[0]);

  // Wait until the task result arrives and the handler is called
  service.WaitResults();
  logger.info("Task processing complete.");

  service.CloseSession();
  return 0;
}
