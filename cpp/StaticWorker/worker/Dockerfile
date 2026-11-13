FROM debian:12@sha256:268fb24c39bca92d326dd83ea3abebe4a3f7751c804ffef23bb6d857dd343ff8 AS builder

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" TZ="Europe/London" apt-get install --no-install-recommends -y \
    ca-certificates \
    cmake \
    make \
    wget \
    gcc \
    g++ \
    libgrpc-dev \
    libgrpc++-dev \
    libprotobuf-dev \
    protobuf-compiler-grpc \
    nlohmann-json3-dev

WORKDIR /app

COPY CMakeLists.txt ./
COPY ArithmeticService.h ./
COPY main.cpp ./

# Install Debian packages for ArmoniK API
WORKDIR /tmp
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

# Install Debian packages for ArmoniK SDK
ARG SDK_VERSION
ARG ARMONIKSDK_DEB_SHA256
# Checked against the SHA-256 from cpp/versions.env
RUN wget -q "https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases/download/${SDK_VERSION}/armoniksdk-${SDK_VERSION}.deb" && \
    printf '%s  %s\n' "${ARMONIKSDK_DEB_SHA256}" "armoniksdk-${SDK_VERSION}.deb" | sha256sum -c - && \
    dpkg -i armoniksdk-${SDK_VERSION}.deb && \
    apt-get -f install -y && \
    rm armoniksdk-${SDK_VERSION}.deb

WORKDIR /app
RUN mkdir -p build && cd build && cmake .. && make -j

FROM debian:12-slim@sha256:734a72b3d6142a71993b4bc580ca01d8ca687f77a2c422d96a961d3376b179bc AS runner

# Runtime only: the libarmonik package checked in the builder stage pulls the gRPC and protobuf runtime libraries
COPY --from=builder /pkgs/runtime/ /tmp/pkgs/
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install --no-install-recommends -y \
    ca-certificates \
    libre2-9 \
    zlib1g \
    libssl3 \
    /tmp/pkgs/*.deb \
    && rm -rf /tmp/pkgs /var/lib/apt/lists/*

WORKDIR /app
COPY --from=builder /app/build/ArmoniK.Samples.Cpp.StaticWorker.Worker ./

# Non-root by default. In an ArmoniK deployment the compute-plane pod sets the user itself.
USER 65534:65534

ENTRYPOINT ["./ArmoniK.Samples.Cpp.StaticWorker.Worker"]
