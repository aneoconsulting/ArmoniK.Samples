FROM debian:bookworm-20260623 AS builder

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
ARG API_VERSION=3.28.3
RUN wget "https://github.com/aneoconsulting/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.deb" && \
    dpkg -i libarmonik-${API_VERSION}-Linux.deb && \
    apt-get -f install -y && \
    rm libarmonik-${API_VERSION}-Linux.deb

ARG SDK_VERSION
RUN wget "https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases/download/${SDK_VERSION}/armoniksdk-${SDK_VERSION}.deb" && \
    dpkg -i armoniksdk-${SDK_VERSION}.deb && \
    apt-get -f install -y && \
    rm armoniksdk-${SDK_VERSION}.deb

WORKDIR /app
COPY CMakeLists.txt ./
COPY hello_worker.c ./
RUN mkdir -p build && cd build && cmake .. && make

FROM debian:bookworm-20260623 AS runner

WORKDIR /app
COPY --from=builder /app/build/libArmoniK.Samples.C.Hello.Worker.so ./
