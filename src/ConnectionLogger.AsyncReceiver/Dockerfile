FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["src/ConnectionLogger.AsyncReceiver/ConnectionLogger.AsyncReceiver.csproj", "src/ConnectionLogger.AsyncReceiver/"]
RUN dotnet restore "src/ConnectionLogger.AsyncReceiver/ConnectionLogger.AsyncReceiver.csproj"

COPY . .

WORKDIR /app/src/ConnectionLogger.AsyncReceiver
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ConnectionLogger.AsyncReceiver.dll"]
