# Multi-arch (linux/amd64, linux/arm64): every dependency ships as a manylinux wheel, nothing is compiled.
ARG PYTHON_VERSION=3.12
FROM python:${PYTHON_VERSION}-slim AS base

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    PYTHONPATH=/app/src

WORKDIR /app
COPY requirements.txt .
# --only-binary: fail fast instead of compiling if a wheel is missing for the target platform
RUN pip install --only-binary=:all: -r requirements.txt
COPY src ./src

FROM base AS test
RUN pip install --only-binary=:all: pytest==9.1.1
COPY tests ./tests
COPY pyproject.toml .
RUN python -m pytest -q -p no:cacheprovider

FROM base AS final
# Same UID as the ArmoniK polling agent so both can use the shared unix sockets
USER 1654
EXPOSE 1080
ENTRYPOINT ["python", "-m", "quantlib_worker"]
