# --------------------------------------------
# Stage 1: Build
# --------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Set PATH so that dotnet global tools (like Playwright CLI) are available
ENV PATH="/root/.dotnet/tools:${PATH}"
ENV PLAYWRIGHT_BROWSERS_PATH=/app/ms-playwright

# 1. Install build-stage dependencies BEFORE copying project files
#    so this layer is re-used as long as apt packages don't change.
RUN apt-get update && apt-get install -y wget unzip \
    && rm -rf /var/lib/apt/lists/*

# 2. Copy project files for NuGet restore.
#    (If you have .sln or multiple csproj, copy those too.)
COPY ["Tsutskiridze.TradeBuddy.API/Tsutskiridze.TradeBuddy.API.csproj", "Tsutskiridze.TradeBuddy.API/"]
COPY ["Tsutskiridze.TradeBuddy.Application/Tsutskiridze.TradeBuddy.Application.csproj", "Tsutskiridze.TradeBuddy.Application/"]
COPY ["Tsutskiridze.TradeBuddy.Infrastructure/Tsutskiridze.TradeBuddy.Infrastructure.csproj", "Tsutskiridze.TradeBuddy.Infrastructure/"]
COPY ["Tsutskiridze.TradeBuddy.Core/Tsutskiridze.TradeBuddy.Core.csproj", "Tsutskiridze.TradeBuddy.Core/"]

# 3. Restore NuGet packages. If csproj is unchanged, this layer remains cached.
RUN dotnet restore "Tsutskiridze.TradeBuddy.API/Tsutskiridze.TradeBuddy.API.csproj"

# 4. Install Playwright globally
RUN dotnet tool install --global Microsoft.Playwright.CLI

# 5. Copy the rest of your source code
COPY . .

# 6. Build your project
RUN dotnet build "Tsutskiridze.TradeBuddy.API/Tsutskiridze.TradeBuddy.API.csproj" --no-restore -c Release

# 7. Now that the app is built, install Chromium using the global Playwright CLI
RUN playwright install chromium \
 && rm -rf /root/.cache/ms-playwright/chromium-*/chrome-linux/locales

# 8. Publish your application
RUN dotnet publish "Tsutskiridze.TradeBuddy.API/Tsutskiridze.TradeBuddy.API.csproj" -c Release -o /app/build

# --------------------------------------------
# Stage 2: Runtime
# --------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user and group if it doesn't exist
RUN if ! getent group app > /dev/null; then groupadd -g 1001 app; fi && \
    if ! id -u app > /dev/null 2>&1; then useradd -m -u 1001 -g app app; fi

# Install OS-level dependencies required by Chromium and Playwright at runtime
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 libxcomposite1 \
    libxdamage1 libxrandr2 libcups2 libgbm1 libasound2t64 \
    libpangocairo-1.0-0 libpango-1.0-0 libxshmfence1 \
    libxfixes3 libxkbcommon0 wget unzip && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build --chown=app:app /app/build .
COPY --from=build --chown=app:app /app/ms-playwright /app/ms-playwright

# Let Playwright know where the browsers live
ENV PLAYWRIGHT_BROWSERS_PATH=/app/ms-playwright

# Switch to non-root user
USER app

# Expose the desired port and set the application entrypoint
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.API.dll"]