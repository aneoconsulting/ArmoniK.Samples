# Python API

Here you will find how to use the Python API of ArmoniK.

To use the example please follow the steps below:

- Build the worker using docker
```shell
# In the current folder
docker build -t armonik_test_python:latest .
```
- Deploy ArmoniK according to the main repository (https://github.com/aneoconsulting/ArmoniK)
- Add or modify a partition to use your worker (https://github.com/aneoconsulting/ArmoniK/blob/main/.docs/content/2.guide/1.how-to/how-to-configure-partitions.md)
- Create a virtual environment for your test and source it
```shell
python -m venv ./armonik
source ./armonik/bin/activate
```
- Install the requirements
```shell
python -m pip install -r requirements.txt
```
- Launch the client with the deployed endpoint url without the scheme, and with a number N. The client will task ArmoniK to compute the sum of the N first natural integers.
```shell
# If you deployed the worker on partition "partition" with the endpoint http://127.0.0.1:5001
python ./client.py -e "127.0.0.1:5001" -p "partition" 100
# If you deployed the worker on the default partition with the endpoint http://127.0.0.1:5001
python ./client.py -e "127.0.0.1:5001" 100
```
