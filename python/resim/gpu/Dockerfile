FROM nvidia/cuda:12.0.0-devel-ubuntu20.04

# Install system dependencies
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    build-essential \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy and install Python requirements
COPY requirements.txt ./
RUN pip3 install --no-cache-dir -r requirements.txt

# Create armonik user and cache directory
RUN groupadd --gid 5000 armonikuser && \
    useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && \
    mkdir /cache && chown armonikuser: /cache

USER armonikuser
ENV PYTHONUNBUFFERED=1

WORKDIR /app
COPY common.py /app
COPY worker.py /app

ENTRYPOINT ["python3", "worker.py"]
