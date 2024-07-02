FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LinearSubTasking/Worker/ArmoniK.Samples.LinearSubTasking.Worker.csproj", "LinearSubTasking/Worker/"]
RUN dotnet restore "LinearSubTasking/Worker/ArmoniK.Samples.LinearSubTasking.Worker.csproj"
COPY . .
WORKDIR "/src/LinearSubTasking/Worker/"
RUN dotnet build "ArmoniK.Samples.LinearSubTasking.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.LinearSubTasking.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser
RUN mkdir /cache && chown armonikuser: /cache
USER armonikuser
EXPOSE 1080
ENTRYPOINT ["dotnet", "ArmoniK.Samples.LinearSubTasking.Worker.dll"]
