FROM debian:12@sha256:268fb24c39bca92d326dd83ea3abebe4a3f7751c804ffef23bb6d857dd343ff8 AS builder

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" apt-get install --no-install-recommends -y \
    ca-certificates \
    cmake \
    make \
    wget \
    git \
    gcc \
    g++ \
    libc6-dev \
    libgrpc-dev \
    libgrpc++-dev \
    libprotobuf-dev \
    protobuf-compiler-grpc \
    nlohmann-json3-dev \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Install ArmoniK API — provides cmake configs for ArmoniK.Api.Common/Client needed to compile the SDK
ARG API_VERSION
ARG LIBARMONIK_DEB_SHA256
ARG LIBARMONIK_DEV_DEB_SHA256
# Checked against the SHA-256 from cpp/versions.env; the runtime package is kept for the runner stage
RUN wget -q "https://github.com/aneoconsulting/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.deb" \
        "https://github.com/aneoconsulting/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-dev-${API_VERSION}-Linux.deb" && \
    printf '%s  %s\n' \
        "${LIBARMONIK_DEB_SHA256}" "libarmonik-${API_VERSION}-Linux.deb" \
        "${LIBARMONIK_DEV_DEB_SHA256}" "libarmonik-dev-${API_VERSION}-Linux.deb" | sha256sum -c - && \
    dpkg -i libarmonik-${API_VERSION}-Linux.deb libarmonik-dev-${API_VERSION}-Linux.deb && \
    apt-get -f install -y && \
    mkdir -p /pkgs/runtime && mv libarmonik-${API_VERSION}-Linux.deb /pkgs/runtime/ && \
    rm libarmonik-dev-${API_VERSION}-Linux.deb

ARG SDK_VERSION
ARG ARMONIKSDK_DEB_SHA256
# Checked against the SHA-256 from cpp/versions.env
RUN wget -q "https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases/download/${SDK_VERSION}/armoniksdk-${SDK_VERSION}.deb" && \
    printf '%s  %s\n' "${ARMONIKSDK_DEB_SHA256}" "armoniksdk-${SDK_VERSION}.deb" | sha256sum -c - && \
    dpkg -i armoniksdk-${SDK_VERSION}.deb && \
    apt-get -f install -y && \
    rm armoniksdk-${SDK_VERSION}.deb

WORKDIR /app
COPY CMakeLists.txt ./
COPY hello_worker.c ./
RUN mkdir -p build && cd build && cmake .. && make

# Carries the worker library only: it is copied out of the image, never run as a service.
# It keeps a shell and the root default so that the documented
# `docker run --entrypoint sh <image> -c "cp /app/<lib>.so /host/"` keeps working.
FROM debian:12-slim@sha256:734a72b3d6142a71993b4bc580ca01d8ca687f77a2c422d96a961d3376b179bc AS runner

WORKDIR /app
COPY --from=builder /app/build/libArmoniK.Samples.C.Hello.Worker.so ./
