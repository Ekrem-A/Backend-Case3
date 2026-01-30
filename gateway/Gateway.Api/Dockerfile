# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Backend-Case3.sln .
COPY gateway/Gateway.Api/Gateway.Api.csproj gateway/Gateway.Api/

# Restore dependencies (Bağımlılıklar)
RUN dotnet restore gateway/Gateway.Api/Gateway.Api.csproj

# Copy source code
COPY gateway/Gateway.Api/ gateway/Gateway.Api/

# Build
WORKDIR /src/gateway/Gateway.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage (12-Factor: Stateless Prosesler)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published files
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Port Bağımsızlığı - Port environment variable ile belirlenir
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Logs - stdout'a yaz
ENV Logging__Console__FormatterName=Json

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

