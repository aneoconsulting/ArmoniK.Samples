package fr.aneo.armonik.worker.library.samples;

import fr.aneo.armonik.worker.domain.TaskContext;
import fr.aneo.armonik.worker.domain.TaskOutcome;
import fr.aneo.armonik.worker.domain.TaskProcessor;

import static java.nio.charset.StandardCharsets.UTF_8;

public class MultiplyProcessor implements TaskProcessor {

  @Override
  public TaskOutcome processTask(TaskContext taskContext) {
    // Read inputs
    var num1 = Integer.parseInt(taskContext.getInput("num1").asString(UTF_8));
    var num2 = Integer.parseInt(taskContext.getInput("num2").asString(UTF_8));

    // Compute result
    var result = num1 * num2;

    // Write output named "result"
    taskContext.getOutput("result").write(String.valueOf(result), UTF_8);

    return TaskOutcome.SUCCESS;
  }
}
