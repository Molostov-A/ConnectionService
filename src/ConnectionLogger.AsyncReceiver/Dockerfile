FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY src/ConnectionLogger.AsyncReceiver/ConnectionLogger.AsyncReceiver.csproj ./ConnectionLogger.AsyncReceiver/
COPY src/ConnectionLogger.Messaging/ConnectionLogger.Messaging.csproj ./ConnectionLogger.Messaging/
RUN dotnet restore ConnectionLogger.AsyncReceiver/ConnectionLogger.AsyncReceiver.csproj

COPY src/ConnectionLogger.AsyncReceiver/ ./ConnectionLogger.AsyncReceiver/
COPY src/ConnectionLogger.Messaging/ ./ConnectionLogger.Messaging/

WORKDIR /app/ConnectionLogger.AsyncReceiver
RUN dotnet build ConnectionLogger.AsyncReceiver.csproj -c Development -o /app/build

RUN dotnet publish ConnectionLogger.AsyncReceiver.csproj -c Development -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ConnectionLogger.AsyncReceiver.dll"]
