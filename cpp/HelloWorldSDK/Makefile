WORKING_DIR := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))

.ONESHELL:
CLIENT_IMAGE_TAG?= armonik-samples-cpp-hello-client:0.0.0-sdk
WORKER_IMAGE_TAG?= armonik-samples-cpp-hello-worker:0.0.0-sdk
API_VERSION?= 3.28.3
SDK_VERSION?= 0.5.1

.PHONY: help
help: 	## Show help
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@fgrep "##" Makefile | fgrep -v fgrep

all: build_worker_debian build_client_debian build_worker_ubi8 build_client_ubi8

.PHONY: build_client_debian
build_client_debian: ## Build the client Docker image (Debian 12)
	@docker build -t ${CLIENT_IMAGE_TAG}-debian \
		-f "${WORKING_DIR}/client/Dockerfile.debian" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/client

.PHONY: build_worker_debian
build_worker_debian: ## Build the worker Docker image (Debian 12)
	@docker build -t ${WORKER_IMAGE_TAG}-debian \
		-f "${WORKING_DIR}/worker/Dockerfile.debian" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/worker

.PHONY: build_client_ubi8
build_client_ubi8: ## Build the client Docker image (RHEL 8)
	@docker build -t ${CLIENT_IMAGE_TAG}-ubi8 \
		-f "${WORKING_DIR}/client/Dockerfile.ubi8" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/client

.PHONY: build_worker_ubi8
build_worker_ubi8: ## Build the worker Docker image (RHEL 8)
	@docker build -t ${WORKER_IMAGE_TAG}-ubi8 \
		-f "${WORKING_DIR}/worker/Dockerfile.ubi8" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/worker
