#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY . .
#RUN dotnet restore "Bench/Bench.csproj"

WORKDIR "/src/Samples/StressTests/Armonik.Samples.StressTests.Client/"
RUN dotnet publish "Armonik.Samples.StressTests.Client.csproj" -c Release -o /app/publish
#correction of file format windows / linux
#RUN sed -i $'1s/^\uFEFF//' startup.sh
RUN sed -i -e 's/\r$//' startup.sh
RUN cp startup.sh /app/publish
RUN ls /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 777 . -R

# control-plane is the service name of the control plane in armonik if you are inside the kubernetes cluster.
# those parameters can be overriden in the terraform deployments scripts in modules\armonik folder
ENV Grpc__Endpoint http://control-plane:5001
ENV Grpc__SSLValidation "disable"
ENV Grpc__CaCert ""
ENV Grpc__ClientCert ""
ENV Grpc__ClientKey ""
ENV Grpc__mTLS "false"

RUN ls -ail

ENTRYPOINT ["bash", "-c"]
CMD ["./startup.sh"]
