FROM python:3.10-slim AS builder
WORKDIR /app
RUN python -m venv .venv && .venv/bin/pip install --no-cache-dir -U pip setuptools
COPY pyproject.toml ./
COPY src/helloworld /app/src/helloworld
RUN . ./.venv/bin/activate && pip install --no-cache-dir .

FROM python:3.10-slim
WORKDIR /app
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && mkdir /cache && chown armonikuser: /cache
USER armonikuser
ENV PATH="/app/.venv/bin:$PATH" PYTHONUNBUFFERED=1
COPY --from=builder /app /app
ENTRYPOINT ["python", "src/helloworld/worker/worker.py"]