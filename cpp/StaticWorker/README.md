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

### Windows client

The client also runs natively on Windows and sends its tasks to the same Linux `cppstatic` worker. It is the code of [`client`](client), built with MSVC by [`windows-client/build.ps1`](windows-client/build.ps1). Only the client is built: ArmoniK workers run on Linux.

The script:

1. Fetches ArmoniK.Extensions.Cpp at `SDK_WINDOWS_REF` from [`cpp/versions.env`](../versions.env), since no SDK release ships the Windows build yet.
2. Gets gRPC, protobuf, abseil, nlohmann_json and zlib, built with MSVC. See [Dependencies](#dependencies) below.
3. Builds ArmoniK.Api and the SDK with the SDK's `tools\windows\build.ps1`.
4. Builds the client and copies it to `dist`.

The CI job `buildCppWindowsClient` runs this script on `windows-latest`, with the default conda dependencies, to check that the client builds. It doesn't run the client.

Requirements:

- Visual Studio 2022 with the "Desktop development with C++" workload
- CMake 3.22 or later, and git
- [micromamba](https://mamba.readthedocs.io/en/latest/installation/micromamba-installation.html), only for the default dependencies

Build from a "Developer PowerShell for VS 2022":

```powershell
cd cpp\StaticWorker\windows-client
.\build.ps1
```

The first run takes a while: it creates the conda environment, then builds ArmoniK.Api and the SDK. Later runs reuse them. The result is a `dist` folder with the executable, `appsettings.json` and the DLLs it needs, which can be copied to another Windows machine.

The build fails on long paths. If the repository is deep in your tree, use a short build directory: `.\build.ps1 -WorkDir C:\akbuild`.

#### Dependencies

The script takes the dependencies from one of three sources:

| Source | Option | Notes |
|---|---|---|
| New conda environment | none (default) | Created with micromamba in `<WorkDir>\env` from the SDK's `tools\windows\environment.yml`. Prebuilt by conda-forge. Tested in CI. |
| Existing conda environment | `-DepsPrefix <env root>` | Must contain the packages of `environment.yml`. micromamba isn't needed. |
| Any CMake prefix (vcpkg, source builds) | `-DepsCMakePrefix <prefix>` | The prefix holding the dependencies' CMake config files. Add `-VcpkgRoot <vcpkg checkout>` for vcpkg. micromamba isn't needed. |

With vcpkg, for example:

```powershell
C:\vcpkg\vcpkg.exe install grpc protobuf abseil nlohmann-json zlib --triplet x64-windows
.\build.ps1 -DepsCMakePrefix C:\vcpkg\installed\x64-windows -VcpkgRoot C:\vcpkg
```

With the `x64-windows` triplet, the dependencies are DLLs, which the script copies from `<prefix>\bin` to `dist`. With `x64-windows-static-md`, they are linked into the executable and `dist` has no DLL. Building gRPC with vcpkg takes about an hour. Both cases, and how to install a static build, are described in the [SDK installation guide](https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/blob/main/.docs/content/guide/1.installation.md#how-to-build-for-windows-client-only).

Only the conda-forge versions pinned in `environment.yml` are tested. Other versions should work if they are recent enough for ArmoniK.Api (gRPC 1.50 or later). Build them in `Release` (`/MD`): a `Debug` build (`/MDd`) breaks the link.

To build without the script, follow the SDK installation guide to install ArmoniK.Api and the SDK in a prefix, then configure [`client`](client) with both prefixes:

```powershell
cmake -S ..\client -B build\client "-DCMAKE_PREFIX_PATH=C:\armonik;<dependency prefix>"
cmake --build build\client --config Release
```

With vcpkg, add the toolchain options printed at the end of the SDK's `build.ps1`.

Run the client from `dist`, since it reads `.\appsettings.json` from the current directory:

```powershell
cd dist
$env:GrpcClient__Endpoint = "http://xxx.xxx.xxx:5001"
$env:PartitionId = "cppstatic"
.\ArmoniK.Samples.Cpp.StaticWorker.Client.exe
```

The output is the same as on Linux. The control plane must be reachable from the Windows machine. With ArmoniK deployed in WSL2, `localhost` forwarding may not cover the control plane port: use the WSL2 IP address (`wsl hostname -I`) or the node IP instead.

## Notes

- Replace `http://xxx.xxx.xxx:5001` with the correct endpoint for your control plane.
- In the service, an `ArmoniKSdkException` fails the task, and any other exception makes ArmoniK retry it.
- The package versions are defined in [`cpp/versions.env`](../versions.env) and the image tags in the `Makefile`. All can be overridden with environment variables (`CLIENT_IMAGE_TAG`, `WORKER_IMAGE_TAG`, `API_VERSION`, `SDK_VERSION`).
