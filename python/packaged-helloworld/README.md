# A Python Hello World on ArmoniK

## Description

This project contains a worker and a client to interact with ArmoniK's Control Plane. The worker processes tasks sent by the Agent, transforming input data and sending results back to it.

## Steps

1. Build the Docker image for the worker:
    ```bash
    docker build -t dockerhubaneo/armonik_demo_python_helloworld -f Dockerfile .
    ```

2. Deploy ArmoniK locally by following the instructions at [ArmoniK Documentation](https://aneoconsulting.github.io/ArmoniK/). Ensure you create a new partition named "helloworldpython" with the worker's image. If the deployement was successful, you should get an ip and a port that will be used as the endpoint in order to run the client.

3. Create and activate a virtual environment and build the package:
    ```bash
    python -m venv .venv
    . .venv/bin/activate
    pip install -U pip setuptools && pip install .
    ```

## Usage

The package build will produce a script `helloworld` that you can use directly to run the Client. Use the name of the partition and the endpoint obtained after ArmoniK's deployment:

    ```bash
    helloworld --partition helloworldpython --endpoint <ip>:port
    ```

You should get an output as follows:

```bash
<TIME> - INFO - sessionId: id
<TIME> - INFO - data uploaded
<TIME> - INFO - payload uploaded
<TIME> - INFO - tasks submitted
<TIME> - INFO - resultId: id, data: Hello world!
<TIME> - INFO - End Connection!
```