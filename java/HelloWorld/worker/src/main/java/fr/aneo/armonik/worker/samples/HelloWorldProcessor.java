package fr.aneo.armonik.worker.samples;

import fr.aneo.armonik.worker.domain.TaskContext;
import fr.aneo.armonik.worker.domain.TaskOutcome;
import fr.aneo.armonik.worker.domain.TaskProcessor;

import static java.nio.charset.StandardCharsets.*;

public class HelloWorldProcessor implements TaskProcessor {
  @Override
  public TaskOutcome processTask(TaskContext taskContext) {
    var name = taskContext.getInput("name").asString(UTF_8);
    var result = "Hello " + name + ". Welcome to Armonik Java Worker !!";
    taskContext.getOutput("greeting").write(result.getBytes(UTF_8));

    return TaskOutcome.SUCCESS;
  }
}
