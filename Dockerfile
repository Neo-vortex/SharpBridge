
# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /SharpBridge

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore --use-current-runtime  

# copy everything else and build app
COPY . .
RUN dotnet publish -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .
ENV     ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "SharpBridge.dll"]
EXPOSE 5001

