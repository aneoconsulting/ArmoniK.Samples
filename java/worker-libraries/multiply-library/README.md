# Multiply Library - Java SDK Dynamic Loading Sample

This sample demonstrates how to create a dynamically loaded library for ArmoniK using the Java SDK. The library contains a `MultiplyProcessor` that multiplies two numbers.

Unlike the [Hello World Worker](../../workers/hello-world-worker/README.md) sample where you build your own worker container, this approach uses ArmoniK's **dynamic library loading** feature. The library JAR is uploaded as a blob and loaded at runtime by a generic Java dynamic worker.

## Prerequisites

- Java 17 or higher
- Maven 3.9+
- A running ArmoniK cluster with the Java dynamic worker partition (see [Deploying to ArmoniK](#deploying-to-armonik))

## Project Structure

```
multiply-library/
├── pom.xml
└── src/main/java/fr/aneo/armonik/worker/library/samples/
    └── MultiplyProcessor.java
```

## How It Works

The `MultiplyProcessor` implements the `TaskProcessor` interface:

```java
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
```

### Dynamic Loading Workflow

1. The library is packaged as a **ZIP file** containing the shaded JAR
2. The client uploads the ZIP as a **blob** to ArmoniK
3. When submitting tasks, the client specifies:
  - `LibraryBlobId`: The blob ID of the uploaded ZIP
  - `LibraryName`: The JAR filename inside the ZIP
  - `ClassName`: The fully qualified class name of the `TaskProcessor` implementation
4. The generic Java dynamic worker loads the library at runtime and executes tasks

## Building the Library

### Build the JAR and ZIP

```bash
./mvnw clean package
```

This command:
1. Compiles the Java code
2. Creates a shaded JAR with all dependencies
3. Packages the JAR into a ZIP file: `target/multiply-library.zip`

### Verify the Build Output

```bash
ls -la target/multiply-library.zip
unzip -l target/multiply-library.zip
```

Expected output:
```
Archive:  target/multiply-library.zip
  Length      Date    Time    Name
---------  ---------- -----   ----
    XXXXX  YYYY-MM-DD HH:MM   multiply-library-1.0.0-SNAPSHOT.jar
---------                     -------
    XXXXX                     1 file
```

## Deploying to ArmoniK

### Step 1: Deploy ArmoniK Locally

Follow the [ArmoniK Local Deployment Guide](https://armonik.readthedocs.io/en/latest/content/getting-started/installation/local.html) to set up a local ArmoniK cluster.

### Step 2: Create a Partition for the Java Dynamic Worker

ArmoniK provides a pre-built Java dynamic worker that can load libraries at runtime. You need to create a partition that uses this worker image.

Edit the `partitions.tfvars` file in your ArmoniK deployment directory (typically `infrastructure/quick-deploy/localhost/`):

```hcl
compute_plane = {
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

### Step 3: Apply the Partition Configuration

Redeploy ArmoniK to apply the new partition:

```bash
cd infrastructure/quick-deploy/localhost
make deploy
```

### Step 4: Verify the Partition

Open the ArmoniK Admin GUI in your browser (typically at `http://localhost:5000/admin`) and navigate to the **Partitions** section. You should see your `javadynamic` partition listed with its configuration.

You can also check the partition details to verify:
- The worker image is correctly set to `dockerhubaneo/armonik-dynamic-java-worker:latest`
- The resource limits and requests are as configured

## Running a Client

To test the library, use the Java Client sample that demonstrates dynamic library loading.

See the [Java Client Sample](../../client/README.md) for detailed instructions, particularly the `DynamicLibrary` example that shows how to:
1. Upload the library ZIP as a blob
2. Submit tasks with the dynamic loading configuration
3. Retrieve the computation results

## Learn More

- [ArmoniK Documentation](https://armonik.readthedocs.io/)
- [ArmoniK Java SDK](https://github.com/aneoconsulting/ArmoniK.Extensions.Java)
- [Partition Configuration Guide](https://armonik.readthedocs.io/en/latest/content/user-guide/how-to-configure-partitions.html)
- [ArmoniK Samples Repository](https://github.com/aneoconsulting/ArmoniK.Samples)
