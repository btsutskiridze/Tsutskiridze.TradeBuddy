# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Ensure correct files are copied
COPY Tsutskiridze.TradeBuddy.sln ./
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/

# Restore dependencies
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj

# Copy remaining source code
COPY Tsutskiridze.TradeBuddy/ Tsutskiridze.TradeBuddy/

WORKDIR /src/Tsutskiridze.TradeBuddy

# Build and publish
RUN dotnet publish -c Release -o /app/build
RUN dotnet tool restore

# Install Playwright CLI globally and add it to PATH
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="$PATH:/root/.dotnet/tools"

# Install Chromium for Playwright
RUN dotnet playwright install chromium

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user and group
RUN getent group app || groupadd -g 1001 app && getent passwd app || useradd -m -u 1001 -g app app

# Install Chromium dependencies
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 libxcomposite1 \
    libxdamage1 libxrandr2 libcups2 libgbm1 libasound2 \
    libpangocairo-1.0-0 libpango-1.0-0 libxshmfence1 && \
    rm -rf /var/lib/apt/lists/*

# Copy built output and set correct permissions
COPY --from=build /app/build .
RUN chown -R app:app /app

# Switch to the non-root user
USER app

# Expose port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
