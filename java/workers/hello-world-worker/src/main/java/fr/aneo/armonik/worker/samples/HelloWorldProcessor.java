package fr.aneo.armonik.worker.samples;

import fr.aneo.armonik.worker.domain.TaskContext;
import fr.aneo.armonik.worker.domain.TaskOutcome;
import fr.aneo.armonik.worker.domain.TaskProcessor;

import static java.nio.charset.StandardCharsets.*;

public class HelloWorldProcessor implements TaskProcessor {
  @Override
  public TaskOutcome processTask(TaskContext taskContext) {
    // Read input named "name"
    var name = taskContext.getInput("name").asString(UTF_8);

    // Compute result
    var result = "Hello " + name + ". Welcome to Armonik Java Worker !!";

    // Write output named "greeting"
    taskContext.getOutput("greeting").write(result.getBytes(UTF_8));

    return TaskOutcome.SUCCESS;
  }
}
