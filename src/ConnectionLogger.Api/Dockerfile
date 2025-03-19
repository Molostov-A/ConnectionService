FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/ConnectionLogger.Api/ConnectionLogger.Api.csproj ./ConnectionLogger.Api/
COPY src/ConnectionLogger.Messaging/ConnectionLogger.Messaging.csproj ./ConnectionLogger.Messaging/
COPY src/ConnectionLogger.Data/ConnectionLogger.Data.csproj ./ConnectionLogger.Data/

RUN dotnet restore ConnectionLogger.Api/ConnectionLogger.Api.csproj

COPY src/ ./ 

WORKDIR /app/ConnectionLogger.Api
RUN dotnet build ConnectionLogger.Api.csproj -c Development -o /app/build

RUN dotnet publish ConnectionLogger.Api.csproj -c Development -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConnectionLogger.Api.dll"]
