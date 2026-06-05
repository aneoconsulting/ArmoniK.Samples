# Chained Arithmetic Example using ArmoniK's C++ SDK

This project demonstrates task chaining using the ArmoniK C++ SDK. The client submits two independent `multiply` tasks (3×3 and 4×4), waits for their results, then submits an `add` task whose inputs are the blob outputs of the multiply tasks. The expected final result is 3×3 + 4×4 = 25.

The worker is compiled as a shared library loaded dynamically by the ArmoniK DynamicWorker. It exposes two methods: `multiply` and `add`.

## Prerequisites

An ArmoniK cluster with a partition for the dynamic C++ worker provided by the SDK.
If you already deployed ArmoniK and wish to add such a partition, follow these steps:

1. Add a `cppdynamic` partition to your infrastructure.

    ```diff
    +cppdynamic = {
    +  # number of replicas for each deployment of compute plane
    +  replicas = 0
    +  # ArmoniK polling agent
    +  polling_agent = {...
    +  }
    +  # ArmoniK workers
    +  worker = [
    +    {
    +      image = "dockerhubaneo/armonik-sdk-cpp-dynamicworker"
    +      tag = "0.5.2"
    +      limits = {...}
    +      requests = {...}
    +    }
    +  ]
    +  hpa = {...
    +  }
    +}
    ```

2. Redeploy ArmoniK to include the new partition.

## Building and running the example

You can build either directly on your system or using Docker containers.

### Building in your system

1. Install the right packages for your distribution:

   - [ArmoniK.Api](https://github.com/aneoconsulting/ArmoniK.Api/releases/tag/3.28.3)
   - [ArmoniK.Extensions.Cpp](https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases)

2. Compile the worker shared library:

   ```bash
   cd worker && mkdir build
   cmake -B build -S .
   cmake --build ./build
   ```

   This produces `libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so`. Copy it to ArmoniK's shared data folder — for a localhost deployment the default is `infrastructure/quick-deploy/localhost/data/`.

3. Compile the client:

   ```bash
   cd client && mkdir build
   cmake -B build -S .
   cmake --build ./build
   ```

   This produces `ArmoniK.Samples.Cpp.ChainedArithmetic.Client`. It reads the following configuration from environment variables or from an `appsettings.json` file placed next to the executable:

   ```json
   {
     "GrpcClient__Endpoint": "http://xxx.xxx.xxx:5001",
     "PartitionId": "cppdynamic",
     "Worker__LibraryPath": "/absolute/path/to/libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so"
   }
   ```

4. Run the client:

   ```bash
   ./ArmoniK.Samples.Cpp.ChainedArithmetic.Client
   ```

### Using Docker containers

The provided `Makefile` builds both images:

```bash
make build_worker
make build_client
# or both at once
make all
```

Copy the worker library to ArmoniK's shared data folder before starting the client:

```bash
docker run --rm \
  -v /path/to/armonik/data:/host \
  --entrypoint sh armonik-samples-cpp-chained-arithmetic-worker:0.0.0-sdk \
  -c "cp /app/libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so /host/"
```

Run the client:

```bash
docker run --rm \
  -e GrpcClient__Endpoint=http://xxx.xxx.xxx:5001 \
  -e PartitionId=cppdynamic \
  armonik-samples-cpp-chained-arithmetic-client:0.0.0-sdk
```

Expected output:

```
[Info]  Starting DynamicLibrary ArmoniK Client...
[Info]  Library uploaded, blob ID: <blob-id>
[Info]  Session ID: <session-id>
[Info]  Multiply tasks submitted: <task-id-1>, <task-id-2>
[Info]  Result for task <task-id-1>: 9.000000
[Info]  Result for task <task-id-2>: 16.000000
[Info]  Add task submitted: <task-id-3>
[Info]  Result for task <task-id-3>: 25.000000
[Info]  Task processing complete.
```

## Notes

- Replace `http://xxx.xxx.xxx:5001` with the correct endpoint for your control plane.
- The image tags and package versions are defined in the `Makefile` and can be overridden with environment variables (`CLIENT_IMAGE_TAG`, `WORKER_IMAGE_TAG`, `API_VERSION`, `SDK_VERSION`).
