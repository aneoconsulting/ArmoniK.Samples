# Hello World Example using ArmoniK's C ABI

This project demonstrates a "Hello World" example using ArmoniK's low-level C ABI directly, without the C++ SDK worker wrapper. The worker plugin is written in pure C and implements the five `armonik_*` functions defined in `ArmoniKSDKInterface.h`. The client is written in C++ and uses the ArmoniK C++ SDK.

The example supports three behaviors selected via a command-line flag:

| Flag | Worker behavior | Expected outcome |
|---|---|---|
| *(none)* | Appends `" World!"` to the input and returns it | Task succeeds with result `"Hello, World!"` |
| `--error` | Returns `ARMONIK_STATUS_ERROR` | Task fails permanently, no retry |
| `--retry` | Returns `ARMONIK_STATUS_RETRY` | Task is retried until `max_retries` is exhausted, then fails |

## Prerequisites

An ArmoniK cluster with a partition running the dynamic C++ worker provided by the SDK.
If you already deployed ArmoniK and wish to add such a partition, follow these steps:

1. Add a `cppdynamic` partition to your infrastructure.

    ```diff
    +cppdynamic = {
    +  replicas = 0
    +  polling_agent = { ... }
    +  worker = [
    +    {
    +      image = "dockerhubaneo/armonik-sdk-cpp-dynamicworker"
    +      tag   = "0.4.4"
    +      limits   = { ... }
    +      requests = { ... }
    +    }
    +  ]
    +  hpa = { ... }
    +}
    ```

2. Redeploy ArmoniK to include the new partition.

## Building and running the example

### Building in your system

1. Install the right packages for your distribution:

   - [ArmoniK.Api](https://github.com/aneoconsulting/ArmoniK.Api/releases/tag/3.28.3)
   - [ArmoniK.Extensions.Cpp](https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases)

2. Compile the worker shared library and copy it to ArmoniK's shared data folder.

   The worker only needs `gcc` — no C++ compiler is required.

   ```bash
   cd worker && mkdir build
   cmake -B build -S .
   cmake --build ./build
   ```

   This produces `libArmoniK.Samples.C.Hello.Worker.so`. Copy it to ArmoniK's shared data folder — for a localhost deployment the default is `infrastructure/quick-deploy/localhost/data/`.

3. Compile the client.

   ```bash
   cd client && mkdir build
   cmake -B build -S .
   cmake --build ./build
   ```

   This produces `ArmoniK.Samples.C.Hello.Client`. It reads the following configuration, either from environment variables or from an `appsettings.json` file placed next to the executable:

   ```json
   {
     "GrpcClient": { "Endpoint": "http://xxx.xxx.xxx:5001" },
     "PartitionId": "cppdynamic"
   }
   ```

4. Run the client.

   ```bash
   # Normal hello task (succeeds)
   ./ArmoniK.Samples.C.Hello.Client

   # Trigger a permanent task error
   ./ArmoniK.Samples.C.Hello.Client --error

   # Trigger a transient error that exhausts retries
   ./ArmoniK.Samples.C.Hello.Client --retry
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
  --entrypoint sh armonik-samples-c-hello-worker:0.0.0 \
  -c "cp /app/libArmoniK.Samples.C.Hello.Worker.so /host/"
```

Run the client:

```bash
# Normal hello task
docker run --rm \
  -e GrpcClient__Endpoint=http://xxx.xxx.xxx:5001 \
  -e PartitionId=cppdynamic \
  armonik-samples-c-hello-client:0.0.0

# Permanent error
docker run --rm \
  -e GrpcClient__Endpoint=http://xxx.xxx.xxx:5001 \
  -e PartitionId=cppdynamic \
  armonik-samples-c-hello-client:0.0.0 --error

# Transient error / retry
docker run --rm \
  -e GrpcClient__Endpoint=http://xxx.xxx.xxx:5001 \
  -e PartitionId=cppdynamic \
  armonik-samples-c-hello-client:0.0.0 --retry
```

Expected output for the normal case:

```
[Info]  Starting Hello World ArmoniK Client (pure C worker), mode: hello
[Info]  Session ID: 3fbeebca-083d-416a-b0db-f866d92d13f5
[Info]  Task Submitted: e4cdeead-5fb2-4892-a643-82cd17393e90
[Info]  HANDLE RESPONSE : Received result of size 13 for taskId e4cdeead-5fb2-4892-a643-82cd17393e90
        Content : Hello, World!
[Info]  Task Processing Complete.
```

Expected output for the error case:

```
2026-04-27T11:32:19.206365939z	[Info]	Starting Hello World ArmoniK Client (pure C worker), mode: error
Unable to load json file ./appsettings.json : IO_ERROR: Error reading the file.2026-04-27T11:32:19.218309917z	[Info]Session ID: 6159a9d8-2d15-4b97-8914-9740d7fb04d8
2026-04-27T11:32:19.253967120z	[Info]	Task Submitted: f7ca0cd4-1f98-43c6-a8c8-3b8f113cff4d
2026-04-27T11:32:19.775699094z	[Error]	Error while handling result 59a5e417-6bca-4b66-a49c-b4840872f7fe for task f7ca0cd4-1f98-43c6-a8c8-3b8f113cff4d : Result is aborted : TaskId : f7ca0cd4-1f98-43c6-a8c8-3b8f113cff4d Errors : 
TASK_STATUS_ERROR : Permanent error requested by client
2026-04-27T11:32:19.775725731z	[Error]	HANDLE ERROR : Error for task id f7ca0cd4-1f98-43c6-a8c8-3b8f113cff4d : Result is aborted : TaskId : f7ca0cd4-1f98-43c6-a8c8-3b8f113cff4d Errors : 
TASK_STATUS_ERROR : Permanent error requested by client

2026-04-27T11:32:20.276001425z	[Info]	Task Processing Complete.

```

Expected output for the retry case ( the client sets `max_retries=3` in the corresponding session for this case):

```
2026-04-27T11:41:02.510129823z	[Info]	Starting Hello World ArmoniK Client (pure C worker), mode: retry
Unable to load json file ./appsettings.json : IO_ERROR: Error reading the file.2026-04-27T11:41:02.521919924z	[Info]Session ID: 5102c075-7e2d-48fc-b938-497293f96d4a
2026-04-27T11:41:02.555341296z	[Info]	Task Submitted: c844a085-8ba0-4b20-8767-5973e5d91620
2026-04-27T11:41:03.570548731z	[Error]	Error while handling result 8b2f1c81-e87b-4a57-8613-9e224c35eaea for task c844a085-8ba0-4b20-8767-5973e5d91620 : Result is aborted : TaskId : c844a085-8ba0-4b20-8767-5973e5d91620###3 Errors : 
TASK_STATUS_ERROR : Worker associated to scheduling agent compute-plane-default-85b8458cf7-ntc6c is down with error: 
Status(StatusCode="Unavailable", Detail="Error processing task : Transient error requested by client, will retry")
2026-04-27T11:41:03.570593440z	[Error]	HANDLE ERROR : Error for task id c844a085-8ba0-4b20-8767-5973e5d91620 : Result is aborted : TaskId : c844a085-8ba0-4b20-8767-5973e5d91620###3 Errors : 
TASK_STATUS_ERROR : Worker associated to scheduling agent compute-plane-default-85b8458cf7-ntc6c is down with error: 
Status(StatusCode="Unavailable", Detail="Error processing task : Transient error requested by client, will retry")

2026-04-27T11:41:04.070765530z	[Info]	Task Processing Complete.

```



## Notes

- The worker is a pure C shared library. It has no dependency on the C++ SDK runtime or the ArmoniK API library at runtime — only `ArmoniKSDKInterface.h` is needed at compile time.
- Error vs. retry semantics map directly to `armonik_status_t` return values: `ARMONIK_STATUS_ERROR` marks the task as permanently failed; `ARMONIK_STATUS_RETRY` signals a transient failure and causes ArmoniK to reschedule the task according to its `max_retries` policy.
- Replace `http://xxx.xxx.xxx:5001` with the correct endpoint for your control plane.
- The image tags and package versions used in the `Makefile` can be overridden with environment variables (`CLIENT_IMAGE_TAG`, `WORKER_IMAGE_TAG`, `API_VERSION`, `SDK_VERSION`).
