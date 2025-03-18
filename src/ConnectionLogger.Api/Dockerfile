FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["src/ConnectionLogger.Api/ConnectionLogger.Api.csproj", "src/ConnectionLogger.Api/"]
RUN dotnet restore "src/ConnectionLogger.Api/ConnectionLogger.Api.csproj"

COPY . .

WORKDIR /app/src/ConnectionLogger.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ConnectionLogger.Api.dll"]