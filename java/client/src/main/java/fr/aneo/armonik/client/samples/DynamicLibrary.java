package fr.aneo.armonik.client.samples;

import fr.aneo.armonik.client.ArmoniKClient;
import fr.aneo.armonik.client.ArmoniKConfig;
import fr.aneo.armonik.client.TaskConfiguration;
import fr.aneo.armonik.client.WorkerLibrary;
import fr.aneo.armonik.client.definition.SessionDefinition;
import fr.aneo.armonik.client.definition.TaskDefinition;
import fr.aneo.armonik.client.definition.blob.InputBlobDefinition;
import fr.aneo.armonik.client.samples.listener.SimpleBlobListener;

import java.nio.file.Paths;
import java.util.Set;

import static java.nio.charset.StandardCharsets.UTF_8;

/**
 * Demonstrates dynamic library loading: upload a JAR at runtime and execute tasks with it.
 * The library ZIP is uploaded as a blob, and tasks reference it via WorkerLibrary.
 * Requires the "javadynamic" partition with the armonik-dynamic-java-worker image.
 */
public class DynamicLibrary {

  public static void main(String[] args) {
    var config = ArmoniKConfig.builder()
                              .endpoint("http://localhost:5001")
                              .withoutSslValidation()
                              .build();

    try (var client = new ArmoniKClient(config)) {
      var sessionDefinition = new SessionDefinition(
        Set.of("javadynamic"),
        TaskConfiguration.defaultConfigurationWithPartition("javadynamic"),
        new SimpleBlobListener()
      );
      var session = client.openSession(sessionDefinition);

      // Upload the library ZIP as a blob
      // Copy from: worker-libraries/multiply-library/target/multiply-library.zip
      var libraryBlob = session.createBlob(InputBlobDefinition.from(Paths.get("src/main/resources/multiply-library.zip").toFile()));

      // Define which class to load from the library
      var multiplyLibrary = new WorkerLibrary(
        "multiply-library-1.0.0-SNAPSHOT.jar",
        "fr.aneo.armonik.worker.library.samples.MultiplyProcessor",
        libraryBlob
      );

      // Submit task using the dynamically loaded library
      var taskDefinition = new TaskDefinition().withWorkerLibrary(multiplyLibrary)
                                               .withInput("num1", InputBlobDefinition.from("2".getBytes(UTF_8)))
                                               .withInput("num2", InputBlobDefinition.from("3".getBytes(UTF_8)))
                                               .withOutput("result");

      session.submitTask(taskDefinition);
      session.awaitOutputsProcessed();
    }
  }
}
