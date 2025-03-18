########################################
# Stage 1: Build and Publish the App
########################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Ensure dotnet global tools are available
ENV PATH="/root/.dotnet/tools:${PATH}"

# Copy the solution and project files first for caching dependency restoration
COPY Tsutskiridze.TradeBuddy.sln .
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/

# Restore dependencies
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj

# Copy the rest of the source code and publish the application
COPY Tsutskiridze.TradeBuddy/ Tsutskiridze.TradeBuddy/
WORKDIR /src/Tsutskiridze.TradeBuddy
RUN dotnet publish -c Release -o /app/build

# Install prerequisites for Playwright CLI
RUN apt-get update && apt-get install -y wget unzip && rm -rf /var/lib/apt/lists/*

# Install Playwright CLI and download Chromium, then cache the browser binaries
RUN dotnet tool restore && \
    dotnet tool install --global Microsoft.Playwright.CLI && \
    playwright install chromium && \
    cp -R /root/.cache/ms-playwright /app/ms-playwright

########################################
# Stage 2: Final Runtime Image
########################################
# Use the official Playwright image (which comes with all necessary browsers and OS libraries)
FROM mcr.microsoft.com/playwright:jammy AS runtime

# Install .NET 8.0 Runtime on top of the Playwright image
RUN apt-get update && apt-get install -y wget apt-transport-https && rm -rf /var/lib/apt/lists/*
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y dotnet-runtime-8.0

WORKDIR /app

# Copy the published app and cached Playwright browser binaries from the build stage
COPY --from=build /app/build .
COPY --from=build /app/ms-playwright /app/ms-playwright

# Create a non-root user for security
RUN if ! getent group app > /dev/null; then groupadd -g 1001 app; fi && \
    if ! id -u app > /dev/null 2>&1; then useradd -m -u 1001 -g app app; fi

# Adjust ownership so that the non-root user can access the application files
RUN chown -R app:app /app

# Set the environment variable so that Playwright knows where to find the browser binaries
ENV PLAYWRIGHT_BROWSERS_PATH=/app/ms-playwright

# Expose the port your application listens on
EXPOSE 80

# Switch to the non-root user for added security
USER app

# Set the entrypoint to run your app
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
