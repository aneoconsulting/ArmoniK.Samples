package fr.aneo.armonik.client.samples;

import fr.aneo.armonik.client.ArmoniKClient;
import fr.aneo.armonik.client.ArmoniKConfig;
import fr.aneo.armonik.client.BlobCompletionListener;
import fr.aneo.armonik.client.TaskConfiguration;
import fr.aneo.armonik.client.WorkerLibrary;
import fr.aneo.armonik.client.definition.SessionDefinition;
import fr.aneo.armonik.client.definition.TaskDefinition;
import fr.aneo.armonik.client.definition.blob.InputBlobDefinition;

import java.nio.file.Paths;
import java.util.Set;
import java.util.concurrent.atomic.AtomicReference;

import static java.nio.charset.StandardCharsets.UTF_8;

/**
 * Submits one task computing the sum of i^2 for i in [1, N] to the Java dynamic worker.
 * The worker library (range-sum-library) splits the range recursively by submitting subtasks;
 * the client only waits for the root result.
 * <p>
 * System properties: {@code endpoint} (default http://localhost:5001), {@code partition} (default javadynamic),
 * {@code library} (path to range-sum-library.zip), {@code n} (default 1000), {@code grain} (default 100).
 */
public class RangeSum {

  public static void main(String[] args) {
    var endpoint = System.getProperty("endpoint", "http://localhost:5001");
    var partition = System.getProperty("partition", "javadynamic");
    var libraryZip = System.getProperty("library", "../worker-libraries/range-sum-library/target/range-sum-library.zip");
    var n = Long.parseLong(System.getProperty("n", "1000"));
    var grain = Long.parseLong(System.getProperty("grain", "100"));

    var config = ArmoniKConfig.builder()
                              .endpoint(endpoint)
                              .withoutSslValidation()
                              .build();

    var result = new AtomicReference<String>();
    var listener = new BlobCompletionListener() {
      @Override
      public void onSuccess(Blob blob) {
        result.set(new String(blob.data(), UTF_8));
      }

      @Override
      public void onError(BlobError blobError) {
        System.out.println("Blob error - id: " + blobError.blobInfo().id().asString() +
                           ", message: " + blobError.cause().getMessage());
      }
    };

    try (var client = new ArmoniKClient(config)) {
      var sessionDefinition = new SessionDefinition(
        Set.of(partition),
        TaskConfiguration.defaultConfigurationWithPartition(partition),
        listener
      );
      var session = client.openSession(sessionDefinition);

      var libraryBlob = session.createBlob(InputBlobDefinition.from(Paths.get(libraryZip).toFile()));
      var rangeSumLibrary = new WorkerLibrary(
        "range-sum-library-1.0.0-SNAPSHOT.jar",
        "fr.aneo.armonik.worker.library.samples.RangeSumProcessor",
        libraryBlob
      );

      var taskDefinition = new TaskDefinition().withWorkerLibrary(rangeSumLibrary)
                                               .withInput("lo", InputBlobDefinition.from("1".getBytes(UTF_8)))
                                               .withInput("hi", InputBlobDefinition.from(Long.toString(n + 1).getBytes(UTF_8)))
                                               .withInput("grain", InputBlobDefinition.from(Long.toString(grain).getBytes(UTF_8)))
                                               .withOutput("result");

      session.submitTask(taskDefinition);
      session.awaitOutputsProcessed();
    }

    var expected = n * (n + 1) * (2 * n + 1) / 6;
    System.out.println("Result: " + result.get() + ", expected: " + expected);
    if (!Long.toString(expected).equals(result.get())) {
      throw new IllegalStateException("Wrong result " + result.get() + ", expected " + expected);
    }
  }
}
