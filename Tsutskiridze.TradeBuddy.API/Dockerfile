# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Create a non-root user and group safely in the base image.
RUN if ! getent group app > /dev/null; then groupadd -g 1001 app; fi && \
    if ! id -u app > /dev/null 2>&1; then useradd -m -u 1001 -g app app; fi

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tsutskiridze.TradeBuddy.API.csproj", "Tsutskiridze.TradeBuddy.API/"]
RUN dotnet restore "./Tsutskiridze.TradeBuddy.API/Tsutskiridze.TradeBuddy.API.csproj"

WORKDIR "/src/Tsutskiridze.TradeBuddy.API"
COPY . .

RUN dotnet build "./Tsutskiridze.TradeBuddy.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tsutskiridze.TradeBuddy.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.API.dll"]
