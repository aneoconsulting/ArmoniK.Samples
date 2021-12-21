#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Samples/HelloWorld/Client/src/ArmoniK.HelloWorld.Client.csproj", "Samples/HelloWorld/Client/src/"]
COPY ["DevelopmentKit/csharp/Generated/src/ArmoniK.Csharp.csproj", "DevelopmentKit/csharp/Generated/src/"]
RUN dotnet restore "Samples/HelloWorld/Client/src/ArmoniK.HelloWorld.Client.csproj"
COPY . .
WORKDIR "/src/Samples/HelloWorld/Client/src"
RUN dotnet build "ArmoniK.HelloWorld.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.HelloWorld.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.HelloWorld.Client.dll"]