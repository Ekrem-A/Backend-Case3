# 12-Factor App: Docker Multi-Stage Build
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Products.Api/Products.Api.csproj", "Products.Api/"]
COPY ["Products.Application/Products.Application.csproj", "Products.Application/"]
COPY ["Products.Domain/Products.Domain.csproj", "Products.Domain/"]
COPY ["Products.Infrastructure/Products.Infrastructure.csproj", "Products.Infrastructure/"]

RUN dotnet restore "Products.Api/Products.Api.csproj"

# Copy source code and build
COPY Products.Api/ Products.Api/
COPY Products.Application/ Products.Application/
COPY Products.Domain/ Products.Domain/
COPY Products.Infrastructure/ Products.Infrastructure/

WORKDIR /src/Products.Api
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1000 appuser

# Copy published files
COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health/live || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Products.Api.dll"]

