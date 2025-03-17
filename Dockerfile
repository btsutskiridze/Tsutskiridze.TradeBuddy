# Stage 1: Build the application and install Playwright's Chromium
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files to leverage caching.
COPY *.sln ./
COPY Tsutskiridze.TradeBuddy/*.csproj Tsutskiridze.TradeBuddy/

# Restore dependencies using the project file.
RUN dotnet restore Tsutskiridze.TradeBuddy/Tsutskiridze.TradeBuddy.csproj

# Copy the remaining source code.
COPY . .

# Switch to the project directory.
WORKDIR /src/Tsutskiridze.TradeBuddy

# Set the environment variable so that Playwright downloads browsers into the publish folder.
ENV PLAYWRIGHT_BROWSERS_PATH=0

# Publish the project in Release configuration (this builds the project as well).
RUN dotnet publish -c Release -o /app/publish

# Restore NuGet packages and local tools.
RUN dotnet restore
RUN dotnet tool restore

# Install only Chromium via Playwright.
RUN dotnet playwright install chromium

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install system dependencies needed for Chromium (Playwright)
RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libcups2 \
    libgbm1 \
    libasound2 \
    libpangocairo-1.0-0 \
    libpango-1.0-0 \
    libxshmfence1 && \
    rm -rf /var/lib/apt/lists/*

# Copy the published output from the build stage.
COPY --from=build /app/publish .

# Expose the port your API listens on (adjust if needed).
EXPOSE 80

# Start the application.
ENTRYPOINT ["dotnet", "Tsutskiridze.TradeBuddy.dll"]
