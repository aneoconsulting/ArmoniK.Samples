#pragma once

#include <armonik/common/logger/logger.h>
#include <armonik/sdk/client/IServiceInvocationHandler.h>
#include <map>
#include <mutex>
#include <sstream>

class ResultHandler : public ArmoniK::Sdk::Client::IServiceInvocationHandler {
public:
  explicit ResultHandler(armonik::api::common::logger::Logger &logger) : logger(logger.local()) {}

  void HandleResponse(const std::string &result_payload, const std::string &taskId,
                      const std::string &result_id) override {
    std::stringstream ss;
    ss << "Result for task " << taskId << ": " << result_payload;
    logger.info(ss.str());
    std::lock_guard<std::mutex> lock(mutex_);
    result_ids_[taskId] = result_id;
  }

  void HandleError(const std::exception &e, const std::string &taskId) override {
    std::stringstream ss;
    ss << "Error for task " << taskId << ": " << e.what();
    logger.error(ss.str());
  }

  std::string GetResultId(const std::string &taskId) const {
    std::lock_guard<std::mutex> lock(mutex_);
    return result_ids_.at(taskId);
  }

private:
  armonik::api::common::logger::LocalLogger logger;
  mutable std::mutex mutex_;
  std::map<std::string, std::string> result_ids_;
};
