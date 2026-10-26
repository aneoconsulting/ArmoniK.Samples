package fr.aneo.armonik.worker.library.samples;

import fr.aneo.armonik.worker.domain.TaskContext;
import fr.aneo.armonik.worker.domain.TaskOutcome;
import fr.aneo.armonik.worker.domain.TaskProcessor;
import fr.aneo.armonik.worker.domain.definition.blob.InputBlobDefinition;
import fr.aneo.armonik.worker.domain.definition.task.TaskDefinition;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import static java.nio.charset.StandardCharsets.UTF_8;

/**
 * Computes S(lo, hi) = sum of i^2 for i in [lo, hi) by recursive splitting.
 * <p>
 * A task with inputs {@code lo}, {@code hi}, {@code grain} either computes the sum directly when
 * {@code hi - lo <= grain}, or submits three subtasks: two children on [lo, mid) and [mid, hi), and a
 * combine task that adds their results. The combine task takes over the parent's {@code result} output,
 * so the parent returns without writing it.
 * <p>
 * A task with inputs {@code left}, {@code right} is a combine task: it writes left + right to {@code result}.
 * <p>
 * Subtasks are given the worker library of the running task, so the dynamic worker loads this same class
 * for them.
 */
public class RangeSumProcessor implements TaskProcessor {
  private static final Logger logger = LoggerFactory.getLogger(RangeSumProcessor.class);

  @Override
  public TaskOutcome processTask(TaskContext taskContext) {
    if (taskContext.hasInput("left") && taskContext.hasInput("right")) {
      combine(taskContext);
      return TaskOutcome.SUCCESS;
    }
    if (taskContext.hasInput("lo") && taskContext.hasInput("hi") && taskContext.hasInput("grain")) {
      splitOrCompute(taskContext);
      return TaskOutcome.SUCCESS;
    }
    return new TaskOutcome.Error("Unexpected inputs: " + taskContext.inputs().keySet());
  }

  private static void splitOrCompute(TaskContext taskContext) {
    var lo = readLong(taskContext, "lo");
    var hi = readLong(taskContext, "hi");
    var grain = readLong(taskContext, "grain");

    if (hi - lo <= grain) {
      long sum = 0;
      for (var i = lo; i < hi; i++) {
        sum = Math.addExact(sum, Math.multiplyExact(i, i));
      }
      logger.info("Leaf [{}, {}) = {}", lo, hi, sum);
      taskContext.getOutput("result").write(Long.toString(sum), UTF_8);
      return;
    }

    var mid = lo + (hi - lo) / 2;
    logger.info("Split [{}, {}) at {}", lo, hi, mid);

    var library = taskContext.workerLibrary();
    // New blob shared by both children: upper bound of the left half, lower bound of the right half.
    var midBlob = taskContext.createBlob(InputBlobDefinition.from("mid", Long.toString(mid).getBytes(UTF_8)));

    // lo, hi and grain are the parent's inputs, reused without copy.
    var left = taskContext.submitTask(new TaskDefinition().withInput("lo", taskContext.getInput("lo"))
                                                          .withInput("hi", midBlob)
                                                          .withInput("grain", taskContext.getInput("grain"))
                                                          .withOutput("result")
                                                          .withWorkerLibrary(library));
    var right = taskContext.submitTask(new TaskDefinition().withInput("lo", midBlob)
                                                           .withInput("hi", taskContext.getInput("hi"))
                                                           .withInput("grain", taskContext.getInput("grain"))
                                                           .withOutput("result")
                                                           .withWorkerLibrary(library));

    // The combine task depends on the children's outputs and produces this task's output.
    // ArmoniK only starts it once both children have completed.
    taskContext.submitTask(new TaskDefinition().withInput("left", left.outputs().get("result"))
                                               .withInput("right", right.outputs().get("result"))
                                               .withOutput("result", taskContext.getOutput("result"))
                                               .withWorkerLibrary(library));
  }

  private static void combine(TaskContext taskContext) {
    var left = readLong(taskContext, "left");
    var right = readLong(taskContext, "right");
    var sum = Math.addExact(left, right);
    logger.info("Combine {} + {} = {}", left, right, sum);
    taskContext.getOutput("result").write(Long.toString(sum), UTF_8);
  }

  private static long readLong(TaskContext taskContext, String name) {
    return Long.parseLong(taskContext.getInput(name).asString(UTF_8).trim());
  }
}
