# Azure Linux distroless image: no shell, no package manager, non-root by default (UID 1654).
FROM mcr.microsoft.com/dotnet/runtime:10.0-azurelinux3.0-distroless-extra AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HelloWorld/Client/ArmoniK.Samples.HelloWorld.Client.csproj", "HelloWorld/Client/"]
RUN dotnet restore "HelloWorld/Client/ArmoniK.Samples.HelloWorld.Client.csproj"
COPY . .
WORKDIR "/src/HelloWorld/Client"
RUN dotnet build "ArmoniK.Samples.HelloWorld.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.HelloWorld.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.HelloWorld.Client.dll"]
