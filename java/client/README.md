# Java Client Samples

This project demonstrates how to use the ArmoniK Java Client SDK to submit tasks and retrieve results.

## Prerequisites

- Java 17 or higher
- Maven 3.9+
- A running ArmoniK cluster (see [Local Deployment](https://armonik.readthedocs.io/en/latest/content/getting-started/installation/local.html))
- Deployed worker partitions (see [Partition Setup](#partition-setup))

## Project Structure

```
client/
├── pom.xml
└── src/main/java/fr/aneo/armonik/client/samples/
    ├── listener/
    │   └── SimpleBlobListener.java
    ├── HelloWorld.java
    ├── SharedBlob.java
    ├── TaskDependencies.java
    └── DynamicLibrary.java
```

## Samples Overview

| Sample | Description | Required Partition |
|--------|-------------|-------------------|
| `HelloWorld` | Basic task submission with inline input | `helloworld` |
| `SharedBlob` | Reuse a blob across multiple tasks | `sum` |
| `TaskDependencies` | Chain tasks using outputs as inputs | `sum` |
| `DynamicLibrary` | Load a library JAR at runtime | `javadynamic` |

## Partition Setup

Before running the samples, you need to configure the required partitions in your ArmoniK deployment.

Edit the `partitions.tfvars` file in your ArmoniK deployment directory (typically `infrastructure/quick-deploy/localhost/`):

```hcl
compute_plane = {
  # partition for HelloWorld sample
  helloworld = {
    replicas    = 0
    socket_type = "tcp"
    polling_agent = {
      limits   = { cpu = "2000m", memory = "2048Mi" }
      requests = { cpu = "50m", memory = "50Mi" }
    }
    worker = [{
      image    = "hello-world-worker"
      tag      = "latest"
      limits   = { cpu = "1000m", memory = "1024Mi" }
      requests = { cpu = "50m", memory = "50Mi" }
    }]
    hpa = {
      type              = "prometheus"
      polling_interval  = 15
      cooldown_period   = 300
      min_replica_count = 0
      max_replica_count = 5
      behavior = {
        restore_to_original_replica_count = true
        stabilization_window_seconds      = 300
        type                              = "Percent"
        value                             = 100
        period_seconds                    = 15
      }
      triggers = [
        { type = "prometheus", threshold = 2 }
      ]
    }
  },
  # partition for sum worker
  sum = {
    replicas    = 0
    socket_type = "tcp"
    polling_agent = {
      limits   = { cpu = "2000m", memory = "2048Mi" }
      requests = { cpu = "50m", memory = "50Mi" }
    }
    worker = [{
      image    = "sum-worker"
      tag      = "latest"
      limits   = { cpu = "1000m", memory = "1024Mi" }
      requests = { cpu = "50m", memory = "50Mi" }
    }]
    hpa = {
      type              = "prometheus"
      polling_interval  = 15
      cooldown_period   = 300
      min_replica_count = 0
      max_replica_count = 5
      behavior = {
        restore_to_original_replica_count = true
        stabilization_window_seconds      = 300
        type                              = "Percent"
        value                             = 100
        period_seconds                    = 15
      }
      triggers = [
        { type = "prometheus", threshold = 2 }
      ]
    }
  },
  # partition for Worker Library sample (uses pre-built image from Docker Hub)
  javadynamic = {
    replicas    = 0
    socket_type = "tcp"
    polling_agent = {
      limits   = { cpu = "2000m", memory = "2048Mi" }
      requests = { cpu = "50m", memory = "50Mi" }
    }
    worker = [{
      image    = "dockerhubaneo/armonik-dynamic-java-worker"
      tag      = "latest"
      limits   = { cpu = "1000m", memory = "1024Mi" }
      requests = { cpu = "50m", memory = "50Mi" }
    }]
    hpa = {
      type              = "prometheus"
      polling_interval  = 15
      cooldown_period   = 300
      min_replica_count = 0
      max_replica_count = 5
      behavior = {
        restore_to_original_replica_count = true
        stabilization_window_seconds      = 300
        type                              = "Percent"
        value                             = 100
        period_seconds                    = 15
      }
      triggers = [
        { type = "prometheus", threshold = 2 }
      ]
    }
  },

  # existing partitions
  default = {
    # ... existing default configuration
  }
}
```

Apply the configuration:

```bash
cd infrastructure/quick-deploy/localhost
make deploy
```

Verify partitions are created in the ArmoniK Admin GUI at `http://localhost:5000/admin`.

## Building the Client

```bash
./mvnw clean package
```

## Running the Samples

### HelloWorld

Submits a single task with an inline input and waits for the greeting output.

```bash
./mvnw compile exec:java -Dexec.mainClass="fr.aneo.armonik.client.samples.HelloWorld"
```

Expected output:
```
Blob completed - id: <blob-id>, data: Hello John. Welcome to Armonik Java Worker !!
```

### SharedBlob

Demonstrates creating a blob once and reusing it across multiple task inputs.

```bash
./mvnw compile exec:java -Dexec.mainClass="fr.aneo.armonik.client.samples.SharedBlob"
```

Expected output:
```
Blob completed - id: <blob-id>, data: 4
Blob completed - id: <blob-id>, data: 5
```

### TaskDependencies

Creates a task graph where a third task depends on the outputs of two previous tasks:

```
task1 (1+2=3) ──┐
                ├──► task3 (3+7=10)
task2 (3+4=7) ──┘
```

```bash
./mvnw compile exec:java -Dexec.mainClass="fr.aneo.armonik.client.samples.TaskDependencies"
```

Expected output:
```
Blob completed - id: <blob-id>, data: 3
Blob completed - id: <blob-id>, data: 7
Blob completed - id: <blob-id>, data: 10
```

### DynamicLibrary

Uploads a library ZIP at runtime and executes tasks using it.

**Setup:** Copy the multiply library ZIP to the resources folder:

```bash
cp ../worker-libraries/multiply-library/target/multiply-library.zip src/main/resources/
```

**Run:**

```bash
./mvnw compile exec:java -Dexec.mainClass="fr.aneo.armonik.client.samples.DynamicLibrary"
```

Expected output:
```
Blob completed - id: <blob-id>, data: 6
```

## Key Concepts

### ArmoniKClient

The entry point for interacting with the ArmoniK cluster:

```java
var config = ArmoniKConfig.builder()
                          .endpoint("http://localhost:5001")
                          .withoutSslValidation()
                          .build();

try (var client = new ArmoniKClient(config)) {
    // Use client...
}
```

### SessionDefinition

Defines a session with target partitions, default task configuration, and a completion listener:

```java
var sessionDefinition = new SessionDefinition(
    Set.of("partition-name"),
    TaskConfiguration.defaultConfigurationWithPartition("partition-name"),
    new SimpleBlobListener()
);
var session = client.openSession(sessionDefinition);
```

### TaskDefinition

Defines a task with inputs and outputs:

```java
var taskDefinition = new TaskDefinition()
    .withInput("inputName", InputBlobDefinition.from("data".getBytes(UTF_8)))
    .withOutput("outputName");

session.submitTask(taskDefinition);
```

### WorkerLibrary (Dynamic Loading)

Specifies a library to load at runtime:

```java
var libraryBlob = session.createBlob(InputBlobDefinition.from(zipFile));

var library = new WorkerLibrary(
    "library.jar",                    // JAR filename inside ZIP
    "com.example.MyProcessor",        // Fully qualified class name
    libraryBlob                       // Blob containing the ZIP
);

var taskDefinition = new TaskDefinition()
    .withWorkerLibrary(library)
    .withInput("num1", InputBlobDefinition.from("2".getBytes(UTF_8)))
    .withOutput("result");
```

### BlobCompletionListener

Receives notifications when output blobs are completed:

```java
public class SimpleBlobListener implements BlobCompletionListener {
    @Override
    public void onSuccess(Blob blob) {
        System.out.println("Data: " + new String(blob.data(), UTF_8));
    }

    @Override
    public void onError(BlobError blobError) {
        System.out.println("Error: " + blobError.cause().getMessage());
    }
}
```

## Learn More

- [ArmoniK Documentation](https://armonik.readthedocs.io/)
- [ArmoniK Java SDK](https://github.com/aneoconsulting/ArmoniK.Extensions.Java)
- [Partition Configuration Guide](https://armonik.readthedocs.io/en/latest/content/user-guide/how-to-configure-partitions.html)
- [Hello World Worker Sample](../workers/hello-world-worker/README.md)
- [Sum Worker Sample](../workers/sum-worker/README.md)
- [Multiply Library Sample](../worker-libraries/multiply-library/README.md)
- [ArmoniK Samples Repository](https://github.com/aneoconsulting/ArmoniK.Samples)
