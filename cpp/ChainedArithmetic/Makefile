WORKING_DIR := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))

.ONESHELL:
CLIENT_IMAGE_TAG?= armonik-samples-cpp-chained-arithmetic-client:0.0.0-sdk
WORKER_IMAGE_TAG?= armonik-samples-cpp-chained-arithmetic-worker:0.0.0-sdk
API_VERSION?= $(shell . $(WORKING_DIR)/../versions.env && echo $$API_VERSION)
SDK_VERSION?= $(shell . $(WORKING_DIR)/../versions.env && echo $$CHAINEDARITHMETIC_SDK_VERSION)

.PHONY: help
help: 	## Show help
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@fgrep "##" Makefile | fgrep -v fgrep

all: build_worker build_client

.PHONY: build_client
build_client: ## Build the client Docker image
	@docker build -t ${CLIENT_IMAGE_TAG}  \
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
