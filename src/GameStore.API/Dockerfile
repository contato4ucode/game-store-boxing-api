FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/GameStore.API/GameStore.API.csproj", "src/GameStore.API/"]
COPY ["src/GameStore.BoxingService/GameStore.BoxingService.csproj", "src/GameStore.BoxingService/"]
COPY ["src/GameStore.Domain/GameStore.Domain.csproj", "src/GameStore.Domain/"]
COPY ["src/GameStore.SharedServices/GameStore.SharedServices.csproj", "src/GameStore.SharedServices/"]
COPY ["src/GameStore.Identity/GameStore.Identity.csproj", "src/GameStore.Identity/"]
COPY ["src/GameStore.Infrastructure/GameStore.Infrastructure.csproj", "src/GameStore.Infrastructure/"]
COPY ["src/GameStore.Messaging/GameStore.Messaging.csproj", "src/GameStore.Messaging/"]
RUN dotnet restore "./src/GameStore.API/GameStore.API.csproj"

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
WORKDIR "/src/GameStore.API"
RUN dotnet build "./GameStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GameStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameStore.API.dll"]
