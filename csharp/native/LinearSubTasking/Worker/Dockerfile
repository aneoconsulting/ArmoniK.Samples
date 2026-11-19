# Chiseled image: no shell, no package manager, non-root by default (UID 1654).
# In an ArmoniK deployment the compute-plane pod sets the user (UID 5000 by default)
# and mounts /cache/shared and /cache/internal as emptyDir volumes.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra@sha256:cae5b962bfb786606def03d250c6a645cc98839095c89e806e4bd8978a68115b AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0@sha256:fe0fd4e93bd2fbb47c883b7a6369e5c8d3db8bf01cdc5981f6ee35fbdf3ff4fb AS build
WORKDIR /src
COPY ["LinearSubTasking/Worker/ArmoniK.Samples.LinearSubTasking.Worker.csproj", "LinearSubTasking/Worker/"]
RUN dotnet restore "LinearSubTasking/Worker/ArmoniK.Samples.LinearSubTasking.Worker.csproj"
COPY . .
WORKDIR "/src/LinearSubTasking/Worker"
RUN dotnet build "ArmoniK.Samples.LinearSubTasking.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.LinearSubTasking.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.LinearSubTasking.Worker.dll"]
