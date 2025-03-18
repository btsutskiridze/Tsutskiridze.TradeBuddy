# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Tsutskiridze.TradeBuddy.sln ./
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/

# Restore dependencies
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj

# Copy remaining source code
COPY Tsutskiridze.TradeBuddy/ Tsutskiridze.TradeBuddy/

WORKDIR /src/Tsutskiridze.TradeBuddy

# Build and publish the application
RUN dotnet publish -c Release -o /app/build
RUN dotnet tool restore

# Install prerequisites for Playwright
RUN apt-get update && apt-get install -y wget unzip && rm -rf /var/lib/apt/lists/*

# Install the Playwright CLI and download Chromium
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN playwright install chromium

# Copy Playwright's browser binaries to a folder that we can later use in runtime
RUN cp -R /root/.cache/ms-playwright /app/ms-playwright

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user and group
RUN getent group app || groupadd -g 1001 app && \
    getent passwd app || useradd -m -u 1001 -g app app

# Install OS-level Chromium dependencies (including wget and unzip for Playwright)
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 libxcomposite1 \
    libxdamage1 libxrandr2 libcups2 libgbm1 libasound2 libxfixes3 libxkbcommon0 \
    libpangocairo-1.0-0 libpango-1.0-0 libxshmfence1 wget unzip && \
    rm -rf /var/lib/apt/lists/* \
    
# Copy the built application and the Playwright browser binaries from the build stage
COPY --from=build /app/build .
COPY --from=build /app/ms-playwright /app/ms-playwright

# Adjust permissions so the non-root user can access everything
RUN chown -R app:app /app

# Set environment variable to point to the downloaded browsers
ENV PLAYWRIGHT_BROWSERS_PATH=/app/ms-playwright

# Switch to the non-root user
USER app

# Expose port and set entrypoint
EXPOSE 80
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
