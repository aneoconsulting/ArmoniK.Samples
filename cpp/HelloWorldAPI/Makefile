.ONESHELL:
WORKING_DIR := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))
WORKER_IMAGE_TAG?="armonik-cpp-hello-worker:0.1.0-api"
CLIENT_IMAGE_TAG?="armonik-cpp-hello-client:0.1.0-api"
API_VERSION?=3.29.0

.PHONY: help
help: 	## Show help
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@fgrep "##" Makefile | fgrep -v fgrep

all: build_worker build_client

.PHONY: build_client
build_client: ## Builds docker image for client (RHEL 8)
	@docker build -t ${CLIENT_IMAGE_TAG}  \
		--build-arg="API_VERSION=${API_VERSION}" \
		-f "${WORKING_DIR}/client/Dockerfile" \
		${WORKING_DIR}/client

.PHONY: build_worker
build_worker: ## Builds docker image for worker (RHEL 8)
	@docker build -t ${WORKER_IMAGE_TAG} \
		--build-arg="API_VERSION=${API_VERSION}" \
		-f "${WORKING_DIR}/worker/Dockerfile" \
		${WORKING_DIR}/worker

.PHONY: build_client_debian
build_client_debian: ## Builds docker image for client (Debian 12)
	@docker build -t ${CLIENT_IMAGE_TAG}-debian \
		--build-arg="API_VERSION=${API_VERSION}" \
		-f "${WORKING_DIR}/client/Dockerfile.debian" \
		${WORKING_DIR}/client

.PHONY: build_worker_debian
build_worker_debian: ## Builds docker image for worker (Debian 12)
	@docker build -t ${WORKER_IMAGE_TAG}-debian \
		--build-arg="API_VERSION=${API_VERSION}" \
		-f "${WORKING_DIR}/worker/Dockerfile.debian" \
		${WORKING_DIR}/worker

