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
import java.util.Map;
import java.util.Set;

import static java.nio.charset.StandardCharsets.UTF_8;

/**
 * Demonstrates dynamic library loading for a C++ worker: upload a .so at runtime and execute tasks with it.
 * The library is uploaded as a blob and tasks reference it via the LibraryBlobId task option.
 * Requires the "cppdynamic" partition with the ArmoniK C++ DynamicWorker image.
 */
public class CppDynamicLibrary {

  public static void main(String[] args) {
    var config = ArmoniKConfig.builder()
                              .endpoint("http://localhost:5001")
                              .withoutSslValidation()
                              .build();

    String partition = System.getProperty("fr.aneo.armonik.client.samples.partition", "cppdynamic");

    try (var client = new ArmoniKClient(config)) {
      var sessionDefinition = new SessionDefinition(
        Set.of(partition),
        TaskConfiguration.defaultConfigurationWithPartition(partition),
        new SimpleBlobListener()
      );
      var session = client.openSession(sessionDefinition);

      // Upload the worker .so as a blob
      // Hint path: cpp/MultiplyProcessor/worker/build/libArmoniK.Samples.Cpp.MultiplyProcessor.Worker.so
      String cppMultiplyProcessorPath = System.getProperty("fr.aneo.armonik.client.samples.dynlib.path");
      var libraryBlob = session.createBlob(
        InputBlobDefinition.from(Paths.get(cppMultiplyProcessorPath).toFile())
      );

      // WorkerLibrary ensures the blob is added to the task's data dependencies so the C++ worker
      // can fetch it at runtime. path is the .so filename (ignored when LibraryBlobId is set);
      // symbol is the armonik_* function prefix used by dlsym (empty string defaults to "armonik").
      var cppWorkerLibrary = new WorkerLibrary(
        "libArmoniK.Samples.Cpp.MultiplyProcessor.Worker.so",
        "armonik",
        libraryBlob
      );

      // MethodName tells the C++ worker which function to call inside the .so
      var taskConfig = TaskConfiguration.defaultConfigurationWithPartition(partition)
                                        .withOptions(Map.of("MethodName", "multiply"));

      // Submit task using the C++ convention: the worker fetches the .so by LibraryBlobId,
      // dlopen()s it, and calls the function named by MethodName.
      var taskDefinition = new TaskDefinition()
        .withWorkerLibrary(cppWorkerLibrary)
        .withConfiguration(taskConfig)
        .withInput("num1", InputBlobDefinition.from("2".getBytes(UTF_8)))
        .withInput("num2", InputBlobDefinition.from("3".getBytes(UTF_8)))
        .withOutput("result");

      session.submitTask(taskDefinition);
      session.awaitOutputsProcessed();
      session.close();
    }
  }
}
