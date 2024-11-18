FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

WORKDIR /app

FROM  mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LinearMultiplicationSubTasking/Client/ArmoniK.Samples.LinearMultiplicationSubTasking.Client.csproj", "LinearMultiplicationSubTasking/Client/"]
RUN dotnet restore "LinearMultiplicationSubTasking/Client/ArmoniK.Samples.LinearMultiplicationSubTasking.Client.csproj"
COPY . .
WORKDIR "/src/LinearMultiplicationSubTasking/Client/"
RUN dotnet build "ArmoniK.Samples.LinearMultiplicationSubTasking.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmoniK.Samples.LinearMultiplicationSubTasking.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmoniK.Samples.LinearMultiplicationSubTasking.Client.dll"]
