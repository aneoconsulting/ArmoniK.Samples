# A Python Hello World on ArmoniK

## Description

This project contains a worker and a client to interact with ArmoniK's Control Plane. The worker processes a simple task sent by the Agent. The worker does nothing and sends a empty string to the Client.

## Preqrequisites
Create a partition named "cdf" with the worker's image in the terraform configuration file.

## Steps

1. Build the Docker image for the worker:
    ```bash
    docker build -t cdf-worker -f Dockerfile .
    ```

2. Deploy ArmoniK locally by following the instructions at [ArmoniK Documentation](https://aneoconsulting.github.io/ArmoniK/). Ensure you create a new partition named "cdf" with the worker's image named "cdf-worker".

3. Move to the `client` folder and create a virtual environment:
    ```bash
    cd client
    python -m venv .venv
    ```

4. Activate the virtual environment:
    ```bash
    source .venv/bin/activate
    ```

5. Install the client dependencies:
    ```bash
    pip install -r client-requirements.txt
    ```

## Usage

Make sure the file "armonik_benchmark_cli.py" is executable:

```bash
chmod +x armonik_benchmark_cli.py
```

Run the Client with the name of the partition:

```bash
./armonik_benchmark_cli --partition cdf --endpoint localhost:5001
```