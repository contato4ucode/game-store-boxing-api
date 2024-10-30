FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/GameStore.API/GameStore.API.csproj", "GameStore.API/"]
COPY ["src/GameStore.BoxingService/GameStore.BoxingService.csproj", "GameStore.BoxingService/"]
COPY ["src/GameStore.Domain/GameStore.Domain.csproj", "GameStore.Domain/"]
COPY ["src/GameStore.SharedServices/GameStore.SharedServices.csproj", "GameStore.SharedServices/"]
COPY ["src/GameStore.Identity/GameStore.Identity.csproj", "GameStore.Identity/"]
COPY ["src/GameStore.Infrastructure/GameStore.Infrastructure.csproj", "GameStore.Infrastructure/"]
COPY ["src/GameStore.Messaging/GameStore.Messaging.csproj", "GameStore.Messaging/"]

RUN dotnet restore "GameStore.API/GameStore.API.csproj"

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .

WORKDIR "/src/GameStore.API"

RUN dotnet build "GameStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "GameStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "GameStore.API.dll"]
