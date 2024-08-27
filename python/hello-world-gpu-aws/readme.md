# A Python Hello World on ArmoniK on the GPU

## Description

This project contains a worker and a client to interact with ArmoniK's Control Plane. The worker processes tasks sent by the Agent, transforming input data and sending results back to it. The input data consists of two vectors with a size specified by the user. The worker performs the sum on the GPU to accelerate the computation.

## Steps

1. Build the Docker image for the worker:
    ```bash
    docker build -t useraneo/armonik-worker -f Dockerfile .
    ```

2. Push the image to dockerhub:
    ```
    docker push useraneo/gpu-worker
    ```

3. Deploy ArmoniK on aws by following the instructions at [ArmoniK Documentation](https://aneoconsulting.github.io/ArmoniK/installation/aws/aws-all-in-one-deployment). Ensure you create a new partition named "gputest" with the worker's image. Following this example:

```bash
  # Partition that run the workload on gpu
  gputest = {
    node_selector = { service = "gpu_workers" }
    # number of replicas for each deployment of compute plane
    replicas = 1
    # ArmoniK polling agent
    polling_agent = {
      limits = {
        cpu    = "2000m"
        memory = "2048Mi"
      }
      requests = {
        cpu    = "500m"
        memory = "256Mi"
      }
    }
    # ArmoniK workers
    worker = [
      {
        image = "useraneo/gpu-worker"
        tag = "latest"
        limits = {
          cpu    = "4000m"
          memory = "16384Mi"
          "nvidia.com/gpu" = "1"
        }
        requests = {
          cpu    = "2000m"
          memory = "8192Mi"
          "nvidia.com/gpu" = "1"
        }
      }
    ]
    hpa = {
      type              = "prometheus"
      polling_interval  = 15
      cooldown_period   = 300
      min_replica_count = 0
      max_replica_count = 100
      behavior = {
        restore_to_original_replica_count = true
        stabilization_window_seconds      = 300
        type                              = "Percent"
        value                             = 100
        period_seconds                    = 15
      }
      triggers = [
        {
          type      = "prometheus"
          threshold = 2
        },
      ]
    }
  },
```

4. Create a virtual environment:
    ```bash
    python -m venv .venv
    ```

5. Activate the virtual environment:
    ```bash
    source .venv/bin/activate
    ```

6. Install the client dependencies:
    ```bash
    pip install -r client-requirements.txt
    ```

## Usage

Run the Client with the name of the partition:

```bash
python client.py --partition gputest --endpoint example.eu-west-3.elb.amazonaws.com:5001
```