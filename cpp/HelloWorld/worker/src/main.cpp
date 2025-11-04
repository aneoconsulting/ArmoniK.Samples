#include <iostream>
#include <memory>

#include <grpcpp/grpcpp.h>

#include "grpcpp/support/sync_stream.h"
#include "objects.pb.h"

#include "utils/WorkerServer.h"

#include "Worker/ArmoniKWorker.h"
#include "Worker/ProcessStatus.h"
#include "Worker/TaskHandler.h"
#include "exceptions/ArmoniKApiException.h"


class HelloWorker: public armonik::api::worker::ArmoniKWorker {
    public:
        explicit HelloWorker(std::unique_ptr<armonik::api::grpc::v1::agent::Agent::Stub> agent): ArmoniKWorker(std::move(agent)) {}

        armonik::api::worker::ProcessStatus Execute(armonik::api::worker::TaskHandler &taskHandler) override {
            std::string payload = taskHandler.getPayload();

            std::cout << "Received input = " << payload << "\n";

            try {
                if (!taskHandler.getExpectedResults().empty()) {
                    taskHandler.send_result(taskHandler.getExpectedResults()[0], payload).get();
                }

            } catch (const std::exception &e) {
                std::cout << "Error sending result " << e.what() << std::endl;
                return armonik::api::worker::ProcessStatus(e.what());
            }

            return armonik::api::worker::ProcessStatus::Ok;
        }
};

int main()
{
    std::cout << "Hello Worker started. gRPC version = " << grpc::Version() << "\n";

    armonik::api::common::utils::Configuration config;
    config.add_json_configuration("/appsettings.json").add_env_configuration();

    config.set("ComputePlane__WorkerChannel__Address", "/cache/armonik_worker.sock");
    config.set("ComputePlane__AgentChannel__Address", "/cache/armonik_agent.sock");

    try {
        armonik::api::worker::WorkerServer::create<HelloWorker>(config)->run();
    } catch (const std::exception &e) {
        std::cout << "Error in worker" << e.what() << std::endl;
    }

    std::cout << "Stopping Server..." << std::endl;
    return 0;
}
