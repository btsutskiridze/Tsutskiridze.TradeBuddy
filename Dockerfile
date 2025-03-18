# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-focal AS build
WORKDIR /src
COPY Tsutskiridze.TradeBuddy.sln .
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj
COPY Tsutskiridze.TradeBuddy/ Tsutskiridze.TradeBuddy/
WORKDIR /src/Tsutskiridze.TradeBuddy
RUN dotnet publish -c Release -o /app/build

# Stage 2: Runtime
FROM mcr.microsoft.com/playwright:focal AS runtime
WORKDIR /app

# Copy the published app from build
COPY --from=build /app/build .

# (Optional) create a non-root user if you’d like
RUN if ! getent group app > /dev/null; then groupadd -g 1001 app; fi && \
    if ! id -u app > /dev/null 2>&1; then useradd -m -u 1001 -g app app; fi

USER app

EXPOSE 80
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
