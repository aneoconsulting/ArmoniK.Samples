FROM python:3.7-slim
WORKDIR /app
COPY requirements.txt ./
# Install ArmoniK
RUN python -m pip install --no-cache-dir -r requirements.txt
# Setup the user (mandatory)
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && mkdir /cache && chown armonikuser: /cache
USER armonikuser
# Unbuffered Python logs
ENV PYTHONUNBUFFERED=1
# Copy scripts
COPY worker.py ./
# Run
ENTRYPOINT ["python", "worker.py"]
