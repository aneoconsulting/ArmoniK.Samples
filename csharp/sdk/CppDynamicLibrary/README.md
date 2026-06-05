# C# Client with C++ Dynamic Worker (Interoperability)

This project demonstrates cross-language interoperability between a C# client and a C++ dynamic worker on ArmoniK. The C# client uses the ArmoniK C# SDK to upload a C++ shared library (`.so`) as a blob, then submits a task to a C++ DynamicWorker that loads the library at runtime and dispatches to the specified function.

The default configuration calls the `multiply` function of the `ChainedArithmetic` C++ worker with inputs `num1=2` and `num2=3`, expecting the result `6`.

## Prerequisites

An ArmoniK cluster with a partition running the dynamic C++ worker:

1. Add a `cppdynamic` partition to your infrastructure:

    ```diff
    +cppdynamic = {
    +  replicas = 0
    +  polling_agent = { ... }
    +  worker = [
    +    {
    +      image = "dockerhubaneo/armonik-sdk-cpp-dynamicworker"
    +      tag   = "0.5.2"
    +      limits   = { ... }
    +      requests = { ... }
    +    }
    +  ]
    +  hpa = { ... }
    +}
    ```

2. Redeploy ArmoniK to include the new partition.

3. Build the C++ worker shared library you want to call. The default configuration points to `libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so` from the `cpp/ChainedArithmetic` sample. Copy it to ArmoniK's shared data folder before running this client.

## Building and running the example

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Building and running

1. Restore dependencies and build:

   ```bash
   cd csharp/sdk
   dotnet build
   ```

2. Configure the client. Edit `CppDynamicLibrary/appsettings.json` or pass values via environment variables or command-line flags:

   | Key | Description | Default |
   |-----|-------------|---------|
   | `Grpc__EndPoint` | ArmoniK control-plane endpoint | `http://localhost:5001` |
   | `LibraryPath` | Absolute path to the `.so` to upload | *(required)* |
   | `Partition` | Target ArmoniK partition | `default` |
   | `Symbol` | Function name to invoke in the library | `multiply` |

   Example using command-line flags:

   ```bash
   dotnet run --project CppDynamicLibrary \
     --LibraryPath /path/to/libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so \
     --Partition cppdynamic \
     --Symbol multiply
   ```

3. Expected output:

   ```
   [Info]  Library : /path/to/libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so
   [Info]  Partition: cppdynamic
   [Info]  Opening session on partition 'cppdynamic'...
   [Info]  Session opened: <session-id>
   [Info]  Uploaded library blob: <blob-id>
   [Info]  Submitting task ...
   [Info]  Task submitted: <task-id>
   [Info]  Waiting for task result...
   [Info]  Result: 6
   [Info]  Closing session <session-id>...
   [Info]  Session <session-id> closed.
   ```

## How it works

1. The client uploads the `.so` file as a named blob in the session.
2. The blob ID is stored in the task options under `LibraryBlobId`, and the target function name under `Symbol`.
3. The C++ DynamicWorker receives the task, retrieves the blob from its data dependencies, writes it to a temporary file, `dlopen()`s it, and calls `armonik_call` with the symbol and inputs.
4. The result is written back as a blob and the C# client reads it via its callback.

## Notes

- Replace `http://localhost:5001` with the correct endpoint for your control plane.
- The library can be any C++ shared object that implements the `armonik_call` entry point. See the C++ SDK samples (`cpp/HelloWorldSDK`, `cpp/ChainedArithmetic`) for examples.
