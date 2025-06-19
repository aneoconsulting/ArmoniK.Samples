export ARMONIK_SHARED_HOST_PATH=/home/nicodl/code/ArmoniK/infrastructure/quick-deploy/localhost/data
cd /home/nicodl/code/ArmoniK.Samples/tools/tests/

./unified_api.sh -e http://192.168.252.47:5001 -no-copy-dll -r -- addition --nbTask 20