# 12-Factor App: Docker Multi-Stage Build
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["services/Products/Products.Api/Products.Api.csproj", "services/Products/Products.Api/"]
COPY ["services/Products/Products.Application/Products.Application.csproj", "services/Products/Products.Application/"]
COPY ["services/Products/Products.Domain/Products.Domain.csproj", "services/Products/Products.Domain/"]
COPY ["services/Products/Products.Infrastructure/Products.Infrastructure.csproj", "services/Products/Products.Infrastructure/"]

RUN dotnet restore "services/Products/Products.Api/Products.Api.csproj"

# Copy source code and build
COPY services/Products/Products.Api/ services/Products/Products.Api/
COPY services/Products/Products.Application/ services/Products/Products.Application/
COPY services/Products/Products.Domain/ services/Products/Products.Domain/
COPY services/Products/Products.Infrastructure/ services/Products/Products.Infrastructure/

WORKDIR /src/services/Products/Products.Api
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

