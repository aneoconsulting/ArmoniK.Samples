WORKING_DIR := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))

.ONESHELL:
CLIENT_IMAGE_TAG?= armonik-samples-cpp-hello-client:0.0.0-sdk
WORKER_IMAGE_TAG?= armonik-samples-cpp-hello-worker:0.0.0-sdk
API_VERSION?= 3.28.3
SDK_VERSION?= 0.4.2

.PHONY: help
help: 	## Show help
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@fgrep "##" Makefile | fgrep -v fgrep

all: build_worker build_client

.PHONY: build_client
build_client: ## Build the client Docker image
	@docker build -t ${CLIENT_IMAGE_TAG} \
		-f "${WORKING_DIR}/client/Dockerfile" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/client

.PHONY: build_worker
build_worker: ## Build the worker Docker image
	@docker build -t ${WORKER_IMAGE_TAG} \
		-f "${WORKING_DIR}/worker/Dockerfile" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/worker