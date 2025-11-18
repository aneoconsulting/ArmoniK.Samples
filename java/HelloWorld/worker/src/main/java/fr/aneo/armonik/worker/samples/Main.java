package fr.aneo.armonik.worker.samples;

import fr.aneo.armonik.worker.ArmoniKWorker;

import java.io.IOException;

public class Main {

  public static void main(String[] args) throws IOException, InterruptedException {

    var armoniKWorker = ArmoniKWorker.withTaskProcessor(new HelloWorldProcessor());
    armoniKWorker.start();
    armoniKWorker.blockUntilShutdown();
  }
}
