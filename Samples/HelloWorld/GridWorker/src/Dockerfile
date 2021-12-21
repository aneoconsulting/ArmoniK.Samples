#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Samples/HelloWorld/GridWorker/src/ArmoniK.HelloWorld.Worker.csproj", "Samples/HelloWorld/GridWorker/src/"]
COPY ["DevelopmentKit/csharp/Generated/src/ArmoniK.Csharp.csproj", "DevelopmentKit/csharp/Generated/src/"]
RUN dotnet restore "Samples/HelloWorld/GridWorker/src/ArmoniK.HelloWorld.Worker.csproj"
COPY . .
WORKDIR "/src/Samples/HelloWorld/GridWorker/src"
RUN dotnet build "ArmoniK.HelloWorld.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.HelloWorld.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.HelloWorld.Worker.dll"]