#!/bin/bash
set -e

export MODE="All"
export SERVER_NFS_IP=""
export STORAGE_TYPE="HostPath"
configuration=Debug
FRAMEWORK=net6.0
OUTPUT_JSON="nofile"
TO_BUCKET=false
PACKAGE_NAME="ArmoniK.Samples.SymphonyPackage-v2.0.0.zip"
RELATIVE_PROJECT="../../Samples/SymphonyLike"
RELATIVE_CLIENT="ArmoniK.Samples.SymphonyClient"
DEFAULT=FALSE
args=""

BASEDIR=$(dirname "$0")
pushd $BASEDIR
BASEDIR=$(pwd -P)
popd

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color
DRY_RUN="${DRY_RUN:-0}"

pushd $(dirname $0) >/dev/null 2>&1
BASEDIR=$(pwd -P)
popd >/dev/null 2>&1

TestDir=${BASEDIR}/$RELATIVE_PROJECT
cd ${TestDir}

export CPIP=$(kubectl get svc ingress -n armonik -o custom-columns="IP:.spec.clusterIP" --no-headers=true)
export CPPort=$(kubectl get svc ingress -n armonik -o custom-columns="PORT:.spec.ports[1].port" --no-headers=true)
export Grpc__Endpoint=http://$CPIP:$CPPort
export Grpc__SSLValidation="false"
export Grpc__CaCert=""
export Grpc__ClientCert=""
export Grpc__ClientKey=""
export Grpc__mTLS="false"

nuget_cache=$(dotnet nuget locals global-packages --list | awk '{ print $2 }')

function createLocalDirectory() {
    if [[ ${TO_BUCKET} == false ]]; then
      echo "Need to create Data folder \"${HOME}/data\" for application"
      mkdir -p ${HOME}/data
    fi
}

function SSLConnection()
{
    export Grpc__mTLS="true"
    export Grpc__Endpoint=https://$CPIP:$CPPort
    export Grpc__SSLValidation="disable"
    export Grpc__CaCert=${BASEDIR}/../../../../infrastructure/quick-deploy/localhost/armonik/generated/certificates/ingress/ca.crt
    export Grpc__ClientCert=${BASEDIR}/../../../../infrastructure/quick-deploy/localhost/armonik/generated/certificates/ingress/client.crt
    export Grpc__ClientKey=${BASEDIR}/../../../../infrastructure/quick-deploy/localhost/armonik/generated/certificates/ingress/client.key
}

function GetGrpcEndPointFromFile()
{
  OUTPUT_JSON=$1
  if [ -f ${OUTPUT_JSON} ]; then
    #Test if ingress exists
    link=`cat ${OUTPUT_JSON} | jq -r -e '.armonik.ingress.control_plane_url'`
    if [ "$?" == "1" ]; then
      link=`cat ${OUTPUT_JSON} | jq -r -e '.armonik.control_plane_url'`
      if [ "$?" == "1" ]; then
        echo "Error : cannot read Endpoint from file ${OUTPUT_JSON}"
        exit 1
      fi
    fi
    export Grpc__Endpoint=$link
  fi
  echo "Running with endPoint ${Grpc__Endpoint} from output.json"
}

function GetGrpcEndPoint()
{
    export Grpc__Endpoint=$1
    echo "Running with endPoint ${Grpc__Endpoint}"
}

function build() {
  cd ${TestDir}/
  echo rm -rf ${nuget_cache}/armonik.*
  rm -rf $(dotnet nuget locals global-packages --list | awk '{ print $2 }')/armonik.*
  find \( -iname obj -o -iname bin \) -exec rm -rf {} +
  dotnet publish --self-contained -c ${configuration} -r linux-x64 -f ${FRAMEWORK} .
}

function deploy() {
  cd ${TestDir}
  if [[ ${TO_BUCKET} == true ]]; then
    export S3_BUCKET=$(aws s3api list-buckets --output json | jq -r '.Buckets[0].Name')
    echo "Copy of S3 Bucket ${TO_BUCKET}"
    echo aws s3 cp packages/${PACKAGE_NAME} s3://$S3_BUCKET
  else
    cp -v packages/${PACKAGE_NAME} ${HOME}/data
  fi
  kubectl delete -n armonik $(kubectl get pods -n armonik -l service=compute-plane --no-headers=true -o name) || true
}

function execute() {
  echo "cd ${TestDir}/${RELATIVE_CLIENT}/"
  cd ${TestDir}/${RELATIVE_CLIENT}/
  echo dotnet run -r linux-x64 -f net6.0 -c ${configuration} $@
  dotnet run -r linux-x64 -f net6.0 -c ${configuration} $@
}

function usage() {
  echo "Usage: $0 [option...]  with : " >&2
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
EOF
  echo
  exit 0
}

function printConfiguration() {
  echo "Running script $0 $@"
  echo
  echo "SSL check strong auth server [${Grpc__SSLValidation}]"
  echo "SSL Client file [${Grpc__ClientCert}]"
  echo
}

function main() {
args=()

while [ $# -ne 0 ]; do
  echo "NB Arguments : $#"

    case "$1" in
    -ssl)
      shift
      SSLConnection
      ;;
    -s3)
      shift
      TO_BUCKET=true
      ;;
    -e | --endpoint)
      #shift
      GetGrpcEndPoint "$1"
      shift
      ;;

    -f | --file)
      #shift
      GetGrpcEndPointFromFile "$1"
      shift
      ;;

    -h | --help)
      usage
      exit
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
    execute "${args[@]}"
    exit 0
  fi

  while [ ${#args[@]} -ne 0 ]; do
    echo "Parse ${args[0]}"
    case "${args[0]}" in
    -r | --run)
      args=("${args[@]:1}") # past argument=value
      echo "Only execute without build '${args[@]}'"
      execute "${args[@]}"
      break
      ;;
    -b | --build)
      args=("${args[@]:1}") # past argument=value
      echo "Only execute without build '${args[@]}'"
      build
      break
      ;;
    -d | --deploy)
      args=("${args[@]:1}") # past argument=value
      echo "Only deploy package'${args[@]}'"
      deploy
      break
      ;;
    -a)
      # all build and execute
	    args=("${args[@]:1}") # past argument=value
      echo "Build and execute '${args[@]}'"
      break
      build
      deploy
      execute "${args[@]}"
      ;;
    *)
	# unknown option
      echo "Running all with args ['${args[@]}']"
      echo "Build and execute '${args[@]}'"
      build
      deploy
      execute "${args[@]}"
      exit 0
      ;;
    esac
  done
}

main "$@"
