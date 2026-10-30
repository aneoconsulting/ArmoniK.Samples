#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/IServiceInvocationHandler.h>
#include <armonik/sdk/client/SessionService.h>
#include <armonik/sdk/common/BlobDefinition.h>
#include <armonik/sdk/common/Configuration.h>
#include <armonik/sdk/common/Properties.h>
#include <armonik/sdk/common/TaskDefinition.h>
#include <armonik/sdk/common/TaskOptions.h>
#include <fstream>
#include <iostream>
#include <map>
#include <mutex>
#include <nlohmann/json.hpp>

using json = nlohmann::json;

/**
 * @brief Keeps the output of each task, or the reason why it failed.
 */
class ResultHandler : public ArmoniK::Sdk::Client::IServiceInvocationHandler {
public:
  void HandleResponse(std::string &&result_payload, armonik::api::string_view taskId,
                      armonik::api::string_view) override {
    std::lock_guard<std::mutex> lock(mutex_);
    results_[std::string(taskId)] = std::move(result_payload);
  }

  void HandleError(const std::exception &e, armonik::api::string_view taskId) override {
    std::lock_guard<std::mutex> lock(mutex_);
    errors_[std::string(taskId)] = e.what();
  }

  // Called once every task is done
  const std::map<std::string, std::string> &results() const { return results_; }
  const std::map<std::string, std::string> &errors() const { return errors_; }

private:
  std::mutex mutex_;
  std::map<std::string, std::string> results_;
  std::map<std::string, std::string> errors_;
};

int main() {
  // Logs to stderr, the results go to stdout
  armonik::api::common::logger::Logger logger{armonik::api::common::logger::writer_file(std::cerr),
                                              armonik::api::common::logger::formatter_plain(true)};

  // appsettings.json, overridden by environment variables
  ArmoniK::Sdk::Common::Configuration config;
  config.add_json_configuration("./appsettings.json").add_env_configuration();
  const auto input = config.get("Input");
  const auto batch_size = std::stoul(config.get("BatchSize"));

  // The input is a full payload: {"evaluationDate": ..., "requests": [...]}
  std::ifstream file(input);
  if (!file) {
    logger.error("Cannot open the input file '" + input + "': set Input");
    return 1;
  }
  const auto document = json::parse(file);
  const auto evaluation_date = document.at("evaluationDate").get<std::string>();
  const auto &requests = document.at("requests");

  std::vector<json> batches;
  for (std::size_t i = 0; i < requests.size(); i += batch_size) {
    batches.emplace_back(json::array());
    for (std::size_t j = i; j < std::min(i + batch_size, requests.size()); ++j) {
      batches.back().push_back(requests[j]);
    }
  }

  ArmoniK::Sdk::Common::TaskOptions task_options("", "", "", "", config.get("PartitionId"));
  task_options.max_retries = 2;
  task_options.priority = 1;
  // ArmoniK SDK convention v1 (ConventionVersion and Symbol), expected by the worker, which ignores the method name
  task_options.SetConvention("price");

  ArmoniK::Sdk::Common::Properties properties{config, task_options};
  ArmoniK::Sdk::Client::SessionService service(properties, logger);
  logger.info("Session " + service.getSession() + ": " + std::to_string(requests.size()) + " requests in " +
              std::to_string(batches.size()) + " tasks");

  // One task per batch with the input "requests" expected by the worker. The SDK uploads the input and
  // creates one output per task, which receives the results.
  std::vector<ArmoniK::Sdk::Common::TaskDefinition> tasks;
  for (const auto &batch : batches) {
    const json payload = {{"version", 1}, {"evaluationDate", evaluation_date}, {"requests", batch}};
    tasks.emplace_back(ArmoniK::Sdk::Common::TaskDefinition().WithInput(
        "requests", ArmoniK::Sdk::Common::BlobDefinition::FromData(payload.dump())));
  }

  auto handler = std::make_shared<ResultHandler>();
  const auto task_ids = service.Submit(tasks, handler);
  service.WaitResults();
  service.CloseSession();

  // Reassemble the results in request order; a failed task yields one error per request of its batch
  json results = json::array();
  for (std::size_t i = 0; i < task_ids.size(); ++i) {
    const auto result = handler->results().find(task_ids[i]);
    if (result != handler->results().end()) {
      const auto output = json::parse(result->second);
      for (const auto &r : output.at("results")) {
        results.push_back(r);
      }
      continue;
    }
    const auto error = handler->errors().find(task_ids[i]);
    for (const auto &request : batches[i]) {
      results.push_back({{"id", request.value("id", json())},
                         {"error", "task failed: " + (error != handler->errors().end() ? error->second : "no result")}});
    }
  }

  std::cout << json{{"version", 1},
                    {"sessionId", service.getSession()},
                    {"evaluationDate", evaluation_date},
                    {"results", results}}
                   .dump(2)
            << std::endl;

  std::size_t errors = 0;
  for (const auto &r : results) {
    errors += r.contains("error");
  }
  logger.info(std::to_string(results.size()) + " results, " + std::to_string(errors) + " errors");
  return errors == 0 ? 0 : 1;
}
