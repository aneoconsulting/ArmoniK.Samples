# Chiseled image: no shell, no package manager, non-root by default (UID 1654).
# In an ArmoniK deployment the compute-plane pod sets the user (UID 5000 by default)
# and mounts /cache/shared and /cache/internal as emptyDir volumes.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MultipleResults/Worker/ArmoniK.Samples.MultipleResults.Worker.csproj", "MultipleResults/Worker/"]
RUN dotnet restore "MultipleResults/Worker/ArmoniK.Samples.MultipleResults.Worker.csproj"
COPY . .
WORKDIR "/src/MultipleResults/Worker"
RUN dotnet build "ArmoniK.Samples.MultipleResults.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.MultipleResults.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.MultipleResults.Worker.dll"]
