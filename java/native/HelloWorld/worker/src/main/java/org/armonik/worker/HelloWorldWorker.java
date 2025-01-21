package org.armonik.worker;

import java.io.IOException;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.nio.charset.StandardCharsets;
import java.util.logging.Logger;

import armonik.api.grpc.v1.Objects.Empty;
import armonik.api.grpc.v1.Objects.Output;
import armonik.api.grpc.v1.worker.WorkerCommon.HealthCheckReply;
import armonik.api.grpc.v1.worker.WorkerCommon.HealthCheckReply.ServingStatus;
import armonik.api.grpc.v1.worker.WorkerCommon.ProcessReply;
import armonik.api.grpc.v1.worker.WorkerCommon.ProcessRequest;
import armonik.worker.FutureWorker;
import armonik.worker.UnixDomainSocketGrpcServer;
import armonik.worker.taskhandlers.FutureTaskHandler;
import io.grpc.stub.StreamObserver;

public class HelloWorldWorker extends FutureWorker {
    private static final Logger logger = Logger.getLogger(HelloWorldWorker.class.getName());

    HelloWorldWorker() throws UnknownHostException {
        super();
    }

    public static void main(String[] args) throws IOException, InterruptedException {

        System.out.println(InetAddress.getLocalHost().getHostAddress());

        var workerAddress = System.getenv("ComputePlane__WorkerChannel__Address");

        UnixDomainSocketGrpcServer server = new UnixDomainSocketGrpcServer();
        server.start(workerAddress, new HelloWorldWorker());
        server.blockUntilShutdown();
    }

    @Override
    public void process(ProcessRequest request, StreamObserver<ProcessReply> responseObserver) {

        logger.info("Request received for the worker");
        this.setStatus(ServingStatus.SERVING);
        FutureTaskHandler taskHandler = new FutureTaskHandler(request, this.getClient());
        String input = new String(taskHandler.getPayload(), StandardCharsets.UTF_8);
        String resultId = taskHandler.getExpectedResults().get(0);

        logger.info("Processing the request");
        String resContent = input + " World_";

        try {
            logger.info("Sending the response");
            taskHandler.notifyResultData(resultId,
                    resContent.getBytes(StandardCharsets.UTF_8))
                    .get();
            responseObserver.onNext(
                    ProcessReply.newBuilder()
                            .setOutput(Output
                                    .newBuilder()
                                    .setOk(Empty.newBuilder().build())
                                    .build())
                            .build());
            // responseObserver.onCompleted();
            super.process(request, responseObserver);
        } catch (Exception e) {
            logger.warning("Error with the response");
            this.setStatus(ServingStatus.NOT_SERVING);
            responseObserver.onError(e);
        }
    }

    @Override
    public void healthCheck(Empty request, StreamObserver<HealthCheckReply> responseObserver) {
        super.healthCheck(request, responseObserver);
    }
}
