#pragma once

#include <armonik/common/logger/logger.h>
#include <armonik/sdk/client/IServiceInvocationHandler.h>
#include <sstream>

class ResultHandler : public ArmoniK::Sdk::Client::IServiceInvocationHandler {
public:
  explicit ResultHandler(armonik::api::common::logger::Logger &logger) : logger(logger.local()) {}

  void HandleResponse(const std::string &result_payload, const std::string &taskId) override {
    std::stringstream ss;
    ss << "Result for task " << taskId << ": " << result_payload;
    logger.info(ss.str());
  }

  void HandleError(const std::exception &e, const std::string &taskId) override {
    std::stringstream ss;
    ss << "Error for task " << taskId << ": " << e.what();
    logger.error(ss.str());
  }

private:
  armonik::api::common::logger::LocalLogger logger;
};
