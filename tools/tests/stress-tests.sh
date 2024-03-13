#!/bin/bash
set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color
DRY_RUN="${DRY_RUN:-0}"

configuration="Debug"
FRAMEWORK="net6.0"
OUTPUT_JSON="nofile"
TO_BUCKET=false
TO_DIRECTORY=true
PACKAGE_NAME="Armonik.Samples.StressTests.Worker-v1.0.0-700.zip"
RELATIVE_PROJECT="../../Samples/StressTests"
RELATIVE_CLIENT="Armonik.Samples.StressTests.Client"
args=""

BASEDIR=$(dirname "$0")
pushd $BASEDIR
BASEDIR=$(pwd -P)
popd

pushd $(dirname $0) >/dev/null 2>&1
BASEDIR=$(pwd -P)
popd >/dev/null 2>&1

TestDir=${BASEDIR}/$RELATIVE_PROJECT
cd ${TestDir}

CPIP=$(kubectl get svc ingress -n armonik -o jsonpath="{.status.loadBalancer.ingress[0]."ip"}" || echo "failure")
CPHOST=$(kubectl get svc ingress -n armonik -o jsonpath="{.status.loadBalancer.ingress[0]."hostname"}" || echo "failure")
export CPIP=${CPHOST:-$CPIP}
export CPPort=$(kubectl get svc ingress -n armonik -o custom-columns="PORT:.spec.ports[1].port" --no-headers=true || echo "failure")
if [[ "$ARMONIK_SHARED_HOST_PATH" == "" ]]; then
  export DATA_PATH=`kubectl get secret -n armonik shared-storage -o jsonpath="{.data.host_path}" 2>/dev/null | base64 -d`
else
  export DATA_PATH="$ARMONIK_SHARED_HOST_PATH"
fi
export Grpc__Endpoint=http://$CPIP:$CPPort
export Grpc__SSLValidation="disable"
export Grpc__CaCert=""
export Grpc__ClientCert=""
export Grpc__ClientKey=""
export Grpc__mTLS="false"

nuget_cache=$(dotnet nuget locals global-packages --list | awk '{ print $2 }')

function createLocalDirectory() {
    if [[ ${TO_BUCKET} == false && ${TO_DIRECTORY} == true ]]; then
      if [[ $DATA_PATH == "" ]]; then
        echo "Could not retrieve data folder from kubernetes secret or ARMONIK_SHARED_HOST_PATH environment variable"
        exit 1
      fi
      echo "Need to create Data folder \"${DATA_PATH}\" for application"
      mkdir -p "${DATA_PATH}"
    fi
}

function SSLConnection()
{
    export Grpc__mTLS="true"
    export Grpc__Endpoint=https://$CPIP:$CPPort
    export Grpc__SSLValidation="disable"
    export Grpc__CaCert=${BASEDIR}/../../../../infrastructure/quick-deploy/localhost/generated/certificates/ingress/ca.crt
    export Grpc__ClientCert=${BASEDIR}/../../../../infrastructure/quick-deploy/localhost/generated/certificates/ingress/client.crt
    export Grpc__ClientKey=${BASEDIR}/../../../../infrastructure/quick-deploy/localhost/generated/certificates/ingress/client.key
}

function GetGrpcEndPointFromFile()
{
  OUTPUT_JSON=$1
  if [ -f "${OUTPUT_JSON}" ]; then
    #Test if ingress exists
    echo "Input file: ${OUTPUT_JSON}"
    link=`cat "${OUTPUT_JSON}" | jq -r -e '.armonik.ingress.control_plane_url'`
    if [ "$?" == "1" ]; then
      link=`cat "${OUTPUT_JSON}" | jq -r -e '.armonik.control_plane_url'`
      if [ "$?" == "1" ]; then
        echo "Error : cannot read Endpoint from file ${OUTPUT_JSON}"
        exit 1
      fi
    fi
    export Grpc__Endpoint="$link"
  fi
  echo "Running with endPoint ${Grpc__Endpoint} from output.json"
}

function GetGrpcEndPoint()
{
    export Grpc__Endpoint=$1
    echo "Running with endPoint ${Grpc__Endpoint}"
}

function build() {
  cd "${TestDir}/"
  echo "rm -rf ${nuget_cache}/armonik.*"
  rm -rf "$(dotnet nuget locals global-packages --list | awk '{ print $2 }')/armonik.*"
  find \( -iname obj -o -iname bin \) -exec rm -rf {} +
  dotnet publish --self-contained -c ${configuration} -r linux-x64 -f ${FRAMEWORK} .
}

function deploy() {
  cd "${TestDir}"
  if [[ ${TO_BUCKET} == true ]]; then
    export S3_BUCKET=$(aws s3api list-buckets --output json | jq -r '.Buckets[].Name' | grep "s3fs")
    echo "Copy of S3 Bucket ${TO_BUCKET}"
    aws s3 cp "packages/${PACKAGE_NAME}" "s3://${S3_BUCKET}"
  elif [[ ${TO_DIRECTORY} == true ]]; then
    cp -v "packages/${PACKAGE_NAME}" "${DATA_PATH}"
  else
    echo "Dll is not copied to shared storage !"
  fi
  kubectl delete -n armonik $(kubectl get pods -n armonik -l service=compute-plane --no-headers=true -o name) || true
}

function execute() {
  echo "cd ${TestDir}/${RELATIVE_CLIENT}/"
  cd "${TestDir}/${RELATIVE_CLIENT}/"
  echo dotnet run -r linux-x64 -f net6.0 -c ${configuration} $@
  dotnet run -r linux-x64 -f net6.0 -c ${configuration} -- $@
}

function usage() {
  echo "Usage: $0 [script options...] [-- binary options...] with : " >&2
  echo
  cat <<-EOF
        no option           : To build and Run tests
        -ssl                : To run in SSL mode
        -e http://endPoint  : change GRPC endpoint
        -f output_path.json : Load EndPoint from Armonik/generated/output.json
        -b | --build        : To build only test and package
        -d | --deploy       : Only Deploy package
        -r | --run          : To run only deploy package and test
        -a                  : To run only deploy package and test
        -s3                 : Need S3 copy with aws cp
        -no-copy-dll        : Don't copy dll anywhere
EOF
  echo
  exit 0
}

# shellcheck disable=SC2120
function printConfiguration() {
  echo "Running script $0 $@"
  echo
  echo "SSL check strong auth server [${Grpc__SSLValidation}]"
  echo "SSL Client file [${Grpc__ClientCert}]"
  echo
}

function main() {
args=()
binargs=()

while [ $# -ne 0 ]; do
  echo "NB Arguments : $#"

    case "$1" in
    -ssl)
      SSLConnection
      shift
      ;;

    -s3)
      TO_BUCKET=true
      shift
      ;;

    -no-copy-dll)
      TO_DIRECTORY=false
      TO_BUCKET=false
      shift
      ;;

    -e | --endpoint)
      GetGrpcEndPoint "$2"
      shift
      shift
      ;;

    -f | --file)
      GetGrpcEndPointFromFile "$2"
      shift
      shift
      ;;

    -h | --help)
      usage
      exit
      ;;

    --)
      shift
      while [ $# -ne 0 ]; do
        binargs+=("$1")
        shift
      done
      echo "Binray arguments : ${binargs[*]}"
      break
      ;;

    *)
      echo "Add Args '$1' to list ${args[*]}"
      args+=("$1")
      shift
      ;;
    esac
  done

  createLocalDirectory
  printConfiguration

echo "List of args : ${args[*]}"

 if [[ "${#args[@]}" == 0 ]]; then
    echo "Execute default run"
    build
    deploy
    execute "${binargs[@]}"
    exit 0
  fi

  while [ ${#args[@]} -ne 0 ]; do
    echo "Parse ${args[0]}"
    case "${args[0]}" in
    -r | --run)
      args=("${args[@]:1}") # past argument=value
      echo "Only execute without build '${binargs[@]}'"
      execute "${binargs[@]}"
      break
      ;;
    -b | --build)
      args=("${args[@]:1}") # past argument=value
      echo "Only execute without build '${binargs[@]}'"
      build
      break
      ;;
    -d | --deploy)
      args=("${args[@]:1}") # past argument=value
      echo "Only deploy package'${binargs[@]}'"
      deploy
      break
      ;;
    -a)
      # all build and execute
	    args=("${args[@]:1}") # past argument=value
      echo "Build and execute '${binargs[@]}'"
      break
      build
      deploy
      execute "${binargs[@]}"
      ;;
    *)
	# unknown option
      echo "Running all with args ['${binargs[@]}']"
      echo "Build and execute '${binargs[@]}'"
      build
      deploy
      execute "${binargs[@]}"
      exit 0
      ;;
    esac
  done
}

main "$@"
