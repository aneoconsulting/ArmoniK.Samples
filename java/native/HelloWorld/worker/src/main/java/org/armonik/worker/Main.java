package org.armonik.worker;

import java.io.File;
import java.io.IOException;
import java.net.InetAddress;
import java.util.concurrent.ExecutionException;

import com.google.common.util.concurrent.ListenableFuture;

import armonik.api.grpc.v1.worker.WorkerCommon.HealthCheckReply;
import armonik.api.grpc.v1.worker.WorkerGrpc;
//import armonik.worker.Worker;
import io.grpc.ManagedChannel;
import io.grpc.netty.NegotiationType;
import io.grpc.netty.NettyChannelBuilder;
import io.netty.channel.epoll.EpollDomainSocketChannel;
import io.netty.channel.epoll.EpollEventLoopGroup;
import io.netty.channel.unix.DomainSocketAddress;

public class Main {

    public static void main(String[] args) throws IOException, InterruptedException, ExecutionException {
        String workerAddress = System.getenv("ComputePlane__WorkerChannel__Address");
        ManagedChannel channel = NettyChannelBuilder
                .forAddress(new DomainSocketAddress(new File("/tmp/sockets/worker.sock")))
                .channelType(EpollDomainSocketChannel.class)
                .overrideAuthority(InetAddress.getLocalHost().getHostAddress())
                .eventLoopGroup(new EpollEventLoopGroup()).usePlaintext()
                .negotiationType(NegotiationType.PLAINTEXT)
                .keepAliveWithoutCalls(true)
                .build();
        WorkerGrpc.WorkerFutureStub workerStub = WorkerGrpc.newFutureStub(channel);
        armonik.api.grpc.v1.Objects.Empty request = armonik.api.grpc.v1.Objects.Empty.newBuilder().build();
        ListenableFuture<HealthCheckReply> response = workerStub.healthCheck(request); // .healthCheck(Empty.newBuilder().build());
        System.out.println(response.get());
        channel.shutdown();
        // Creating a FutureWorker instance to handle tasks asynchronously
        // var worker = new FutureWorker(managedChannel, taskHandler -> {
        // System.out.println(">> TASK HANDLER IS BEING EXECUTED...");
        // });
        // var worker1 = new FutureWorker(managedChannel, null);
        // worker.process(null, null);
        // worker.start("172.30.37.125:5001");

        // Processing a request and defining response handling logic
        // worker.process(WorkerCommon.ProcessRequest.newBuilder().build(), new
        // StreamObserver<>() {

        // @Override
        // public void onNext(WorkerCommon.ProcessReply processReply) {
        // System.out.println(">> REPLY:");
        // System.out.println(processReply);
        // }

        // @Override
        // public void onError(Throwable throwable) {
        // System.out.println(">> ERROR/");
        // System.out.println(throwable.getMessage());
        // }

        // @Override
        // public void onCompleted() {
        // System.out.println("COMPLETEDIO");
        // }
        // });
    }

}
