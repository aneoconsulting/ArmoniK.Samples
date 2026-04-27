FROM debian:12 AS builder

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
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Install ArmoniK API — provides cmake configs for ArmoniK.Api.Common/Client needed to compile the SDK
ARG API_VERSION=3.28.3
RUN wget "https://github.com/aneoconsulting/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.deb" && \
    dpkg -i libarmonik-${API_VERSION}-Linux.deb && \
    apt-get -f install -y && \
    rm libarmonik-${API_VERSION}-Linux.deb

# Clone and build the SDK from the feature branch (introduces ARMONIK_STATUS_RETRY)
ARG SDK_BRANCH=jf/retryExcep
RUN git clone --branch "${SDK_BRANCH}" --depth 1 \
        https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp.git /tmp/sdk && \
    cmake -B /tmp/sdk/build -S /tmp/sdk \
          -DCMAKE_BUILD_TYPE=Release \
          -DCMAKE_INSTALL_PREFIX=/usr \
          -DBUILD_DYNAMICWORKER=OFF \
          -DBUILD_END2END=OFF && \
    cmake --build /tmp/sdk/build -j$(nproc) && \
    cmake --install /tmp/sdk/build && \
    rm -rf /tmp/sdk

WORKDIR /app
COPY CMakeLists.txt ./
COPY hello_worker.c ./
RUN mkdir -p build && cd build && cmake .. && make

FROM debian:12 AS runner

WORKDIR /app
COPY --from=builder /app/build/libArmoniK.Samples.C.Hello.Worker.so ./
