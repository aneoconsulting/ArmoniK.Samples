package fr.aneo.armonik.client.samples;

import fr.aneo.armonik.client.ArmoniKClient;
import fr.aneo.armonik.client.ArmoniKConfig;
import fr.aneo.armonik.client.TaskConfiguration;
import fr.aneo.armonik.client.definition.SessionDefinition;
import fr.aneo.armonik.client.definition.TaskDefinition;
import fr.aneo.armonik.client.definition.blob.InputBlobDefinition;
import fr.aneo.armonik.client.samples.listener.SimpleBlobListener;

import java.util.Set;

import static java.nio.charset.StandardCharsets.UTF_8;

/**
 * Basic example: submit a single task with inline input and wait for output.
 * Requires the "helloworld" partition with the hello-world-worker image.
 */
public class HelloWorld {

  public static void main(String[] args) {
    var config = ArmoniKConfig.builder()
                              .endpoint("http://localhost:5001")
                              .withoutSslValidation()
                              .build();

    try (var client = new ArmoniKClient(config)) {
      // Create session targeting the "helloworld" partition
      var sessionDefinition = new SessionDefinition(
        Set.of("helloworld"),
        TaskConfiguration.defaultConfigurationWithPartition("helloworld"),
        new SimpleBlobListener()
      );
      var session = client.openSession(sessionDefinition);

      // Define task with inline input and expected output
      var taskDefinition = new TaskDefinition().withInput("name", InputBlobDefinition.from("John".getBytes(UTF_8)))
                                               .withOutput("greeting");

      session.submitTask(taskDefinition);
      session.awaitOutputsProcessed();
    }
  }
}
