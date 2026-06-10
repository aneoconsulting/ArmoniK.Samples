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

RUN mkdir -p /client/install/

ARG API_VERSION
RUN wget "https://github.com/aneoconsulting/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.rpm" && \
    rpm -ivh libarmonik-${API_VERSION}-Linux.rpm && \
    rm libarmonik-${API_VERSION}-Linux.rpm

COPY ./src/ /client/src/
COPY ./CMakeLists.txt /client/

RUN cmake -DCMAKE_BUILD_TYPE=Debug -DCMAKE_INSTALL_PREFIX="/client/install" /client/
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

COPY --from=builder /client /client

RUN export LD_LIBRARY_PATH="/client/install/lib:$LD_LIBRARY_PATH"
RUN export PATH="/client/install/bin:$PATH"

ENTRYPOINT ["/client/install/bin/armonik_cpp_hello_client"]
