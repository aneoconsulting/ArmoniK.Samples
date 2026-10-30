FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src
COPY ["QuantLib/Client/ArmoniK.Samples.QuantLib.Client.csproj", "QuantLib/Client/"]
RUN dotnet restore -a "$TARGETARCH" "QuantLib/Client/ArmoniK.Samples.QuantLib.Client.csproj"
COPY QuantLib/Client QuantLib/Client
WORKDIR "/src/QuantLib/Client"
RUN dotnet publish -a "$TARGETARCH" "ArmoniK.Samples.QuantLib.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /data
COPY --from=build /app/publish /app
ENTRYPOINT ["dotnet", "/app/ArmoniK.Samples.QuantLib.Client.dll"]
