# Hello World Worker - Java SDK Sample

This sample demonstrates how to create a simple ArmoniK worker using the Java SDK. The worker receives a name as input and returns a greeting message.

## Prerequisites

- Java 17 or higher
- Maven 3.9+
- Docker
- A running ArmoniK cluster (see [Local Deployment](https://armonik.readthedocs.io/en/latest/content/getting-started/installation/local.html))

## Project Structure

```
sum-worker/
├── pom.xml
└── src/main/java/fr/aneo/armonik/worker/samples/
    ├── SumProcessor.java
    └── Main.java
```

## How It Works

The `SumProcessor` implements the `TaskProcessor` interface from the ArmoniK Java Worker SDK:

```java
public class SumProcessor implements TaskProcessor {

  @Override
  public TaskOutcome processTask(TaskContext taskContext) {
    // Read inputs
    var num1 = Integer.parseInt(taskContext.getInput("num1").asString(UTF_8));
    var num2 = Integer.parseInt(taskContext.getInput("num2").asString(UTF_8));

    // Compute result
    var result = num1 + num2;

    // Write output named "result"
    taskContext.getOutput("result").write(String.valueOf(result), UTF_8);

    return TaskOutcome.SUCCESS;
  }
}
```

The `Main` class bootstraps the worker server:

```java
public static void main(String[] args) throws IOException, InterruptedException {
    var armoniKWorker = ArmoniKWorker.withTaskProcessor(new HelloWorldProcessor());
    armoniKWorker.start();
    armoniKWorker.blockUntilShutdown();
}
```

## Building the Worker

### Build the JAR and Docker Image

```bash
./mvnw clean package
```

This command:
1. Compiles the Java code
2. Creates a shaded JAR with all dependencies
3. Builds a Docker image named `sum-worker:latest` using Jib

### Verify the Docker Image

```bash
docker images | grep sum-worker
```

Expected output:
```
sum-worker   1.0.0-SNAPSHOT   <image-id>   <date>   <size>
sum-worker   latest           <image-id>   <date>   <size>
```

## Deploying to ArmoniK

### Step 1: Deploy ArmoniK Locally

Follow the [ArmoniK Local Deployment Guide](https://armonik.readthedocs.io/en/latest/content/getting-started/installation/local.html) to set up a local ArmoniK cluster.

### Step 2: Create a Partition for the Java Worker

ArmoniK uses partitions to group workers with specific configurations. You need to create a partition that references your `sum-worker` Docker image.

Edit the `partitions.tfvars` file in your ArmoniK deployment directory (typically `infrastructure/quick-deploy/localhost/`):

```hcl
compute_plane = {
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

  # existing partitions
  default = {
    # ... existing default configuration
  }

}
```

### Step 3: Apply the Partition Configuration

Redeploy ArmoniK to apply the new partition:

```bash
cd infrastructure/quick-deploy/localhost
make deploy
```

### Step 4: Verify the Partition

Open the ArmoniK Admin GUI in your browser (typically at `http://localhost:5000/admin`) and navigate to the **Partitions** section. You should see your `helloworld` partition listed with its configuration.

## Running a Client

To test the worker, use the Java Client sample that submits tasks to the `sum` partition.

See the [Java Client Sample](../../client/README.md) for detailed instructions on how to run a client that interacts with this worker.

## Learn More

- [ArmoniK Documentation](https://armonik.readthedocs.io/)
- [ArmoniK Java SDK](https://github.com/aneoconsulting/ArmoniK.Extensions.Java)
- [Partition Configuration Guide](https://armonik.readthedocs.io/en/latest/content/user-guide/how-to-configure-partitions.html)
- [ArmoniK Samples Repository](https://github.com/aneoconsulting/ArmoniK.Samples)
