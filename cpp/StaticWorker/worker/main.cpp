#include "ArithmeticService.h"
#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/common/Configuration.h>
#include <armonik/sdk/worker/ConventionExecutor.h>
#include <armonik/worker/Worker/ArmoniKWorker.h>
#include <armonik/worker/utils/WorkerServer.h>

// Worker with the service compiled in: no shared library is loaded at runtime.
// ConventionExecutor decodes each convention task, passes its named inputs to the service and sends the result.
class StaticWorker : public armonik::api::worker::ArmoniKWorker {
public:
  explicit StaticWorker(std::unique_ptr<armonik::api::grpc::v1::agent::Agent::Stub> agent)
      : ArmoniKWorker(std::move(agent)), executor(service) {}

  armonik::api::worker::ProcessStatus Execute(armonik::api::worker::TaskHandler &taskHandler) override {
    return executor.Execute(taskHandler);
  }

private:
  ArithmeticService service;
  ArmoniK::Sdk::Worker::ConventionExecutor executor;
};

int main() {
  ArmoniK::Sdk::Common::Configuration config;
  config.add_env_configuration();

  armonik::api::common::logger::Logger logger{armonik::api::common::logger::writer_console(),
                                              armonik::api::common::logger::formatter_plain(true),
                                              config.get_log_level()};
  logger.info("Starting static ArmoniK worker...");

  try {
    armonik::api::worker::WorkerServer::create<StaticWorker>(armonik::api::common::utils::Configuration(config))->run();
  } catch (const std::exception &e) {
    logger.fatal(std::string("Error in worker: ") + e.what());
    return 1;
  }

  logger.info("Stopping static ArmoniK worker");
  return 0;
}
