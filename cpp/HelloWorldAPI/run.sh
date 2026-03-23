DEFAULT_TAG="0.1.0-api"
DEFAULT_PARTITION="apisocket"

usage() {
    echo "Usage: $0 [-t TAG] [-p PARTITION] [-a USE_AUTH]"
    echo "TAG: Core tag (defaults to $DEFAULT_TAG)"
    echo "PARTITION: partition name (defaults to $DEFAULT_PARTITION)"
    echo "USE_AUTH: use auth credentials (defaults to false)"
    exit 1
}

while getopts ":t:p:a:" opt; do
  case ${opt} in
    t )
      ARG_TAG=$OPTARG
      ;;
    p )
      ARG_PARTITION=$OPTARG
      ;;
    a )
      ARG_AUTH=$OPTARG
      ;;
    \? )
      usage
      ;;
  esac
done

VERSION="${ARG_TAG:-$DEFAULT_TAG}"
PARTITION="${ARG_PARTITION:-$DEFAULT_PARTITION}"

if [ -z "$AKCONFIG" ]; then
  echo "Error: AKCONFIG is not defined"
  exit 1
fi
GRPC_CLIENT_END_POINT=https://192.168.1.47:5001

CERTS_DIR="/home/jose/repos/ArmoniK/infrastructure/quick-deploy/localhost/generated/certificates/ingress"

if [ "x${ARG_AUTH}" == "xtrue" ]; then
AUTH_OPTS=$(cat<<EOF
  -u $UID:$(id -g)
  -v $CERTS_DIR:/app/certs
  -e GrpcClient__CaCert=/app/certs/ca.crt
  -e GrpcClient__CertP12=/app/certs/custom.submitter.p12
  -e GrpcClient__AllowUnsafeConnection=true
EOF
)
fi

docker run --rm \
  $AUTH_OPTS \
  -e GrpcClient__Endpoint=$GRPC_CLIENT_END_POINT \
  -e PartitionId=$PARTITION armonik-cpp-hello-client:$VERSION
