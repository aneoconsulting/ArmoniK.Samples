



## Getting started

### Prerequisites

- Familiarity with Terraform, Kubernetes, and Docker.
- CMake installed on WSL2.
- ArmoniK prerequisites, including Docker.

Clone the ArmoniK repo on WSL2 if you haven't already, and follow the [local installation guide](https://aneoconsulting.github.io/ArmoniK/installation) to install all necessary prerequisites using the provided script:

```bash
git clone https://github.com/aneoconsulting/ArmoniK.git
```



### Starting up ArmoniK

Once you've cloned the ArmoniK repo and completed the installation steps, start ArmoniK by running:

```bash
make deploy
```

It's that simple. 

### Building your application

To develop a C++ application for ArmoniK:

1. Clone the required repository and copy the `cpp` folder.
2. Use the provided `Makefile` for all necessary commands to build your app.
3. For autocomplete support, especially on WSL or Linux, run:

   ```bash
   make setup_dev_env
   ```

   This command builds the ArmoniK.Api shared libraries, copies generated protobuf files, and downloads additional dependencies into the `vendor` folder. Add these paths to your editor's include paths to enable autocomplete/intellisense. The command will take some time to download everything but you only need to run it once.

For Visual Studio Code, add or create a `c_cpp_properties.json` file in your `.vscode` folder with the following content:

```json
{
    "configurations": [
        {
            "name": "ArmoniK-cpp",
            "includePath": [
                "${workspaceFolder}/**",
                "${workspaceFolder}/armonik_api/include/armonik/worker/",
                "${workspaceFolder}/armonik_api/include/armonik/common/",
                "${workspaceFolder}/armonik_api/include/armonik/client/",
                "${workspaceFolder}/armonik_api/vendor/"
            ],
            "defines": [],
            "compilerPath": "/usr/bin/gcc",
            "cStandard": "c17",
            "cppStandard": "gnu++14",

        }
    ],
    "version": 4
}
```
Use the following commands to build your worker and client containers:

```bash
make build_worker
make build_client
```

You can modify the image names in the `Makefile` or pass them as parameters.

Make sure to modify the ```appsettings.json``` for our client to communicate with our ArmoniK deployment. 

```json
{
        "Grpc__EndPoint": "http://{armonik-output-ip}:5001"
}
```

Replace ```{armonik-output-ip}``` with the ip resulting from our ```make deploy``` command in the ArmoniK repo.

### Running your application on ArmoniK

Now, going back to our ArmoniK infrastructure, we have to change the name of the ArmoniK worker in our previously deployed infrastructure:

```
...
    # ArmoniK workers
    worker = [
      {
        image = "armonik-cpp-hello-worker"
        tag="0.1.0"
...
```

Run the deploy command again to update the image.

You can now run your client application to send tasks over to ArmoniK:
```
docker run --rm armonik-cpp-hello-client:0.1.0 
``` 

That's it! You can now develop and run C++ applications on ArmoniK.

## Troubleshooting

### My worker hasn't updated after I redeployed it with the same tag

If your worker hasn't updated after redeployment with the same tag, use the following command to update the image in your local deployment:

```
kubectl set image deployment/compute-plane-default -n armonik worker-0=image-name
```

If the issue persists, try changing the image name temporarily before reverting it to the correct name.

### I get an error in Seq that says that my TaskOptions have missing parameters

If you haven't modified this code, the more likely cause of this error is that you haven't updated the image name and tag in ArmoniK. Make sure to **redeploy** after doing so (run ```make deploy```).
