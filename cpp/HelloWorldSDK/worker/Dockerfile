FROM debian:12 AS builder

# Install debian package making tools
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

# Set the work directory
WORKDIR /app

# Copy project files into the container
COPY CMakeLists.txt ./
COPY HelloService.h ./
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

# Create a build directory and build the project
WORKDIR /app
RUN mkdir -p build && cd build && cmake .. && make

FROM debian:12 AS runner

# Copy only the necessary files from the build stage
WORKDIR /app
COPY --from=builder /app/build/libArmoniK.Samples.Cpp.Hello.SDK.Worker.so ./
