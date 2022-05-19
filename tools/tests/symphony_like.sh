#!/bin/bash
set -e

BASEDIR=$(dirname "$0")
pushd $BASEDIR
BASEDIR=$(pwd -P)
popd

export MODE=""
export SERVER_NFS_IP=""
export STORAGE_TYPE="HostPath"

RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color
DRY_RUN="${DRY_RUN:-0}"

pushd $(dirname $0) >/dev/null 2>&1
BASEDIR=$(pwd -P)
popd >/dev/null 2>&1

configuration=Debug
FRAMEWORK=net6.0

TestDir=${BASEDIR}/../../Samples/SymphonyLike/

OUTPUT_JSON="nofile"

cd ${TestDir}

export CPIP=$(kubectl get svc ingress -n armonik -o custom-columns="IP:.spec.clusterIP" --no-headers=true)
export CPPort=$(kubectl get svc ingress -n armonik -o custom-columns="PORT:.spec.ports[1].port" --no-headers=true)
export Grpc__Endpoint=http://$CPIP:$CPPort
nuget_cache=$(dotnet nuget locals global-packages --list | awk '{ print $2 }')

function SSLConnection()
{
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
    link=`cat ${OUTPUT_JSON} | jq -e '.armonik.ingress.control_plane'`
    if [ "$?" == "1" ]; then
      link=`cat ${OUTPUT_JSON} | jq -e '.armonik.control_plane_url'`
      if [ "$?" == "1" ]; then
        echo "Error : cannot read Endpoint from file ${OUTPUT_JSON}"
        exit 1
      fi
    fi
  else
    export CPIP=$(kubectl get svc ingress -n armonik -o custom-columns="IP:.spec.clusterIP" --no-headers=true)
    export CPPort=$(kubectl get svc ingress -n armonik -o custom-columns="PORT:.spec.ports[1].port" --no-headers=true)
    export Grpc__Endpoint=http://$CPIP:$CPPort
  fi
  echo "Running with endPoint ${Grpc__Endpoint} from output.json"
}

function GetGrpcEndPoint()
{
    export Grpc__Endpoint=$1
    echo "Running with endPoint ${Grpc__Endpoint}"
}


echo "Need to create Data folder for application"
mkdir -p ${HOME}/data

function build() {
  cd ${TestDir}/
  echo rm -rf ${nuget_cache}/armonik.*
  rm -rf $(dotnet nuget locals global-packages --list | awk '{ print $2 }')/armonik.*
  find \( -iname obj -o -iname bin \) -exec rm -rf {} +
  dotnet publish --self-contained -c Debug -r linux-x64 -f ${FRAMEWORK} .
}

function deploy() {
  cd ${TestDir}
  cp packages/ArmoniK.Samples.SymphonyPackage-v2.0.0.zip ${HOME}/data
  kubectl delete -n armonik $(kubectl get pods -n armonik -l service=compute-plane --no-headers=true -o name) || true
}

function execute() {
  echo "cd ${TestDir}/ArmoniK.Samples.SymphonyClient/"
  cd ${TestDir}/ArmoniK.Samples.SymphonyClient/
  echo dotnet bin/Debug/${FRAMEWORK}/linux-x64/ArmoniK.Samples.SymphonyClient.dll $@
  dotnet bin/Debug/${FRAMEWORK}/linux-x64/ArmoniK.Samples.SymphonyClient.dll $@
}

function usage() {
  echo "Usage: $0 [option...]  with : " >&2
  echo
  cat <<-EOF
        no option           : To build and Run tests
        -s                  : To run in SSL mode
        -e http://endPoint  : change GRPC endpoint
        -f output_path.json : Load EndPoint from Armonik/generated/output.json
        -b | --build        : To build only test and package
        -r | --run          : To run only deploy package and test
        -a                  : To run only deploy package and test
EOF
  echo
  exit 0
}

function printConfiguration() {
  echo "Running script $0 $@"
  echo
  echo "SSL check strong auth server [${Grpc__SSLValidation}]"
  echo "SSL Client file [${Grpc__ClientCert}}"
  echo
}

DEFAULT=FALSE
MODE=All
args=""

function main() {
args=()

while [ $# -ne 0 ]; do
  echo "NB Arguments : $#"

    case "$1" in
    -s)
      shift
      SSLConnection
      ;;

    -e | --endpoint)
      shift
      GetGrpcEndPoint "$1"
      shift
      ;;

    -f | --file)
      shift
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
