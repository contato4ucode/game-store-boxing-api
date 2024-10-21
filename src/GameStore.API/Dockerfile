# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
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
COPY . .
WORKDIR "/src/GameStore.API"
RUN dotnet build "./GameStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GameStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameStore.API.dll"]