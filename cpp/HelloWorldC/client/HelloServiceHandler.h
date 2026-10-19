#include <armonik/common/logger/formatter.h>
#include <armonik/common/logger/logger.h>
#include <armonik/common/logger/writer.h>
#include <armonik/sdk/client/IServiceInvocationHandler.h>
#include <sstream>

class HelloServiceHandler : public ArmoniK::Sdk::Client::IServiceInvocationHandler {
public:
  explicit HelloServiceHandler(armonik::api::common::logger::Logger &logger);
  void HandleResponse(std::string &&result_payload, armonik::api::string_view taskId,
                      armonik::api::string_view result_id) override;
  void HandleError(const std::exception &e, armonik::api::string_view taskId) override;

  bool received = false;
  bool is_error = false;

  armonik::api::common::logger::LocalLogger logger;
};

HelloServiceHandler::HelloServiceHandler(armonik::api::common::logger::Logger &logger) : logger(logger.local()) {}

void HelloServiceHandler::HandleResponse(std::string &&result_payload, armonik::api::string_view taskId,
                                         armonik::api::string_view) {
  std::stringstream ss;
  ss << "HANDLE RESPONSE : Received result of size " << result_payload.size() << " for taskId " << taskId
     << "\nContent : " << result_payload << "\nRaw : ";
  for (char c : result_payload) {
    ss << static_cast<int>(c) << ' ';
  }
  ss << std::endl;
  logger.info(ss.str());
  received = true;
  is_error = false;
}

void HelloServiceHandler::HandleError(const std::exception &e, armonik::api::string_view taskId) {
  std::stringstream ss;
  ss << "HANDLE ERROR : Error for task id " << taskId << " : " << e.what() << std::endl;
  logger.error(ss.str());
  received = true;
  is_error = true;
}
