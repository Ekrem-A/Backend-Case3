#!/bin/bash

# Kubernetes Deployment Script
# Backend Services - Production Deployment

set -e

echo "=========================================="
echo "  Backend Services - K8s Deployment"
echo "=========================================="
echo ""

# Değişkenler
NAMESPACE="backend-services"
REGISTRY="your-registry.azurecr.io"  # Azure Container Registry veya Docker Hub
VERSION="${VERSION:-latest}"

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Kubernetes bağlantısını kontrol et
echo "Checking Kubernetes connection..."
if ! kubectl cluster-info &> /dev/null; then
    print_error "Cannot connect to Kubernetes cluster!"
    exit 1
fi
print_status "Connected to Kubernetes cluster"

# Docker image'ları build et ve push et
build_and_push_images() {
    echo ""
    echo "Building and pushing Docker images..."
    
    # Gateway API
    echo "Building gateway-api..."
    docker build -t $REGISTRY/gateway-api:$VERSION -f gateway/Gateway.Api/Dockerfile .
    docker push $REGISTRY/gateway-api:$VERSION
    print_status "gateway-api pushed"
    
    # Identity API
    echo "Building identity-api..."
    docker build -t $REGISTRY/identity-api:$VERSION -f services/Identity/Identity.Api/Dockerfile .
    docker push $REGISTRY/identity-api:$VERSION
    print_status "identity-api pushed"
    
    # Products API
    echo "Building products-api..."
    docker build -t $REGISTRY/products-api:$VERSION -f services/Products/Products.Api/Dockerfile .
    docker push $REGISTRY/products-api:$VERSION
    print_status "products-api pushed"
}

# Kubernetes manifest'lerini güncelle (image tag'leri)
update_manifests() {
    echo ""
    echo "Updating image tags in manifests..."
    
    # sed ile image tag'lerini güncelle
    sed -i "s|image: backend-case3-gateway-api:.*|image: $REGISTRY/gateway-api:$VERSION|g" k8s/services/gateway-api.yaml
    sed -i "s|image: backend-case3-identity-api:.*|image: $REGISTRY/identity-api:$VERSION|g" k8s/services/identity-api.yaml
    sed -i "s|image: backend-case3-products-api:.*|image: $REGISTRY/products-api:$VERSION|g" k8s/services/products-api.yaml
    
    print_status "Manifests updated with version: $VERSION"
}

# Namespace ve temel kaynakları oluştur
deploy_base() {
    echo ""
    echo "Deploying base resources..."
    
    kubectl apply -f k8s/namespace.yaml
    print_status "Namespace created"
    
    kubectl apply -f k8s/configmap.yaml
    print_status "ConfigMap created"
    
    kubectl apply -f k8s/secrets.yaml
    print_status "Secrets created"
}

# Infrastructure bileşenlerini deploy et
deploy_infrastructure() {
    echo ""
    echo "Deploying infrastructure components..."
    
    kubectl apply -f k8s/infrastructure/sqlserver.yaml
    print_status "SQL Server deployed"
    
    kubectl apply -f k8s/infrastructure/redis.yaml
    print_status "Redis deployed"
    
    kubectl apply -f k8s/infrastructure/kafka.yaml
    print_status "Kafka deployed"
    
    kubectl apply -f k8s/infrastructure/seq.yaml
    print_status "Seq deployed"
    
    # Infrastructure'ın hazır olmasını bekle
    echo "Waiting for infrastructure to be ready..."
    kubectl wait --for=condition=ready pod -l app=sqlserver -n $NAMESPACE --timeout=120s || true
    kubectl wait --for=condition=ready pod -l app=redis -n $NAMESPACE --timeout=60s || true
    kubectl wait --for=condition=ready pod -l app=kafka -n $NAMESPACE --timeout=90s || true
    
    print_status "Infrastructure is ready"
}

# Servisleri deploy et
deploy_services() {
    echo ""
    echo "Deploying application services..."
    
    kubectl apply -f k8s/services/identity-api.yaml
    print_status "Identity API deployed"
    
    kubectl apply -f k8s/services/products-api.yaml
    print_status "Products API deployed"
    
    kubectl apply -f k8s/services/gateway-api.yaml
    print_status "Gateway API deployed"
    
    # Servislerin hazır olmasını bekle
    echo "Waiting for services to be ready..."
    kubectl wait --for=condition=ready pod -l app=identity-api -n $NAMESPACE --timeout=120s || true
    kubectl wait --for=condition=ready pod -l app=products-api -n $NAMESPACE --timeout=120s || true
    kubectl wait --for=condition=ready pod -l app=gateway-api -n $NAMESPACE --timeout=120s || true
    
    print_status "Services are ready"
}

# Network policies ve PDB'leri deploy et
deploy_policies() {
    echo ""
    echo "Deploying policies..."
    
    kubectl apply -f k8s/network-policies.yaml
    print_status "Network policies applied"
    
    kubectl apply -f k8s/pod-disruption-budgets.yaml
    print_status "Pod Disruption Budgets applied"
}

# Ingress'i deploy et
deploy_ingress() {
    echo ""
    echo "Deploying ingress..."
    
    kubectl apply -f k8s/ingress.yaml
    print_status "Ingress deployed"
}

# Deployment durumunu göster
show_status() {
    echo ""
    echo "=========================================="
    echo "  Deployment Status"
    echo "=========================================="
    
    echo ""
    echo "Pods:"
    kubectl get pods -n $NAMESPACE -o wide
    
    echo ""
    echo "Services:"
    kubectl get services -n $NAMESPACE
    
    echo ""
    echo "Ingress:"
    kubectl get ingress -n $NAMESPACE
    
    echo ""
    echo "HPAs:"
    kubectl get hpa -n $NAMESPACE
}

# Ana deployment akışı
main() {
    case "${1:-all}" in
        build)
            build_and_push_images
            ;;
        base)
            deploy_base
            ;;
        infra)
            deploy_infrastructure
            ;;
        services)
            deploy_services
            ;;
        policies)
            deploy_policies
            ;;
        ingress)
            deploy_ingress
            ;;
        status)
            show_status
            ;;
        all)
            build_and_push_images
            update_manifests
            deploy_base
            deploy_infrastructure
            deploy_services
            deploy_policies
            deploy_ingress
            show_status
            ;;
        *)
            echo "Usage: $0 {build|base|infra|services|policies|ingress|status|all}"
            exit 1
            ;;
    esac
}

main "$@"

echo ""
print_status "Deployment completed!"
echo ""
echo "Access your API at: https://api.eko.com"
echo "Seq Logs at: https://logs.eko.com"
