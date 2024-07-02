FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LinearSubTasking/Client/ArmoniK.Samples.LinearSubTasking.Client.csproj", "LinearSubTasking/Client/"]
RUN dotnet restore "LinearSubTasking/Client/ArmoniK.Samples.LinearSubTasking.Client.csproj"
COPY . .
WORKDIR "/src/LinearSubTasking/Client/"
RUN dotnet build "ArmoniK.Samples.LinearSubTasking.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.LinearSubTasking.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.LinearSubTasking.Client.dll"]
