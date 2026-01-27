# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Backend-Case3.sln .
COPY Gateway.Api/Gateway.Api.csproj Gateway.Api/

# Restore dependencies (12-Factor: Bağımlılıklar)
RUN dotnet restore Gateway.Api/Gateway.Api.csproj

# Copy source code
COPY Gateway.Api/ Gateway.Api/

# Build
WORKDIR /src/Gateway.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage (12-Factor: Stateless Prosesler)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published files
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# 12-Factor: Port Bağımsızlığı - Port environment variable ile belirlenir
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# 12-Factor: Logs - stdout'a yaz
ENV Logging__Console__FormatterName=Json

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

