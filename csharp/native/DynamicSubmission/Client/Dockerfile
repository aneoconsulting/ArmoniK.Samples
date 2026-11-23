# Chiseled image: no shell, no package manager, non-root by default (UID 1654).
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["DynamicSubmission/Client/ArmoniK.Samples.DynamicSubmission.Client.csproj", "DynamicSubmission/Client/"]
RUN dotnet restore "DynamicSubmission/Client/ArmoniK.Samples.DynamicSubmission.Client.csproj"
COPY . .
WORKDIR "/src/DynamicSubmission/Client"
RUN dotnet build "ArmoniK.Samples.DynamicSubmission.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.DynamicSubmission.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.DynamicSubmission.Client.dll"]
