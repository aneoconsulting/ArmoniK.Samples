#pragma once

#include <armonik/sdk/worker/ServiceBase.h>
#include <iostream>
namespace ArmoniK {
namespace Sdk {
namespace Worker {
namespace Examples {

/**
 * @brief Example implementation of a ArmoniK::Sdk::Worker::ServiceBase
 */
class HelloService : ServiceBase {
public:
  void *enter_session(const char *session_id) override {
    std::cout << "HelloService entering session : " << session_id << std::endl;
    return new std::string(session_id);
  }
  std::string call(void *, const std::string &name, const std::string &input) override {
    std::cout << "Service method : " << name << std::endl;
    return input + " World!";
  }
  void leave_session(void *session_ctx) override {
    auto session_id = static_cast<std::string *>(session_ctx);
    std::cout << "HelloService leaving session : " << *session_id << std::endl;
    delete session_id;
  }
  ~HelloService() override { std::cout << "Deleted HelloService" << std::endl; };

  HelloService() : ServiceBase() { std::cout << "Created HelloService" << std::endl; }
};

} // namespace Examples
} // namespace Worker
} // namespace Sdk
} // namespace ArmoniK