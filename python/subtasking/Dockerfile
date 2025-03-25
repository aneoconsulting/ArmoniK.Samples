FROM python:3.12.3
WORKDIR /app
COPY requirements.txt ./
RUN python -m venv .venv
RUN . .venv/bin/activate
# Install ArmoniK
RUN .venv/bin/pip install --no-cache-dir -U pip setuptools && pip install --extra-index-url https://test.pypi.org/simple/ armonik==3.25.1.dev3601
# Setup the user (mandatory)
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && mkdir /cache && chown armonikuser: /cache
USER armonikuser
# Unbuffered Python logs
ENV PYTHONUNBUFFERED=1
# Copy scripts
COPY worker.py ./
# Run
ENTRYPOINT ["python", "worker.py"]
