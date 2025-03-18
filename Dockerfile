# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Set PATH so that dotnet global tools (like Playwright CLI) are available
ENV PATH="/root/.dotnet/tools:${PATH}"

# Copy only the solution and project files first to leverage Docker caching for dependency restoration
COPY Tsutskiridze.TradeBuddy.sln . 
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/

# Restore dependencies – this layer will be cached as long as your project files remain unchanged
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj

# Now copy the remaining source code (this step invalidates the cache for later layers if code changes)
COPY Tsutskiridze.TradeBuddy/ Tsutskiridze.TradeBuddy/

WORKDIR /src/Tsutskiridze.TradeBuddy

# Build and publish the application
RUN dotnet publish -c Release -o /app/build

# Install prerequisites for Playwright CLI
RUN apt-get update && apt-get install -y wget unzip && rm -rf /var/lib/apt/lists/*

# Restore and install the Playwright CLI, download Chromium, and cache browser binaries
RUN dotnet tool restore && \
    dotnet tool install --global Microsoft.Playwright.CLI && \
    playwright install chromium && \
    cp -R /root/.cache/ms-playwright /app/ms-playwright

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user and group safely: only add them if they don't already exist.
RUN if ! getent group app > /dev/null; then groupadd -g 1001 app; fi && \
    if ! id -u app > /dev/null 2>&1; then useradd -m -u 1001 -g app app; fi

# Install OS-level dependencies required by Chromium and Playwright
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 libxcomposite1 \
    libxdamage1 libxrandr2 libcups2 libgbm1 libasound2 \
    libpangocairo-1.0-0 libpango-1.0-0 libxshmfence1 \
    libxfixes3 libxkbcommon0 wget unzip && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application and cached Playwright browser binaries from the build stage
COPY --from=build /app/build .
COPY --from=build /app/ms-playwright /app/ms-playwright

# Adjust permissions so the non-root user can access everything
RUN chown -R app:app /app

# Set the environment variable to point to the downloaded browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/app/ms-playwright

# Switch to the non-root user
USER app

# Expose the desired port and set the application entrypoint
EXPOSE 80
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
