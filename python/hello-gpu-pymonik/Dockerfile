FROM nvidia/cuda:12.9.1-runtime-ubuntu24.04
COPY --from=ghcr.io/astral-sh/uv:latest /uv /uvx /bin/
WORKDIR /app
ARG USE_PYTHON_VERSION="3.10.12"
COPY pyproject.toml . 
RUN apt-get update && apt-get --no-install-recommends install -y \
    python3 \
    python3-pip \
    python3-venv \
    && echo "Writing Python version: $USE_PYTHON_VERSION" && echo "$USE_PYTHON_VERSION" >> .python-version \
    && cat .python-version \
    && rm -rf /var/lib/apt/lists/*

RUN groupadd --gid 5000 armonikuser && \
    useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && \
    mkdir /cache && \
    chown armonikuser: /cache && \
    chown -R armonikuser: /app 

USER armonikuser

RUN uv sync

COPY worker.py .
ENV PATH="/app/.venv/bin:$PATH" \
    PYTHONUNBUFFERED=1 \
    LD_LIBRARY_PATH=""
ENTRYPOINT ["python3", "worker.py"]
