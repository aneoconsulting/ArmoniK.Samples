# Static Worker Example using ArmoniK's C++ SDK

This project shows a C++ worker with the service compiled into the worker executable, called through the convention used by all ArmoniK SDKs. Nothing is loaded at runtime: there is no shared library to copy to the workers or to upload.

The client runs the same computation as [ChainedArithmetic](../ChainedArithmetic). It submits two independent `multiply` tasks (3×3 and 4×4), waits for their results, then submits an `add` task whose inputs are the blob outputs of the multiply tasks. The expected final result is 3×3 + 4×4 = 25.

The two samples differ in how the worker gets the service code:

| | ChainedArithmetic | StaticWorker |
|---|---|---|
| Worker | SDK's DynamicWorker image | this sample's worker image |
| Service code | `.so` loaded with `dlopen`, from a path or an uploaded blob | compiled into the worker executable |
| Client task options | `SetDynamicLibrary(lib)` | `SetConvention("multiply")` |

On the worker side, `ArmoniK::Sdk::Worker::ConventionExecutor` does the work. Called from the worker's `Execute`, it reads the method name from the `Symbol` task option, resolves the named inputs, calls the service and sends the result:

```cpp
armonik::api::worker::ProcessStatus Execute(armonik::api::worker::TaskHandler &taskHandler) override {
  return executor.Execute(taskHandler);
}
```

The service is an ordinary `ArmoniK::Sdk::Worker::ServiceBase`, as in the dynamic samples. Since the worker follows the convention, clients written with the Java and C# SDKs can call it too.

## Prerequisites

- ArmoniK.Extensions.Cpp 0.5.8 or later, which provides `ConventionExecutor` and `TaskOptions::SetConvention`.
- An ArmoniK cluster with a partition that runs this sample's worker image. If you already deployed ArmoniK and wish to add such a partition, follow these steps:

1. Build the worker image (see below) and make it available to your cluster.

2. Add a `cppstatic` partition to your infrastructure.

    ```diff
    +cppstatic = {
    +  # number of replicas for each deployment of compute plane
    +  replicas = 0
    +  # ArmoniK polling agent
    +  polling_agent = {...
    +  }
    +  # ArmoniK workers
    +  worker = [
    +    {
    +      image = "armonik-samples-cpp-static-worker-worker"
    +      tag = "0.0.0-sdk"
    +      limits = {...}
    +      requests = {...}
    +    }
    +  ]
    +  hpa = {...
    +  }
    +}
    ```

3. Redeploy ArmoniK to include the new partition.

## Building and running the example

You can build either directly on your system or using Docker containers.

### Building in your system

1. Install the right packages for your distribution:

   - [ArmoniK.Api](https://github.com/aneoconsulting/ArmoniK.Api/releases/latest)
   - [ArmoniK.Extensions.Cpp](https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases)

2. Compile the worker executable:

   ```bash
   cd worker && mkdir build
   cmake -B build -S .
   cmake --build ./build
   ```

   This produces `ArmoniK.Samples.Cpp.StaticWorker.Worker`, to run as the worker container of the `cppstatic` partition.

3. Compile the client:

   ```bash
   cd client && mkdir build
   cmake -B build -S .
   cmake --build ./build
   ```

   This produces `ArmoniK.Samples.Cpp.StaticWorker.Client`. It reads the following configuration from environment variables or from an `appsettings.json` file placed next to the executable:

   ```json
   {
     "GrpcClient__Endpoint": "http://xxx.xxx.xxx:5001",
     "PartitionId": "cppstatic"
   }
   ```

4. Run the client:

   ```bash
   ./ArmoniK.Samples.Cpp.StaticWorker.Client
   ```

### Using Docker containers

The provided `Makefile` builds both images:

```bash
make build_worker
make build_client
# or both at once
make all
```

Run the client:

```bash
docker run --rm \
  -e GrpcClient__Endpoint=http://xxx.xxx.xxx:5001 \
  -e PartitionId=cppstatic \
  armonik-samples-cpp-static-worker-client:0.0.0-sdk
```

Expected output:

```
[Info]  Starting static worker ArmoniK Client...
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
- In the service, an `ArmoniKSdkException` fails the task, and any other exception makes ArmoniK retry it.
- The package versions are defined in [`cpp/versions.env`](../versions.env) and the image tags in the `Makefile`. All can be overridden with environment variables (`CLIENT_IMAGE_TAG`, `WORKER_IMAGE_TAG`, `API_VERSION`, `SDK_VERSION`).
