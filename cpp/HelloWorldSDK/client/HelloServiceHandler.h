#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/IServiceInvocationHandler.h>
#include <sstream>

class HelloServiceHandler : public ArmoniK::Sdk::Client::IServiceInvocationHandler {
public:
  explicit HelloServiceHandler(armonik::api::common::logger::Logger &logger);
  void HandleResponse(const std::string &result_payload, const std::string &taskId);
  void HandleError(const std::exception &e, const std::string &taskId);

  bool received = false;
  bool is_error = false;

  armonik::api::common::logger::LocalLogger logger;
};

HelloServiceHandler::HelloServiceHandler(armonik::api::common::logger::Logger &logger) : logger(logger.local()) {}

// Handle response from the service
void HelloServiceHandler::HandleResponse(const std::string &result_payload, const std::string &taskId) {
  std::stringstream ss;
  ss << "HANDLE RESPONSE : Received result of size " << result_payload.size() << " for taskId " << taskId
     << "\nContent : " << result_payload << "\nRaw : ";

  // Log raw byte values
  for (char c : result_payload) {
    ss << static_cast<int>(c) << ' ';
  }

  ss << std::endl;
  logger.info(ss.str());
  received = true;
  is_error = false;
}

// Handle errors that occur during task execution
void HelloServiceHandler::HandleError(const std::exception &e, const std::string &taskId) {
  std::stringstream ss;
  ss << "HANDLE ERROR : Error for task id " << taskId << " : " << e.what() << std::endl;
  logger.error(ss.str());
  received = true;
  is_error = true;
}
