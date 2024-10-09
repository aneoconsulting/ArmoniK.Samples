package org.armonik.client;

import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;

import com.google.protobuf.ByteString;
import com.google.protobuf.Duration;

import armonik.api.grpc.v1.Objects.TaskOptions;
import armonik.api.grpc.v1.results.ResultsCommon.CreateResultsRequest;
import armonik.api.grpc.v1.results.ResultsCommon.CreateResultsRequest.ResultCreate;
import armonik.api.grpc.v1.results.ResultsCommon.ResultRaw;
import armonik.api.grpc.v1.sessions.SessionsCommon.CreateSessionRequest;
import armonik.api.grpc.v1.sessions.SessionsGrpc;
import armonik.api.grpc.v1.sessions.SessionsGrpc.SessionsBlockingStub;
import armonik.api.grpc.v1.tasks.TasksCommon.SubmitTasksRequest;
import armonik.api.grpc.v1.tasks.TasksCommon.SubmitTasksRequest.TaskCreation;
import armonik.api.grpc.v1.tasks.TasksGrpc;
import armonik.api.grpc.v1.tasks.TasksGrpc.TasksBlockingStub;
import armonik.client.event.EventClient;
import armonik.client.event.util.records.EventSubscriptionResponseRecord;
import armonik.client.result.ResultClient;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import picocli.CommandLine;
import picocli.CommandLine.Option;

public class Main implements Runnable {

        @Option(names = "--endpoint", description = "Endpoint for the connection to ArmoniK control plane. Format: http://<host>:<port>", defaultValue = "http://localhost:5001")
        private String endpoint;

        public static void main(String[] args) throws InterruptedException {
                int exitCode = new CommandLine(new Main()).execute(args);
                System.exit(exitCode);
        }

        @Override
        public void run() {

                try {
                        // Parse the endpoint into a URL object to extract the host and port
                        URL url = new URL(endpoint);
                        String ipAddress = url.getHost();
                        int port = url.getPort() == -1 ? url.getDefaultPort() : url.getPort();

                        System.out.println("Connecting to: " + ipAddress + ":" + port);

                        // Creating a managed channel to connect to the server
                        ManagedChannel managedChannel = ManagedChannelBuilder.forAddress(ipAddress, port)
                                        .usePlaintext()
                                        .intercept()
                                        .build();

                        // Creating a synchronous session to interact with sessions
                        SessionsBlockingStub sessionClient = SessionsGrpc.newBlockingStub(managedChannel);

                        // Creating a synchronous task client for task submission
                        TasksBlockingStub taskClient = TasksGrpc.newBlockingStub(managedChannel);

                        // Define the payload to send
                        byte[] payload = new byte[0];

                        // Define the partition
                        String partitionId = "bench";

                        // Defining options for the task
                        TaskOptions taskOptions = TaskOptions.newBuilder()
                                        .setMaxDuration(Duration.newBuilder().setSeconds(3600).build())
                                        .setMaxRetries(3)
                                        .setPriority(1)
                                        .setPartitionId(partitionId)
                                        .putOptions("PayloadSize", String.valueOf(payload.length))
                                        .putOptions("ResultSize", "10")
                                        .build();
                        // Creating a session and obtaining its ID
                        String sessionId = sessionClient.createSession(CreateSessionRequest.newBuilder()
                                        .setDefaultTaskOption(taskOptions)
                                        .addAllPartitionIds(List.of(partitionId))
                                        .build()).getSessionId();
                        System.out.println(">> Session ID:" + sessionId);

                        // Create client for result creation
                        ResultClient resultClient = new ResultClient(managedChannel);

                        // Create the payload metadata (a result) and upload data at the same time
                        List<ResultRaw> results = resultClient.createResults(
                                        CreateResultsRequest.newBuilder()
                                                        .setSessionId(sessionId)
                                                        .addResults(ResultCreate.newBuilder()
                                                                        .setName("Payload")
                                                                        .setData(ByteString.copyFrom(payload))
                                                                        .build())
                                                        .build());

                        System.out.println(">> RESULTS RAW: ");
                        results.forEach(System.out::println);

                        List<TaskCreation> taskCreations = new ArrayList<>();
                        for (ResultRaw resultRaw : results) {
                                TaskCreation taskCreation = TaskCreation
                                                .newBuilder()
                                                .setPayloadId(resultRaw.getResultId())
                                                .addExpectedOutputKeys(resultRaw.getResultId())
                                                .build();
                                taskCreations.add(taskCreation);
                        }

                        // submit and get the taskId for the submitted task
                        String taskId = taskClient
                                        .submitTasks(SubmitTasksRequest.newBuilder()
                                                        .setSessionId(sessionId)
                                                        .setTaskOptions(taskOptions)
                                                        .addAllTaskCreations(taskCreations.stream()
                                                                        .<TaskCreation>map(taskCreation -> {
                                                                                boolean taskOptionExist = taskCreation
                                                                                                .getTaskOptions()
                                                                                                .getMaxRetries() != Integer.MIN_VALUE;
                                                                                if (!taskOptionExist) {
                                                                                        return taskCreation.toBuilder()
                                                                                                        .setTaskOptions(taskOptions)
                                                                                                        .build();
                                                                                }
                                                                                return taskCreation;
                                                                        }).toList())
                                                        .build())
                                        .getTaskInfosList()
                                        .get(0)
                                        .getTaskId();

                        System.out.println(">> Task ID: " + taskId);
                        EventClient eventClient = new EventClient(managedChannel);
                        List<String> resultsIds = results.stream().map(ResultRaw::getResultId).toList();

                        // getting events
                        List<EventSubscriptionResponseRecord> records = eventClient.getEventResponseRecords(sessionId,
                                        resultsIds);

                        records.forEach(record -> {
                                System.out.println("Task Status Update: " + record.taskStatusUpdate());
                                System.out.println("Result Status Update: " + record.resultStatusUpdate());
                                System.out.println("Result Owner Update: " + record.resultOwnerUpdate());
                                System.out.println("New Task: " + record.newTask());
                                System.out.println("New Result: " + record.newResult());
                        });
                        records.forEach(System.out::println);

                        System.out.println(
                                        "DOWNLOADING DATA FOR SESSION-ID: " + sessionId + " AND RESULT-ID:"
                                                        + results.get(0).getResultId());
                        List<byte[]> bytes = resultClient.downloadResultData(sessionId, results.get(0).getResultId());
                        StringBuilder binaryStringBuilder = new StringBuilder();
                        for (byte b : bytes.get(0)) {
                                String binaryString = String.format("%8s", Integer.toBinaryString(b & 0xFF))
                                                .replace(' ', '0');
                                binaryStringBuilder.append(binaryString);
                        }
                        String result = binaryStringBuilder.toString();
                        System.out.println("DATA DOWNLOADED: " + result);

                } catch (MalformedURLException e) {
                        System.err.println("Invalid endpoint format. Expected format: http://<host>:<port>");
                        e.printStackTrace();
                } catch (InterruptedException e) {
                        e.printStackTrace();
                }

        }
}
