# Kubernetes Deployment Guide

Bu döküman, Backend Services projesinin Kubernetes ortamýna deploy edilmesi için gerekli adýmlarý içerir.

## ?? K8s Dosya Yapýsý

```
k8s/
??? namespace.yaml              # Namespace tanýmý
??? configmap.yaml              # Konfigürasyon deðiþkenleri
??? secrets.yaml                # Hassas veriler (þifreler, anahtarlar)
??? ingress.yaml                # Dýþ dünyaya açýlým (TLS)
??? network-policies.yaml       # Pod'lar arasý trafik kurallarý
??? pod-disruption-budgets.yaml # High Availability için
??? infrastructure/
?   ??? sqlserver.yaml          # SQL Server StatefulSet
?   ??? redis.yaml              # Redis Deployment
?   ??? kafka.yaml              # Kafka + Zookeeper
?   ??? seq.yaml                # Seq Log Server
??? services/
    ??? gateway-api.yaml        # API Gateway Deployment + HPA
    ??? identity-api.yaml       # Identity API Deployment + HPA
    ??? products-api.yaml       # Products API Deployment + HPA
```

## ?? Deployment Adýmlarý

### Ön Gereksinimler

1. **Kubernetes Cluster** (AKS, EKS, GKE veya on-premise)
2. **kubectl** yapýlandýrýlmýþ
3. **Container Registry** (Azure ACR, Docker Hub, vb.)
4. **NGINX Ingress Controller** kurulu
5. **cert-manager** (TLS için, opsiyonel)

### 1. Container Registry Ayarlarý

```bash
# Azure Container Registry örneði
az acr login --name your-registry

# Docker Hub örneði
docker login
```

### 2. Image'larý Build ve Push Et

```bash
# Windows
.\scripts\k8s-deploy.ps1 -Action build -Registry "your-registry.azurecr.io" -Version "1.0.0"

# Linux/macOS
chmod +x scripts/k8s-deploy.sh
./scripts/k8s-deploy.sh build
```

### 3. Kubernetes'e Deploy Et

```bash
# Tüm bileþenleri deploy et
.\scripts\k8s-deploy.ps1 -Action all

# Veya adým adým:
.\scripts\k8s-deploy.ps1 -Action base      # Namespace, ConfigMap, Secrets
.\scripts\k8s-deploy.ps1 -Action infra     # SQL Server, Redis, Kafka, Seq
.\scripts\k8s-deploy.ps1 -Action services  # API'ler
.\scripts\k8s-deploy.ps1 -Action policies  # Network Policies, PDB
.\scripts\k8s-deploy.ps1 -Action ingress   # Ingress
```

### 4. Manuel Deployment (Alternatif)

```bash
# Namespace oluþtur
kubectl apply -f k8s/namespace.yaml

# ConfigMap ve Secrets
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml

# Infrastructure
kubectl apply -f k8s/infrastructure/

# Servislerin hazýr olmasýný bekle
kubectl wait --for=condition=ready pod -l app=sqlserver -n backend-services --timeout=120s

# API'leri deploy et
kubectl apply -f k8s/services/

# Ingress ve Policies
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/network-policies.yaml
kubectl apply -f k8s/pod-disruption-budgets.yaml
```

## ?? Mimari

```
                                    ???????????????????
                                    ?   Internet      ?
                                    ???????????????????
                                             ?
                                    ???????????????????
                                    ?  Load Balancer  ?
                                    ?  (Cloud/NGINX)  ?
                                    ???????????????????
                                             ?
???????????????????????????????????????????????????????????????????????????????????????????
? Kubernetes Cluster                         ?                                            ?
?                                   ???????????????????                                   ?
?                                   ? Ingress (NGINX) ?                                   ?
?                                   ?   TLS Termination?                                  ?
?                                   ???????????????????                                   ?
?                                            ?                                            ?
?                                   ???????????????????                                   ?
?                                   ?  Gateway API    ? ??? HPA (2-10 replicas)           ?
?                                   ?  (YARP Proxy)   ?                                   ?
?                                   ???????????????????                                   ?
?                          ?????????????????????????????????????                          ?
?                          ?                 ?                 ?                          ?
?                 ??????????????????? ??????????????? ?????????????????                   ?
?                 ?  Identity API   ? ?Products API ? ?    Redis      ?                   ?
?                 ?  (2-10 pods)    ? ? (2-10 pods) ? ?   (Cache)     ?                   ?
?                 ??????????????????? ??????????????? ?????????????????                   ?
?                          ?                 ?                                            ?
?                          ?         ?????????????????                                    ?
?                          ?         ?               ?                                    ?
?                 ????????????????????????   ???????????????                              ?
?                 ?     SQL Server       ?   ?   Kafka     ?                              ?
?                 ?   (StatefulSet)      ?   ? (Events)    ?                              ?
?                 ????????????????????????   ???????????????                              ?
?                                                                                         ?
?                                   ???????????????????                                   ?
?                                   ?  Seq (Logging)  ?                                   ?
?                                   ???????????????????                                   ?
???????????????????????????????????????????????????????????????????????????????????????????
```

## ?? Konfigürasyon

### Domain Ayarlarý

`k8s/ingress.yaml` dosyasýnda domain'inizi güncelleyin:

```yaml
spec:
  tls:
    - hosts:
        - api.eko.com  # ? Kendi domain'iniz
      secretName: backend-tls-secret
  rules:
    - host: api.eko.com  # ? Kendi domain'iniz
```

### Secrets Güncelleme

Production'da secrets'larý güvenli bir þekilde yönetin:

```bash
# Azure Key Vault kullanýmý
kubectl create secret generic backend-secrets \
  --from-literal=JWT_SECRET_KEY=$(az keyvault secret show --name jwt-secret --vault-name your-vault --query value -o tsv) \
  -n backend-services
```

### Resource Limits

`k8s/services/*.yaml` dosyalarýnda ihtiyaca göre ayarlayýn:

```yaml
resources:
  requests:
    memory: "256Mi"   # Minimum bellek
    cpu: "100m"       # Minimum CPU
  limits:
    memory: "512Mi"   # Maksimum bellek
    cpu: "500m"       # Maksimum CPU
```

## ?? Monitoring & Debugging

### Pod Durumunu Kontrol Et

```bash
# Tüm pod'larý listele
kubectl get pods -n backend-services

# Pod loglarýný görüntüle
kubectl logs -f deployment/gateway-api -n backend-services

# Pod'a baðlan
kubectl exec -it <pod-name> -n backend-services -- /bin/sh
```

### Servis Durumu

```bash
# Servisleri listele
kubectl get svc -n backend-services

# Endpoint'leri kontrol et
kubectl get endpoints -n backend-services
```

### HPA Durumu

```bash
# Autoscaling durumunu görüntüle
kubectl get hpa -n backend-services

# HPA detaylarý
kubectl describe hpa gateway-api-hpa -n backend-services
```

### Debugging

```bash
# Pod events
kubectl describe pod <pod-name> -n backend-services

# Cluster events
kubectl get events -n backend-services --sort-by='.lastTimestamp'

# DNS test
kubectl run -it --rm debug --image=busybox --restart=Never -- nslookup gateway-api-service.backend-services.svc.cluster.local
```

## ?? Rolling Update

```bash
# Yeni versiyon deploy et
kubectl set image deployment/gateway-api gateway-api=your-registry/gateway-api:v2.0.0 -n backend-services

# Rollout durumunu izle
kubectl rollout status deployment/gateway-api -n backend-services

# Rollback (gerekirse)
kubectl rollout undo deployment/gateway-api -n backend-services
```

## ?? Temizlik

```bash
# Tüm kaynaklarý sil
kubectl delete namespace backend-services

# Sadece deployment'larý sil (infrastructure'ý koru)
kubectl delete -f k8s/services/ -n backend-services
```

## ?? Checklist

### Production'a Geçmeden Önce:

- [ ] Secrets'lar güvenli þekilde yönetiliyor (Key Vault, Sealed Secrets)
- [ ] TLS sertifikasý yapýlandýrýldý
- [ ] Resource limits ayarlandý
- [ ] HPA limitleri gözden geçirildi
- [ ] Network Policies test edildi
- [ ] Backup stratejisi belirlendi (SQL Server, Redis)
- [ ] Monitoring kuruldu (Prometheus, Grafana)
- [ ] Alerting yapýlandýrýldý
- [ ] CI/CD pipeline hazýr
- [ ] Disaster Recovery planý var
