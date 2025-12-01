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
 * Demonstrates task dependencies: a task can use outputs from previous tasks as inputs.
 * This creates a DAG where task3 waits for task1 and task2 to complete.
 * <p>
 * Task graph:
 *   task1 (1+2=3) ──┐
 *                   ├──► task3 (3+7=10)
 *   task2 (3+4=7) ──┘
 * <p>
 * Requires a "sum" partition with a worker that adds two numbers.
 */
public class TaskDependencies {

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

      // First level: two independent tasks
      var task1Definition = new TaskDefinition().withInput("num1", InputBlobDefinition.from("1".getBytes(UTF_8)))
                                                .withInput("num2", InputBlobDefinition.from("2".getBytes(UTF_8)))
                                                .withOutput("result");

      var task2Definition = new TaskDefinition().withInput("num1", InputBlobDefinition.from("3".getBytes(UTF_8)))
                                                .withInput("num2", InputBlobDefinition.from("4".getBytes(UTF_8)))
                                                .withOutput("result");

      var task1 = session.submitTask(task1Definition);
      var task2 = session.submitTask(task2Definition);

      // Second level: depends on outputs from task1 and task2
      var task3Definition = new TaskDefinition().withInput("num1", task1.outputs().get("result"))
                                                .withInput("num2", task2.outputs().get("result"))
                                                .withOutput("result");

      session.submitTask(task3Definition);
      session.awaitOutputsProcessed();
    }
  }
}
