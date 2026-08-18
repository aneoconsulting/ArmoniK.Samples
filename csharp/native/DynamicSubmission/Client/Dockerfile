FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base

WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SubTasking/Client/ArmoniK.Samples.SubTasking.Client.csproj", "SubTasking/Client/"]
RUN dotnet restore "SubTasking/Client/ArmoniK.Samples.SubTasking.Client.csproj"
COPY . .
WORKDIR "/src/SubTasking/Client/"
RUN dotnet build "ArmoniK.Samples.SubTasking.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.SubTasking.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.SubTasking.Client.dll"]
