WORKING_DIR := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))

.ONESHELL:
CLIENT_IMAGE_TAG?= armonik-samples-cpp-quantlib-client:0.0.0-sdk
API_VERSION?= $(shell . $(WORKING_DIR)/../versions.env && echo $$API_VERSION)
SDK_VERSION?= $(shell . $(WORKING_DIR)/../versions.env && echo $$SDK_VERSION)

.PHONY: help
help: 	## Show help
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@fgrep "##" Makefile | fgrep -v fgrep

.PHONY: build_client
build_client: ## Build the client Docker image
	@docker build -t ${CLIENT_IMAGE_TAG}  \
		-f "${WORKING_DIR}/client/Dockerfile" \
		--build-arg="API_VERSION=${API_VERSION}" \
		--build-arg="SDK_VERSION=${SDK_VERSION}" \
		${WORKING_DIR}/client
