# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Backend-Case3.sln .
COPY services/Identity/Identity.Api/Identity.Api.csproj services/Identity/Identity.Api/
COPY services/Identity/Identity.Application/Identity.Application.csproj services/Identity/Identity.Application/
COPY services/Identity/Identity.Domain/Identity.Domain.csproj services/Identity/Identity.Domain/
COPY services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj services/Identity/Identity.Infrastructure/

# Restore dependencies (12-Factor: Bağımlılıklar)
RUN dotnet restore services/Identity/Identity.Api/Identity.Api.csproj

# Copy source code
COPY services/Identity/Identity.Api/ services/Identity/Identity.Api/
COPY services/Identity/Identity.Application/ services/Identity/Identity.Application/
COPY services/Identity/Identity.Domain/ services/Identity/Identity.Domain/
COPY services/Identity/Identity.Infrastructure/ services/Identity/Identity.Infrastructure/

# Build
WORKDIR /src/services/Identity/Identity.Api
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

