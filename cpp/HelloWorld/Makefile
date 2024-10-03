.ONESHELL:
WORKER_IMAGE_TAG?="armonik-cpp-hello-worker:0.1.0"
CLIENT_IMAGE_TAG?="armonik-cpp-hello-client:0.1.0"

.PHONY: help
help: 	## Show help
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@fgrep "##" Makefile | fgrep -v fgrep 

all: build_worker build_client

.PHONY: build_client
build_client: ## Builds docker image for client
	@docker build -t ${CLIENT_IMAGE_TAG} -f ./Dockerfile.client .

.PHONY: build_worker
build_worker: ## Builds docker image for worker
	@docker build -t ${WORKER_IMAGE_TAG} -f ./Dockerfile.worker .


.PHONY: setup_dev_env
setup_dev_env:  # Gets ArmoniK APIs and other dependencies for autocomplete 
	@rm -rf ./armonik_api/
	@rm -rf ./armonik_api_temp/
	@rm -rf ./vendor_temp/
	@git clone https://github.com/aneoconsulting/ArmoniK.Api.git ./armonik_api_temp/
	@( \
		ORIGINAL_DIR=$$(pwd); \
		cd ./armonik_api_temp/packages/cpp/tools/ && ./compile.sh; \
		cd "$$ORIGINAL_DIR"; \
	)
	@mkdir -p ./armonik_api/proto
	@sudo mv armonik_api_temp/packages/cpp/install/. ./armonik_api/
	@sudo mv armonik_api_temp/Protos/V1/. ./armonik_api/proto/
	@rm -rf ./armonik_api_temp/
	# TODO: Should make it so I only clone specific folders to save time here
	@mkdir -p ./armonik_api/vendor/
	@git clone https://github.com/protocolbuffers/protobuf.git ./vendor_temp/proto/
	@sudo mv ./vendor_temp/proto/src/google ./armonik_api/vendor/
	@git clone https://github.com/abseil/abseil-cpp.git ./vendor_temp/abseil/
	@sudo mv ./vendor_temp/abseil/absl ./armonik_api/vendor/
	@git clone https://github.com/grpc/grpc.git ./vendor_temp/grpc
	@sudo mv ./vendor_temp/grpc/include/* ./armonik_api/vendor/
	@rm -rf ./vendor_temp/

