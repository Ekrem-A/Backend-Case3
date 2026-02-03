# Kubernetes Deployment Script for Windows - Local Development
# Docker Desktop Kubernetes için optimize edilmiþ

param(
    [Parameter(Position=0)]
    [ValidateSet("build", "base", "infra", "services", "policies", "ingress", "status", "all", "local", "clean")]
    [string]$Action = "local",
    
    [string]$Registry = "",  # Boþ býrakýlýrsa local image kullanýlýr
    [string]$Version = "latest"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Backend Services - K8s Deployment" -ForegroundColor Cyan
Write-Host "  (Docker Desktop Local Development)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$Namespace = "backend-services"

function Print-Status($message) {
    Write-Host "[OK] $message" -ForegroundColor Green
}

function Print-Warning($message) {
    Write-Host "[!] $message" -ForegroundColor Yellow
}

function Print-Error($message) {
    Write-Host "[X] $message" -ForegroundColor Red
}

# Kubernetes baðlantýsýný kontrol et
function Test-KubernetesConnection {
    Write-Host "Checking Kubernetes connection..."
    try {
        $context = kubectl config current-context 2>&1
        if ($context -ne "docker-desktop") {
            Print-Warning "Current context: $context"
            Print-Warning "Switching to docker-desktop context..."
            kubectl config use-context docker-desktop
        }
        kubectl cluster-info | Out-Null
        Print-Status "Connected to Docker Desktop Kubernetes"
    }
    catch {
        Print-Error "Cannot connect to Kubernetes cluster!"
        Write-Host ""
        Write-Host "Please enable Kubernetes in Docker Desktop:" -ForegroundColor Yellow
        Write-Host "  1. Open Docker Desktop"
        Write-Host "  2. Go to Settings > Kubernetes"
        Write-Host "  3. Check 'Enable Kubernetes'"
        Write-Host "  4. Click 'Apply & Restart'"
        exit 1
    }
}

# Docker image'larý sadece local olarak build et (push yok)
function Build-LocalImages {
    Write-Host ""
    Write-Host "Building Docker images locally..."
    
    # Gateway API
    Write-Host "Building gateway-api..."
    docker build -t backend-gateway-api:$Version -f gateway/Gateway.Api/Dockerfile .
    Print-Status "gateway-api built"
    
    # Identity API
    Write-Host "Building identity-api..."
    docker build -t backend-identity-api:$Version -f services/Identity/Identity.Api/Dockerfile .
    Print-Status "identity-api built"
    
    # Products API
    Write-Host "Building products-api..."
    docker build -t backend-products-api:$Version -f services/Products/Products.Api/Dockerfile .
    Print-Status "products-api built"
    
    Write-Host ""
    Print-Status "All images built locally"
    docker images | Select-String "backend-"
}

# Namespace ve temel kaynaklarý oluþtur
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

# Infrastructure bileþenlerini deploy et
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
    
    Write-Host ""
    Write-Host "Waiting for infrastructure to be ready..." -ForegroundColor Yellow
    Write-Host "(This may take 1-2 minutes)"
    
    # SQL Server'ýn hazýr olmasýný bekle
    $timeout = 120
    $elapsed = 0
    while ($elapsed -lt $timeout) {
        $sqlPod = kubectl get pods -n $Namespace -l app=sqlserver -o jsonpath='{.items[0].status.phase}' 2>$null
        if ($sqlPod -eq "Running") {
            break
        }
        Start-Sleep -Seconds 5
        $elapsed += 5
        Write-Host "  Waiting for SQL Server... ($elapsed s)"
    }
    
    Print-Status "Infrastructure deployment initiated"
}

# Servisleri deploy et (local images için)
function Deploy-Services-Local {
    Write-Host ""
    Write-Host "Deploying application services (local images)..."
    
    # Local image isimleriyle geçici YAML oluþtur
    $tempDir = "k8s/temp"
    if (!(Test-Path $tempDir)) {
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    }
    
    # Gateway API
    $gatewayYaml = Get-Content k8s/services/gateway-api.yaml -Raw
    $gatewayYaml = $gatewayYaml -replace "image: .*gateway-api:.*", "image: backend-gateway-api:$Version"
    $gatewayYaml = $gatewayYaml -replace "imagePullPolicy: Always", "imagePullPolicy: Never"
    $gatewayYaml | Set-Content "$tempDir/gateway-api.yaml"
    
    # Identity API
    $identityYaml = Get-Content k8s/services/identity-api.yaml -Raw
    $identityYaml = $identityYaml -replace "image: .*identity-api:.*", "image: backend-identity-api:$Version"
    $identityYaml = $identityYaml -replace "imagePullPolicy: Always", "imagePullPolicy: Never"
    $identityYaml | Set-Content "$tempDir/identity-api.yaml"
    
    # Products API
    $productsYaml = Get-Content k8s/services/products-api.yaml -Raw
    $productsYaml = $productsYaml -replace "image: .*products-api:.*", "image: backend-products-api:$Version"
    $productsYaml = $productsYaml -replace "imagePullPolicy: Always", "imagePullPolicy: Never"
    $productsYaml | Set-Content "$tempDir/products-api.yaml"
    
    kubectl apply -f "$tempDir/identity-api.yaml"
    Print-Status "Identity API deployed"
    
    kubectl apply -f "$tempDir/products-api.yaml"
    Print-Status "Products API deployed"
    
    kubectl apply -f "$tempDir/gateway-api.yaml"
    Print-Status "Gateway API deployed"
    
    # Temp dosyalarý temizle
    Remove-Item -Path $tempDir -Recurse -Force
    
    Print-Status "Services deployment initiated"
}

# Network policies ve PDB'leri deploy et
function Deploy-Policies {
    Write-Host ""
    Write-Host "Deploying policies..."
    
    # Local development için network policies'i atla (opsiyonel)
    # kubectl apply -f k8s/network-policies.yaml
    # Print-Status "Network policies applied"
    
    kubectl apply -f k8s/pod-disruption-budgets.yaml
    Print-Status "Pod Disruption Budgets applied"
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
    Write-Host "HPAs:" -ForegroundColor Yellow
    kubectl get hpa -n $Namespace 2>$null
}

# Port forwarding ile local eriþim saðla
function Start-PortForwarding {
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "  Local Access URLs" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Starting port forwarding..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Run these commands in separate terminals:" -ForegroundColor Green
    Write-Host ""
    Write-Host "  # Gateway API (Main Entry Point)" -ForegroundColor Cyan
    Write-Host "  kubectl port-forward svc/gateway-api-service 5100:80 -n backend-services"
    Write-Host ""
    Write-Host "  # Seq Log Server" -ForegroundColor Cyan
    Write-Host "  kubectl port-forward svc/seq-service 5341:80 -n backend-services"
    Write-Host ""
    Write-Host "  # Redis Commander (optional)" -ForegroundColor Cyan
    Write-Host "  kubectl port-forward svc/redis-service 6379:6379 -n backend-services"
    Write-Host ""
    Write-Host "Then access:" -ForegroundColor Green
    Write-Host "  - API Gateway: http://localhost:5100" -ForegroundColor White
    Write-Host "  - Auth:        http://localhost:5100/api/auth/login" -ForegroundColor White
    Write-Host "  - Products:    http://localhost:5100/api/products" -ForegroundColor White
    Write-Host "  - Seq Logs:    http://localhost:5341" -ForegroundColor White
}

# Tüm kaynaklarý temizle
function Clean-All {
    Write-Host ""
    Write-Host "Cleaning up all resources..." -ForegroundColor Yellow
    
    kubectl delete namespace $Namespace --ignore-not-found=true
    Print-Status "Namespace and all resources deleted"
    
    # Local temp dosyalarýný temizle
    if (Test-Path "k8s/temp") {
        Remove-Item -Path "k8s/temp" -Recurse -Force
    }
}

# Ana akýþ
Test-KubernetesConnection

switch ($Action) {
    "build" {
        Build-LocalImages
    }
    "base" {
        Deploy-Base
    }
    "infra" {
        Deploy-Infrastructure
    }
    "services" {
        Deploy-Services-Local
    }
    "policies" {
        Deploy-Policies
    }
    "status" {
        Show-Status
        Start-PortForwarding
    }
    "clean" {
        Clean-All
    }
    "local" {
        # Local development için tam akýþ
        Build-LocalImages
        Deploy-Base
        Deploy-Infrastructure
        
        Write-Host ""
        Write-Host "Waiting 60 seconds for infrastructure..." -ForegroundColor Yellow
        Start-Sleep -Seconds 60
        
        Deploy-Services-Local
        Deploy-Policies
        
        Write-Host ""
        Write-Host "Waiting 30 seconds for services to start..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        Show-Status
        Start-PortForwarding
    }
    "all" {
        # Registry ile tam akýþ (production)
        if ([string]::IsNullOrEmpty($Registry)) {
            Print-Error "Registry parameter required for 'all' action"
            Write-Host "Use: .\k8s-deploy-local.ps1 -Action all -Registry 'your-registry.azurecr.io'"
            exit 1
        }
        Build-AndPushImages
        Deploy-Base
        Deploy-Infrastructure
        Start-Sleep -Seconds 60
        Deploy-Services
        Deploy-Policies
        Show-Status
    }
}

Write-Host ""
Print-Status "Deployment completed!"
