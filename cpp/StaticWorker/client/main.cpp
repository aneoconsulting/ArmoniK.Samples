#include "ResultHandler.h"
#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/SessionService.h>
#include <armonik/sdk/common/BlobDefinition.h>
#include <armonik/sdk/common/Configuration.h>
#include <armonik/sdk/common/Properties.h>
#include <armonik/sdk/common/TaskDefinition.h>
#include <armonik/sdk/common/TaskOptions.h>

int main() {
  // Setup logging
  armonik::api::common::logger::Logger logger{armonik::api::common::logger::writer_console(),
                                              armonik::api::common::logger::formatter_plain(true)};

  logger.info("Starting static worker ArmoniK Client...");

  // Load configuration from file and environment
  ArmoniK::Sdk::Common::Configuration config;
  config.add_json_configuration("./appsettings.json").add_env_configuration();

  // The partition_id routes tasks to the partition running the static worker.
  // SetConvention() writes only ConventionVersion and Symbol: the worker has the service compiled in,
  // so no library path or library blob is needed.
  ArmoniK::Sdk::Common::TaskOptions opts_multiply("", "", "", "", config.get("PartitionId"));
  opts_multiply.max_retries = 1;
  opts_multiply.SetConvention("multiply");

  auto opts_add = opts_multiply;
  opts_add.SetConvention("add");

  // Create properties for the session and open it
  ArmoniK::Sdk::Common::Properties properties{config, opts_multiply};
  ArmoniK::Sdk::Client::SessionService service(properties, logger);
  logger.info("Session ID: " + service.getSession());

  auto handler = std::make_shared<ResultHandler>(logger);

  using ArmoniK::Sdk::Common::BlobDefinition;
  using ArmoniK::Sdk::Common::TaskDefinition;

  // Step 1: submit 3*3 and 4*4 as two independent multiply tasks.
  auto mul_tasks = service.Submit(
      {TaskDefinition({{"num1", BlobDefinition::FromData("3")}, {"num2", BlobDefinition::FromData("3")}}),
       TaskDefinition({{"num1", BlobDefinition::FromData("4")}, {"num2", BlobDefinition::FromData("4")}})},
      handler, opts_multiply);
  logger.info("Multiply tasks submitted: " + mul_tasks[0] + ", " + mul_tasks[1]);

  service.WaitResults();

  // Step 2: submit an add task whose inputs are the two multiply results, given by blob ID.
  auto add_tasks = service.Submit(
      {TaskDefinition({{"num1", BlobDefinition::FromBlobId(handler->GetResultId(mul_tasks[0]))},
                       {"num2", BlobDefinition::FromBlobId(handler->GetResultId(mul_tasks[1]))}})},
      handler, opts_add);
  logger.info("Add task submitted: " + add_tasks[0]);

  service.WaitResults();
  logger.info("Task processing complete.");

  service.CloseSession();
  return 0;
}
