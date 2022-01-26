#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GridWorker/src/ArmoniK.Samples.HtcMock.GridWorker.csproj", "GridWorker/src/"]
COPY ["Adapter/src/ArmoniK.Samples.HtcMock.Adapter.csproj", "Adapter/src/"]
RUN dotnet restore "GridWorker/src/ArmoniK.Samples.HtcMock.GridWorker.csproj"
COPY . .
WORKDIR "/src/GridWorker/src"
RUN dotnet build "ArmoniK.Samples.HtcMock.GridWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.HtcMock.GridWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.HtcMock.GridWorker.dll"]