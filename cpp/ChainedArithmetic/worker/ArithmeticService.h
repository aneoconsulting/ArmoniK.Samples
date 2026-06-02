#pragma once

#include <armonik/sdk/worker/ServiceBase.h>
#include <map>
#include <stdexcept>
#include <string>

class ArithmeticService : public ArmoniK::Sdk::Worker::ServiceBase {
public:
  std::string call(void *, const std::string &name, const std::map<std::string, std::string> &inputs) override {

    if (name == "multiply") {
      double num1 = std::stod(inputs.at("num1"));
      double num2 = std::stod(inputs.at("num2"));
      return std::to_string(num1 * num2);
    }

    if (name == "add") {
      double num1 = std::stod(inputs.at("num1"));
      double num2 = std::stod(inputs.at("num2"));
      return std::to_string(num1 + num2);
    }

    throw std::invalid_argument("Unknown method: " + name);
  }

  void *enter_session(const char *session_id) override { return new std::string(session_id); }
  void leave_session(void *session_ctx) override { delete static_cast<std::string *>(session_ctx); }
};
