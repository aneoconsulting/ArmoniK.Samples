# Chiseled image: no shell, no package manager, non-root by default (UID 1654).
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra@sha256:b64d614c3c1c440abe6d57f510c5a4cfc0ccd56a9b96f96c1503efe4a7ac3e56 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0@sha256:fe0fd4e93bd2fbb47c883b7a6369e5c8d3db8bf01cdc5981f6ee35fbdf3ff4fb AS build
WORKDIR /src
COPY ["MultipleResults/Client/ArmoniK.Samples.MultipleResults.Client.csproj", "MultipleResults/Client/"]
RUN dotnet restore "MultipleResults/Client/ArmoniK.Samples.MultipleResults.Client.csproj"
COPY . .
WORKDIR "/src/MultipleResults/Client"
RUN dotnet build "ArmoniK.Samples.MultipleResults.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.MultipleResults.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.MultipleResults.Client.dll"]
