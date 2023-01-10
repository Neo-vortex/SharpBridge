
# https://hub.docker.com/_/microsoft-dotnet
FROM  mcr.hamdocker.ir/dotnet/sdk:7.0 AS build

COPY . .
RUN dotnet publish -c Release -o /app

# final stage/image
FROM mcr.hamdocker.ir/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENV     ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "SharpBridge.dll"]
EXPOSE 5001

