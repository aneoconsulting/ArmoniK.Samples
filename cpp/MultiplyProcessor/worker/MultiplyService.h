#pragma once

#include <armonik/sdk/worker/ServiceBase.h>
#include <nlohmann/json.hpp>
#include <stdexcept>
#include <string>

/**
 * @brief Convention-mode worker library that multiplies two numbers.
 *
 * The DynamicWorker calls armonik_call() with a JSON-serialized TaskPayload:
 *   {"method":"multiply","inputs":{"num1":"2","num2":"3"},"outputs":{"result":""}}
 *
 * The call() method parses that JSON, dispatches on method name, and returns
 * the result as a plain string (e.g. "6").
 */
class MultiplyService : public ArmoniK::Sdk::Worker::ServiceBase {
public:
  std::string call(void * /*session_ctx*/, const std::string &name, const std::string &input) override {
    auto payload = nlohmann::json::parse(input);

    if (name == "multiply") {
      double num1 = std::stod(payload.at("inputs").at("num1").get<std::string>());
      double num2 = std::stod(payload.at("inputs").at("num2").get<std::string>());
      return std::to_string(num1 * num2);
    }

    throw std::invalid_argument("Unknown method: " + name);
  }
};
