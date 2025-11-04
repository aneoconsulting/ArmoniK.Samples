#include "objects.pb.h"
#include "utils/Configuration.h"
#include "logger/logger.h"
#include "logger/writer.h"
#include "logger/formatter.h"
#include "channel/ChannelFactory.h"
#include "sessions/SessionsClient.h"
#include "tasks/TasksClient.h"
#include "results/ResultsClient.h"
#include "events/EventsClient.h"

#include <map>

namespace ak_common = armonik::api::common;
namespace ak_client = armonik::api::client;
namespace ak_grpc = armonik::api::grpc::v1;

int main() {
    ak_common::logger::Logger logger{ak_common::logger::writer_console(), ak_common::logger::formatter_plain(true)};
    ak_common::utils::Configuration config;

    config.add_json_configuration("/appsettings.json").add_env_configuration();
    logger.info("Initialized client config.");
    ak_client::ChannelFactory channelFactory(config, logger);
    std::shared_ptr<::grpc::Channel> channel = channelFactory.create_channel();

    ak_grpc::TaskOptions taskOptions;

    logger.info ("Endpoint : " + config.get("GrpcClient__Endpoint"));
    std::string  used_partition = config.get("PartitionId").empty() ? "default" : config.get("PartitionId");
    logger.info("Using the '"+ used_partition + "' partition.");

    taskOptions.mutable_max_duration()->set_seconds(3600);
    taskOptions.set_max_retries(3);
    taskOptions.set_priority(1);
    taskOptions.set_partition_id(used_partition);
    taskOptions.set_application_name("hello-cpp");
    taskOptions.set_application_version("1.0");
    taskOptions.set_application_namespace("samples");

    ak_client::TasksClient tasksClient(ak_grpc::tasks::Tasks::NewStub(channel));
    ak_client::ResultsClient resultsClient(ak_grpc::results::Results::NewStub(channel));
    ak_client::SessionsClient sessionsClient(ak_grpc::sessions::Sessions::NewStub(channel));
    ak_client::EventsClient eventsClient(ak_grpc::events::Events::NewStub(channel));

    std::string session_id = sessionsClient.create_session(taskOptions, {used_partition});
    logger.info("Created session with id = " + session_id);

    std::map<std::string,std::string> results = resultsClient.create_results_metadata(session_id, {"output", "payload"});

    resultsClient.upload_result_data(session_id, results["payload"], "hello");
    logger.info("Uploaded payload.");

    auto task_info = tasksClient.submit_tasks(session_id, {ak_common::TaskCreation{results["payload"], {results["output"]}}})[0];
    logger.info("Task submitted.");
    logger.info("Going to wait for result with id = " + results["output"] );
    eventsClient.wait_for_result_availability(session_id, {results["output"]});

    logger.info("Finished waiting.");
    std::string taskResult = resultsClient.download_result_data(session_id, {results["output"]});
    logger.info("Got result = " + taskResult);

    return 0;
}
