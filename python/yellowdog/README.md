# A Python Hello World on ArmoniK

## Description

This project contains a worker and a client to interact with ArmoniK's Control Plane. The worker processes a simple task sent by the Agent. The worker does nothing and sends a empty string to the Client.

## Preqrequisites
Create a partition named "yellowdog" with the worker's image in the terraform configuration file.

## Steps

1. Build the Docker image for the worker:
    ```bash
    docker build -t yellowdog-worker -f Dockerfile .
    ```

2. Deploy ArmoniK locally by following the instructions at [ArmoniK Documentation](https://aneoconsulting.github.io/ArmoniK/). Ensure you create a new partition named "yellowdog" with the worker's image named "yellowdog-worker".

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

Run the Client with the name of the partition:

```bash
python client.py --partition yellowdog --endpoint localhost:5001
```