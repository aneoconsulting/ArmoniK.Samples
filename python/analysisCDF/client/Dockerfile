# Stage 1: Build the client with a virtual environment and install dependencies
FROM python:3.10-slim AS builder
WORKDIR /app
# Create a virtual environment and update pip and setuptools
RUN python -m venv .venv && .venv/bin/pip install --no-cache-dir -U pip setuptools
# Copy your requirements (adjust name if needed)
COPY requirements.txt ./
RUN .venv/bin/pip install --no-cache-dir -r requirements.txt
# Copy the whole source (including main.py)
COPY . .

# Stage 2: Create the runtime image
FROM python:3.10-slim
WORKDIR /app
# Create a non-root user to run the application
RUN groupadd --gid 5000 armonikuser && \
    useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 \
         --shell /bin/sh --skel /dev/null armonikuser && \
    mkdir -p /cache && chown armonikuser: /cache
USER armonikuser
# Set environment variables
ENV PATH="/app/.venv/bin:$PATH" \
    PYTHONUNBUFFERED=1
# Copy the built app from the builder stage
COPY --from=builder /app /app
# Set the entrypoint to launch your benchmarking client
ENTRYPOINT ["python", "main.py"]