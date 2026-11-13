FROM debian:12@sha256:268fb24c39bca92d326dd83ea3abebe4a3f7751c804ffef23bb6d857dd343ff8 AS builder

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
    protobuf-compiler-grpc \
    nlohmann-json3-dev

WORKDIR /app
COPY CMakeLists.txt ./
COPY HelloServiceHandler.h ./
COPY main.cpp ./

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

# Runtime packages, from which only the shared libraries the binaries load go to the final image
FROM debian:12-slim@sha256:734a72b3d6142a71993b4bc580ca01d8ca687f77a2c422d96a961d3376b179bc AS runtime

# The exact runtime package: apt reads a bare 'libgrpc++' as the regex 'libgrpc+',
# which also installs libgrpc-dev, libgrpc++-dev and their toolchain dependencies.
RUN apt-get update && DEBIAN_FRONTEND="noninteractive" apt-get install --no-install-recommends -y \
    libgrpc++1.51 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=builder /lib/libArmoniK*.so* /lib/

WORKDIR /app
COPY --from=builder /app/build/ArmoniK.Samples.C.Hello.Client ./

# Copy each shared library the binaries load into /rootfs, under the name they load it by, except the
# glibc, libgcc, libstdc++ and OpenSSL libraries that the distroless base provides. Fails on a missing library.
# The dpkg record of the package owning each library goes to /var/lib/dpkg/status.d, as distroless does
# for its own packages, so that vulnerability scanners still see these libraries.
RUN set -e; bins="/app/ArmoniK.Samples.C.Hello.Client"; \
    if ldd $bins | grep "not found"; then exit 1; fi; \
    mkdir -p /rootfs; \
    ldd $bins | awk '$2 == "=>" && $3 ~ /^\// {print $1, $3}' | sort -u | while read -r name path; do \
      case "$name" in \
        ld-linux*|libc.so.*|libm.so.*|libdl.so.*|libpthread.so.*|librt.so.*|libresolv.so.*|libgcc_s.so.*|libstdc++.so.*|libssl.so.*|libcrypto.so.*) continue ;; \
      esac; \
      dir=$(dirname "$(readlink -f "$path")"); \
      mkdir -p "/rootfs$dir"; \
      cp -L "$path" "/rootfs$dir/$name"; \
      pkg=$( (dpkg -S "$path" || dpkg -S "$(readlink -f "$path")") 2>/dev/null | head -n1 | cut -d: -f1 ) || true; \
      if [ -n "$pkg" ]; then \
        mkdir -p /rootfs/var/lib/dpkg/status.d; \
        dpkg -s "$pkg" > "/rootfs/var/lib/dpkg/status.d/$pkg"; \
      fi; \
    done

# Distroless: no shell, no package manager, non-root by default (UID 65532)
FROM gcr.io/distroless/cc-debian13:nonroot@sha256:cb87acc574abe719b6e5c547384defa2f7b1f24af9605d3f618cf852495d8be2 AS runner

COPY --from=runtime /rootfs/ /
COPY --from=runtime /app /app
WORKDIR /app
ENTRYPOINT ["./ArmoniK.Samples.C.Hello.Client"]
