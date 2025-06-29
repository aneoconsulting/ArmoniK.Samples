FROM python:3.10
WORKDIR /app
RUN python -m venv .venv && .venv/bin/pip install --no-cache-dir -U pip setuptools
COPY pyproject.toml ./
COPY src/helloworld /app/src/helloworld
RUN . ./.venv/bin/activate && pip install --no-cache-dir .
RUN groupadd --gid 5000 armonikuser && \
    useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && \
    mkdir /cache && chown armonikuser: /cache && \
    python -m venv .venv && \
    . ./.venv/bin/activate && \
    pip install --no-cache-dir -U pip setuptools && \
    pip install --no-cache-dir .
USER armonikuser
ENV PATH="/app/.venv/bin:$PATH" PYTHONUNBUFFERED=1
ENTRYPOINT ["python", "src/helloworld/worker/worker.py"]