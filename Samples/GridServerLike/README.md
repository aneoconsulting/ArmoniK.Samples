# ArmoniK SymphonyLike Samples
- [ArmoniK SymphonyLike Samples](#armonik-symphonylike-samples)
- [Introduction <a name="introduction"></a>](#introduction-)
- [Software prerequisites <a name="software-prerequisites"></a>](#software-prerequisites-)
- [How to start with sample SymphonyLike](#how-to-start-with-sample-symphonylike)
    - [Configure applications](#configure-applications)
    - [Compile the full solution and publish Server package](#compile-the-full-solution-and-publish-server-package)
    - [Publish package into mount filesystem](#publish-package-into-mount-filesystem)
    - [Configure a mount point for local application installation](#configure-a-mount-point-for-local-application-installation)
    - [Deploy new package to WSL /data directory](#deploy-new-package-to-wsl-data-directory)
    - [Restart Compute pod](#restart-compute-pod)
    - [Execute Armonik.Samples.SymphonyClient](#execute-armoniksamplessymphonyclient)
  - [How to debug your code remotely](#how-to-debug-your-code-remotely)

# Introduction <a name="introduction"></a>

You will find the Sample to execute tasks with Armonik with API Symphony like.
All API References to develop your own application within Armonik can be found in this [API documentation](Documentation/Home.md)

# Software prerequisites <a name="software-prerequisites"></a>

Before you start using the Samples, please follow the Armonik installation that you can find here :
[Install & deploy Armonik](https://github.com/aneoconsulting/armonik/blob/main/infrastructure/README.md)

Before starting, it is important to have correctly deployed the ArmoniK solution in Kubernetes. A command line from a console in WSL 2 is enough to check it
```bash
kubectl get po -n armonik
```

# How to start with sample SymphonyLike


### Configure applications
Go to folder of SymphonyLike
```bash
cd Samples/SymphonyLike/
```
Open the solution ArmoniK.sln with Visual Studio 2019 or later

In the project Armonik.Samples.SymphonyLikeClient

Open the file appsettings.json and change IP adress to your correct WSL address ip.

To get the Ip adress, in a terminal get the ip adress with this command line

```bash
ip addr show eth0 | grep "inet\b" | awk '{print $2}' | cut -d/ -f1
```
Copy the result and replace the adress in Section Grpc -> EndPoint
```json

  "Grpc": {
    "EndPoint": "http://XXX.XX.XX.XX:5001"
  },
```

### Compile the full solution and publish Server package
From Visual Studio, Rebuild the solution


### Publish package into mount filesystem

### Configure a mount point for local application installation
Please create /data directory to the deposit the package
```bash
sudo mkdir /data
```

### Deploy new package to WSL /data directory
From Armonik.Samples root directory :
```bash
cp -v Samples/SymphonyLike/packages/ArmoniK.Samples.SymphonyPackage-v1.0.0.zip /data/
```

### Restart Compute pod
```bash
kubectl delete -n armonik $(kubectl get pods -n armonik -l service=compute-plane --no-headers=true -o name)
```

### Execute Armonik.Samples.SymphonyClient

From Visual Studio, execute the project ArmoniK.Samples.SymplhonyClient within Visual Studio 2019 or later


## How to debug your code remotely
TODO explain how to connect to docker Compute-0 and attach the to the package
Need to configure the ssh in wsl
