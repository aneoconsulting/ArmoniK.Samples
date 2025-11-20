package fr.aneo.armonik.worker.samples;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;
import fr.aneo.armonik.worker.domain.TaskContext;
import fr.aneo.armonik.worker.domain.TaskOutcome;
import fr.aneo.armonik.worker.domain.TaskProcessor;

import java.nio.charset.StandardCharsets;
import java.util.List;

import static java.nio.charset.StandardCharsets.*;

public class CalculatorProcessor implements TaskProcessor {

  private static final ObjectMapper MAPPER = new ObjectMapper();

  @Override
  public TaskOutcome processTask(TaskContext taskContext) {
    var operation = taskContext.getInput("operation").asString(UTF_8).toLowerCase();
    var json = taskContext.getInput("value").asString(UTF_8);
    List<Integer> nums;
    try {
      nums = MAPPER.readValue(json, new TypeReference<>() {
      });
    } catch (JsonProcessingException e) {
      throw new RuntimeException(e);
    }

    var result = switch (operation) {
      case "sum" -> nums.stream().mapToInt(Integer::intValue).sum();
      case "max" -> nums.stream().max(Integer::compare).orElse(0);
      case "min" -> nums.stream().min(Integer::compare).orElse(0);
      default -> throw new IllegalArgumentException("Unsupported operation: " + operation);
    };

    taskContext.getOutput("result").write(String.valueOf(result).getBytes(UTF_8));

    return TaskOutcome.SUCCESS;
  }
}
