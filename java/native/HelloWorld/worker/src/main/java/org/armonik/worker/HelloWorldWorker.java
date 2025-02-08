package org.armonik.worker;

import java.io.IOException;
import java.net.MalformedURLException;
import java.net.URISyntaxException;
import java.net.UnknownHostException;
import java.nio.charset.StandardCharsets;
import java.util.logging.Logger;

import armonik.api.grpc.v1.Objects.Empty;
import armonik.api.grpc.v1.Objects.Output;
import armonik.worker.FutureWorker;
import armonik.worker.GrpcWorkerServer;
import armonik.worker.taskhandlers.FutureTaskHandler;

public class HelloWorldWorker extends FutureWorker {
    private static final Logger logger = Logger.getLogger(HelloWorldWorker.class.getName());

    HelloWorldWorker() throws UnknownHostException, MalformedURLException, URISyntaxException {
        super();
    }

    public static void main(String[] args) throws IOException, InterruptedException, URISyntaxException {

        var workerAddress = System.getenv("ComputePlane__WorkerChannel__Address");

        GrpcWorkerServer server = new GrpcWorkerServer();
        server.start(workerAddress, new HelloWorldWorker());
        server.blockUntilShutdown();
    }

    @Override
    public Output processInternal(FutureTaskHandler futureTaskHandler) {
        try {

            logger.info("Request received for the worker");
            String input = new String(futureTaskHandler.getPayload(), StandardCharsets.UTF_8);
            String resultId = futureTaskHandler.getExpectedResults().get(0);
            String result = input + " World_";
            futureTaskHandler.notifyResultData(resultId, result.getBytes()).get();

        } catch (Exception e) {
            e.printStackTrace();
            Thread.currentThread().interrupt();
            return Output.newBuilder().setError(Output.Error.newBuilder()
                    .setDetails(e.getMessage())
                    .build()).build();

        }
        Output emptyOutput = Output.newBuilder()
                .setOk(Empty.newBuilder().build()).build();
        return emptyOutput;
    }
}
