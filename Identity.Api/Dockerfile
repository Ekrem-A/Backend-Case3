# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Backend-Case3.sln .
COPY Identity.Api/Identity.Api.csproj Identity.Api/
COPY Identity.Application/Identity.Application.csproj Identity.Application/
COPY Identity.Domain/Identity.Domain.csproj Identity.Domain/
COPY Identity.Infrastructure/Identity.Infrastructure.csproj Identity.Infrastructure/

# Restore dependencies (12-Factor: Bağımlılıklar)
RUN dotnet restore Identity.Api/Identity.Api.csproj

# Copy source code
COPY Identity.Api/ Identity.Api/
COPY Identity.Application/ Identity.Application/
COPY Identity.Domain/ Identity.Domain/
COPY Identity.Infrastructure/ Identity.Infrastructure/

# Build
WORKDIR /src/Identity.Api
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

ENTRYPOINT ["dotnet", "Identity.Api.dll"]

