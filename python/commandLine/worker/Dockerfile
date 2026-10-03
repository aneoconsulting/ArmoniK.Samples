FROM python:3.10-slim AS builder
WORKDIR /app
RUN python -m venv .venv && .venv/bin/pip install --no-cache-dir -U pip setuptools
COPY requirements.txt ./
RUN .venv/bin/pip install --no-cache-dir -r requirements.txt
COPY . /app

FROM python:3.10-slim
WORKDIR /app
RUN groupadd --gid 5000 armonikuser \
    && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh armonikuser \
    && mkdir /cache && chown armonikuser: /cache
USER armonikuser
ENV PATH="/app/.venv/bin:$PATH" \
    PYTHONUNBUFFERED=1
COPY --from=builder /app /app
ENTRYPOINT ["python", "main.py"]