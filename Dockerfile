# Stage 1: Install Playwright Browsers (cached independently)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS playwright
WORKDIR /tmp
RUN apt-get update && apt-get install -y wget unzip && rm -rf /var/lib/apt/lists/*
# Set PATH for dotnet global tools
ENV PATH="/root/.dotnet/tools:${PATH}"
# Install the Playwright CLI and download Chromium
RUN dotnet tool install --global Microsoft.Playwright.CLI && \
    playwright install chromium
# Copy the installed Playwright browser binaries to a persistent folder
RUN cp -R /root/.cache/ms-playwright /playwright_binaries

# Stage 2: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy solution and project files to leverage caching for dependencies
COPY Tsutskiridze.TradeBuddy.sln . 
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/
# Restore NuGet packages
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj
# Copy the remaining source code
COPY Tsutskiridze.TradeBuddy/ Tsutskiridze.TradeBuddy/
WORKDIR /src/Tsutskiridze.TradeBuddy
# Publish the application
RUN dotnet publish -c Release -o /app/build

# Stage 3: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Safely create a non-root user and group if they don't exist
RUN if ! getent group app > /dev/null; then groupadd -g 1001 app; fi && \
    if ! id -u app > /dev/null 2>&1; then useradd -m -u 1001 -g app app; fi

# Install OS-level dependencies required by Chromium/Playwright
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 libxcomposite1 \
    libxdamage1 libxrandr2 libcups2 libgbm1 libasound2 \
    libpangocairo-1.0-0 libpango-1.0-0 libxshmfence1 \
    libxfixes3 libxkbcommon0 wget unzip && \
    rm -rf /var/lib/apt/lists/*

# Copy the published application from the build stage
COPY --from=build /app/build .
# Copy the cached Playwright binaries from the playwright stage
COPY --from=playwright /playwright_binaries /app/ms-playwright

# Adjust permissions so the non-root user can access everything
RUN chown -R app:app /app

# Set the environment variable for Playwright to find the browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/app/ms-playwright

# Switch to the non-root user for runtime
USER app

# Expose port 80 and set the entrypoint
EXPOSE 80
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
