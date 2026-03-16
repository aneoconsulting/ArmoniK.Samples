FROM registry.access.redhat.com/ubi8 AS builder

ARG PARALLEL_JOBS=4

RUN rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm

RUN yum --disableplugin=subscription-manager update -y && \
    yum --disableplugin=subscription-manager install -y \
    make \
    cmake \
    git \
    gcc \
    gcc-c++ \
    wget \
    rpm-build \
    re2-devel \
    zlib-devel \
    openssl-devel

RUN yum --disableplugin=subscription-manager clean all

WORKDIR /tmp

# Fetch aneo's grpc rpm packages
RUN wget https://github.com/aneoconsulting/grpc-rpm/releases/download/1.62.2.0/grpc-1.62.2-1.el8.x86_64.rpm && \
    rpm -ivh grpc-1.62.2-1.el8.x86_64.rpm

RUN wget https://github.com/aneoconsulting/grpc-rpm/releases/download/1.62.2.0/grpc-devel-1.62.2-1.el8.x86_64.rpm && \
    rpm -ivh grpc-devel-1.62.2-1.el8.x86_64.rpm

RUN rm -rf *.rpm

ENV protobuf_BUILD_TESTS=OFF
RUN mkdir -p /worker/install/

RUN git clone https://github.com/aneoconsulting/ArmoniK.Api.git -b jf/cpp-static /armonik_api_temp/
WORKDIR /armonik_api_temp/packages/cpp/
RUN mkdir ./install

RUN mkdir ./build
WORKDIR /armonik_api_temp/packages/cpp/build
RUN cmake "-DCMAKE_BUILD_TYPE=Debug" \
     "-DPROTO_FILES_DIR=/armonik_api_temp/Protos/V1/" \
     "-DCMAKE_INSTALL_PREFIX=/armonik_api_temp/packages/cpp/install/" \
     "-DSTATIC_LINK=ON" \
     "-DBUILD_TEST=OFF" \
     /armonik_api_temp/packages/cpp/  && make -j ${PARALLEL_JOBS} install
WORKDIR /
RUN cp -a /armonik_api_temp/packages/cpp/install/. /worker/install/
RUN rm -rf /armonik_api_temp/

RUN export LD_LIBRARY_PATH="/worker/install/lib:$LD_LIBRARY_PATH"
RUN export PATH="/worker/install/bin:$PATH"

COPY ./src/ /worker/src/
COPY ./CMakeLists.txt /worker/

RUN cmake -DCMAKE_INSTALL_PREFIX="/worker/install" /worker/
RUN make -j ${PARALLEL_JOBS} install

FROM registry.access.redhat.com/ubi8 AS runner

RUN rpm -ivh https://dl.fedoraproject.org/pub/epel/epel-release-latest-8.noarch.rpm

RUN yum --disableplugin=subscription-manager update -y && \
    yum --disableplugin=subscription-manager install -y \
    re2 \
    zlib \
    wget \
    openssl

RUN yum --disableplugin=subscription-manager clean all

WORKDIR /tmp
RUN wget https://github.com/aneoconsulting/grpc-rpm/releases/download/1.62.2.0/grpc-1.62.2-1.el8.x86_64.rpm && \
    rpm -ivh grpc-1.62.2-1.el8.x86_64.rpm && rm grpc-1.62.2-1.el8.x86_64.rpm

RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser && mkdir /cache && chown armonikuser: /cache
USER armonikuser

COPY --from=builder /worker /worker

RUN export LD_LIBRARY_PATH="/worker/install/lib:$LD_LIBRARY_PATH"
RUN export PATH="/worker/install/bin:$PATH"

ENTRYPOINT ["/worker/install/bin/armonik_cpp_hello"]
