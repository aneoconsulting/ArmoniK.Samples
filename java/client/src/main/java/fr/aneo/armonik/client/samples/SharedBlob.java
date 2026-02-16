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
 * Demonstrates reusing a blob across multiple tasks.
 * The shared blob is uploaded once and referenced by multiple task definitions.
 * Requires a "sum" partition with a worker that adds two numbers.
 */
public class SharedBlob {

  public static void main(String[] args) {
    var config = ArmoniKConfig.builder()
                              .endpoint("http://localhost:5001")
                              .withoutSslValidation()
                              .build();

    try (var client = new ArmoniKClient(config)) {
      var sessionDefinition = new SessionDefinition(
        Set.of("sum"),
        TaskConfiguration.defaultConfigurationWithPartition("sum"),
        new SimpleBlobListener()
      );
      var session = client.openSession(sessionDefinition);

      // Create a blob that can be shared across multiple tasks
      var sharedBlob = session.createBlob(InputBlobDefinition.from("3".getBytes(UTF_8)));

      // Use the shared blob as input (avoids uploading the same data twice)
      var sum1Definition = new TaskDefinition().withInput("num1", InputBlobDefinition.from("1".getBytes(UTF_8)))
                                               .withInput("num2", sharedBlob)
                                               .withOutput("result");

      var sum2Definition = new TaskDefinition().withInput("num1", InputBlobDefinition.from("2".getBytes(UTF_8)))
                                               .withInput("num2", sharedBlob)
                                               .withOutput("result");

      session.submitTask(sum1Definition);
      session.submitTask(sum2Definition);

      session.awaitOutputsProcessed();
    }
  }
}
