# Build the multiply library
FROM debian:bookworm-20260623 AS builder

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" TZ="Europe/London" apt-get install --no-install-recommends -y \
    devscripts \
    equivs \
    cmake \
    cmake \
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
COPY ServiceDispatch.cpp ./


# Install Debian packages for ArmoniK API
WORKDIR /tmp
ARG API_VERSION

RUN wget "https://github.com/aneoconsulting/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.deb" && \
    dpkg -i libarmonik-${API_VERSION}-Linux.deb && \
    apt-get -f install -y && \
    rm libarmonik-${API_VERSION}-Linux.deb

# Install Debian packages for ArmoniK SDK
ARG SDK_VERSION
RUN wget "https://github.com/aneoconsulting/ArmoniK.Extensions.Cpp/releases/download/${SDK_VERSION}/armoniksdk-${SDK_VERSION}.deb" && \
    dpkg -i armoniksdk-${SDK_VERSION}.deb && \
    apt-get -f install -y && \
    rm armoniksdk-${SDK_VERSION}.deb

WORKDIR /app
RUN mkdir -p build && cd build && cmake .. && make -j

FROM debian:bookworm-20260623 AS runner

WORKDIR /app
COPY --from=builder /app/build/libArmoniK.Samples.Cpp.ChainedArithmetic.Worker.so ./
