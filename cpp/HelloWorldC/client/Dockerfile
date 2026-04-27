FROM debian:12 AS builder

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" TZ="Europe/London" apt-get install --no-install-recommends -y \
    devscripts \
    equivs \
    cmake \
    wget \
    gcc \
    g++ \
    libgrpc-dev \
    libgrpc++-dev \
    libprotobuf-dev \
    protobuf-compiler-grpc

WORKDIR /app
COPY CMakeLists.txt ./
COPY HelloServiceHandler.h ./
COPY main.cpp ./

WORKDIR /tmp
ARG API_VERSION
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
RUN mkdir -p build && cd build && cmake .. && make -j

FROM debian:12 AS runner

RUN apt-get update && DEBIAN_FRONTEND="noninteractive" apt-get install --no-install-recommends -y \
    libgrpc++ \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=builder /lib/libArmoniK*.so* /lib/

WORKDIR /app
COPY --from=builder /app/build/ArmoniK.Samples.C.Hello.Client ./

ENTRYPOINT ["./ArmoniK.Samples.C.Hello.Client"]
