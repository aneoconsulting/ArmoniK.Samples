# Azure Linux distroless image: no shell, no package manager, non-root by default (UID 1654).
# In an ArmoniK deployment the compute-plane pod sets the user (UID 5000 by default)
# and mounts /cache/shared and /cache/internal as emptyDir volumes.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-azurelinux3.0-distroless-extra AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HelloWorld/Worker/ArmoniK.Samples.HelloWorld.Worker.csproj", "HelloWorld/Worker/"]
RUN dotnet restore "HelloWorld/Worker/ArmoniK.Samples.HelloWorld.Worker.csproj"
COPY . .
WORKDIR "/src/HelloWorld/Worker"
RUN dotnet build "ArmoniK.Samples.HelloWorld.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.HelloWorld.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.HelloWorld.Worker.dll"]
