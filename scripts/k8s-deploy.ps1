# Kubernetes Deployment Script for Windows
# Backend Services - Production Deployment

param(
    [Parameter(Position=0)]
    [ValidateSet("build", "base", "infra", "services", "policies", "ingress", "status", "all")]
    [string]$Action = "all",
    
    [string]$Registry = "your-registry.azurecr.io",
    [string]$Version = "latest"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Backend Services - K8s Deployment" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$Namespace = "backend-services"

function Print-Status($message) {
    Write-Host "[✓] $message" -ForegroundColor Green
}

function Print-Warning($message) {
    Write-Host "[!] $message" -ForegroundColor Yellow
}

function Print-Error($message) {
    Write-Host "[✗] $message" -ForegroundColor Red
}

# Kubernetes bağlantısını kontrol et
function Test-KubernetesConnection {
    Write-Host "Checking Kubernetes connection..."
    try {
        kubectl cluster-info | Out-Null
        Print-Status "Connected to Kubernetes cluster"
    }
    catch {
        Print-Error "Cannot connect to Kubernetes cluster!"
        exit 1
    }
}

# Docker image'ları build et ve push et
function Build-AndPushImages {
    Write-Host ""
    Write-Host "Building and pushing Docker images..."
    
    # Gateway API
    Write-Host "Building gateway-api..."
    docker build -t "$Registry/gateway-api:$Version" -f gateway/Gateway.Api/Dockerfile .
    docker push "$Registry/gateway-api:$Version"
    Print-Status "gateway-api pushed"
    
    # Identity API
    Write-Host "Building identity-api..."
    docker build -t "$Registry/identity-api:$Version" -f services/Identity/Identity.Api/Dockerfile .
    docker push "$Registry/identity-api:$Version"
    Print-Status "identity-api pushed"
    
    # Products API
    Write-Host "Building products-api..."
    docker build -t "$Registry/products-api:$Version" -f services/Products/Products.Api/Dockerfile .
    docker push "$Registry/products-api:$Version"
    Print-Status "products-api pushed"
}

# Namespace ve temel kaynakları oluştur
function Deploy-Base {
    Write-Host ""
    Write-Host "Deploying base resources..."
    
    kubectl apply -f k8s/namespace.yaml
    Print-Status "Namespace created"
    
    kubectl apply -f k8s/configmap.yaml
    Print-Status "ConfigMap created"
    
    kubectl apply -f k8s/secrets.yaml
    Print-Status "Secrets created"
}

# Infrastructure bileşenlerini deploy et
function Deploy-Infrastructure {
    Write-Host ""
    Write-Host "Deploying infrastructure components..."
    
    kubectl apply -f k8s/infrastructure/sqlserver.yaml
    Print-Status "SQL Server deployed"
    
    kubectl apply -f k8s/infrastructure/redis.yaml
    Print-Status "Redis deployed"
    
    kubectl apply -f k8s/infrastructure/kafka.yaml
    Print-Status "Kafka deployed"
    
    kubectl apply -f k8s/infrastructure/seq.yaml
    Print-Status "Seq deployed"
    
    Write-Host "Waiting for infrastructure to be ready..."
    Start-Sleep -Seconds 30
    Print-Status "Infrastructure deployment initiated"
}

# Servisleri deploy et
function Deploy-Services {
    Write-Host ""
    Write-Host "Deploying application services..."
    
    # Image tag'lerini güncelle
    $gatewayYaml = Get-Content k8s/services/gateway-api.yaml -Raw
    $gatewayYaml = $gatewayYaml -replace "image: backend-case3-gateway-api:.*", "image: $Registry/gateway-api:$Version"
    $gatewayYaml | Set-Content k8s/services/gateway-api.yaml
    
    $identityYaml = Get-Content k8s/services/identity-api.yaml -Raw
    $identityYaml = $identityYaml -replace "image: backend-case3-identity-api:.*", "image: $Registry/identity-api:$Version"
    $identityYaml | Set-Content k8s/services/identity-api.yaml
    
    $productsYaml = Get-Content k8s/services/products-api.yaml -Raw
    $productsYaml = $productsYaml -replace "image: backend-case3-products-api:.*", "image: $Registry/products-api:$Version"
    $productsYaml | Set-Content k8s/services/products-api.yaml
    
    kubectl apply -f k8s/services/identity-api.yaml
    Print-Status "Identity API deployed"
    
    kubectl apply -f k8s/services/products-api.yaml
    Print-Status "Products API deployed"
    
    kubectl apply -f k8s/services/gateway-api.yaml
    Print-Status "Gateway API deployed"
    
    Print-Status "Services deployment initiated"
}

# Network policies ve PDB'leri deploy et
function Deploy-Policies {
    Write-Host ""
    Write-Host "Deploying policies..."
    
    kubectl apply -f k8s/network-policies.yaml
    Print-Status "Network policies applied"
    
    kubectl apply -f k8s/pod-disruption-budgets.yaml
    Print-Status "Pod Disruption Budgets applied"
}

# Ingress'i deploy et
function Deploy-Ingress {
    Write-Host ""
    Write-Host "Deploying ingress..."
    
    kubectl apply -f k8s/ingress.yaml
    Print-Status "Ingress deployed"
}

# Deployment durumunu göster
function Show-Status {
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "  Deployment Status" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    
    Write-Host ""
    Write-Host "Pods:" -ForegroundColor Yellow
    kubectl get pods -n $Namespace -o wide
    
    Write-Host ""
    Write-Host "Services:" -ForegroundColor Yellow
    kubectl get services -n $Namespace
    
    Write-Host ""
    Write-Host "Ingress:" -ForegroundColor Yellow
    kubectl get ingress -n $Namespace
    
    Write-Host ""
    Write-Host "HPAs:" -ForegroundColor Yellow
    kubectl get hpa -n $Namespace
}

# Ana akış
Test-KubernetesConnection

switch ($Action) {
    "build" {
        Build-AndPushImages
    }
    "base" {
        Deploy-Base
    }
    "infra" {
        Deploy-Infrastructure
    }
    "services" {
        Deploy-Services
    }
    "policies" {
        Deploy-Policies
    }
    "ingress" {
        Deploy-Ingress
    }
    "status" {
        Show-Status
    }
    "all" {
        Build-AndPushImages
        Deploy-Base
        Deploy-Infrastructure
        Start-Sleep -Seconds 60  # Infrastructure'ın hazır olmasını bekle
        Deploy-Services
        Deploy-Policies
        Deploy-Ingress
        Show-Status
    }
}

Write-Host ""
Print-Status "Deployment completed!"
Write-Host ""
Write-Host "Access your API at: https://api.eko.com" -ForegroundColor Cyan
Write-Host "Seq Logs at: https://logs.eko.com" -ForegroundColor Cyan
