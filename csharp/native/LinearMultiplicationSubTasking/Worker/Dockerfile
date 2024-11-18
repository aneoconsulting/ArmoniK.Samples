FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LinearMultiplicationSubTasking/Worker/ArmoniK.Samples.LinearMultiplicationSubTasking.Worker.csproj", "LinearMultiplicationSubTasking/Worker/"]
RUN dotnet restore "LinearMultiplicationSubTasking/Worker/ArmoniK.Samples.LinearMultiplicationSubTasking.Worker.csproj"
COPY . .
WORKDIR "/src/LinearMultiplicationSubTasking/Worker/"
RUN dotnet build "ArmoniK.Samples.LinearMultiplicationSubTasking.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.LinearMultiplicationSubTasking.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN groupadd --gid 5000 armonikuser && useradd --home-dir /home/armonikuser --create-home --uid 5000 --gid 5000 --shell /bin/sh --skel /dev/null armonikuser
RUN mkdir /cache && chown armonikuser: /cache
USER armonikuser
EXPOSE 1080
ENTRYPOINT ["dotnet", "ArmoniK.Samples.LinearMultiplicationSubTasking.Worker.dll"]
