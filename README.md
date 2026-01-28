# Backend Microservices - Authentication, Authorization & Products API

JWT tabanlı kimlik doğrulama/yetkilendirme sistemi ve CQRS pattern ile ürün yönetimi mikroservisleri. .NET 8 + Kafka + Docker.

**🎯 12-Factor App Prensiplerine Uygun Geliştirme

## 🏗️ Mimari Yapı

```
┌─────────────────────────────────────────────────────────────────┐
│                    API GATEWAY (YARP)                           │
│                   http://localhost:5100                         │
│                                                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │ JWT Doğrula │  │ Yetkilendir │  │ İstek Yönlendir (Proxy) │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
            ┌───────────────┼───────────────┐
            │               │               │
            ▼               ▼               ▼
┌───────────────────┐ ┌───────────────────┐ ┌───────────────────┐
│   Identity.Api    │ │   Products.Api    │ │      Kafka        │
│  localhost:5101   │ │  localhost:5102   │ │  localhost:9092   │
│                   │ │                   │ │                   │
│ • Kullanıcı Kaydı │ │ • Ürün CRUD       │ │ • Event Streaming │
│ • Login/Logout    │ │ • Kategori        │ │ • product-events  │
│ • JWT Token       │ │ • CQRS Pattern    │ │                   │
│ • Refresh Token   │ │ • MediatR         │ │                   │
│ • Rol Yönetimi    │ │ • Kafka Events    │ │                   │
└───────────────────┘ └───────────────────┘ └───────────────────┘
           │                   │
           └─────────┬─────────┘
                     ▼
            ┌───────────────────┐
            │    SQL Server     │
            │  localhost:1433   │
            └───────────────────┘
```

## 📋 12-Factor App Uyumluluğu

| Faktör | Uygulama | Detay |
|--------|----------|-------|
| **1. Kod Tabanı** | ✅ Git | Merkezi kod deposu, `.gitignore` yapılandırılmış |
| **2. Bağımlılıklar** | ✅ NuGet | Tüm bağımlılıklar `.csproj` dosyalarında tanımlı |
| **3. Konfigürasyon** | ✅ Environment Variables | `appsettings.json` + ortam değişkenleri |
| **4. Destek Servisleri** | ✅ Ayrılmış | SQL Server, Kafka, mikroservisler bağımsız |
| **5. Build/Run Ayrımı** | ✅ Docker | Multi-stage build, ayrı derleme ve çalışma |
| **6. Stateless** | ✅ JWT | Token tabanlı, session yok |
| **7. Port Bağımsızlığı** | ✅ Konfigüre edilebilir | Environment ile port belirlenir |
| **8. Concurrency** | ✅ Async/Await | Yatay ölçeklenebilir tasarım |
| **9. Disposability** | ✅ Graceful Shutdown | Kaynaklar düzgün serbest bırakılır |
| **10. Dev/Prod Paritesi** | ✅ Docker Compose | Aynı yapılandırma, farklı env |
| **11. Loglar** | ✅ Serilog | Yapılandırılmış loglar, stdout |
| **12. Admin Prosesleri** | ✅ Health Checks | `/health`, `/health/live`, `/health/ready` |

## 🛠️ Teknolojiler

| Teknoloji | Versiyon | Açıklama |
|-----------|----------|----------|
| .NET | 8.0 | Framework |
| YARP | - | Reverse Proxy (API Gateway) |
| Entity Framework Core | 8.0 | ORM |
| SQL Server | 2022 | Veritabanı |
| Microsoft Identity | 8.0 | Kimlik Yönetimi |
| JWT Bearer | 8.0 | Token Tabanlı Kimlik Doğrulama |
| MediatR | 12.2 | CQRS Pattern |
| FluentValidation | 11.9 | Validasyon |
| Confluent.Kafka | 2.3 | Event Streaming |
| Serilog | 8.0 | Yapılandırılmış Loglama |
| Docker | - | Konteynerizasyon |
| Swagger/OpenAPI | 6.6 | API Dokümantasyonu |

## 📁 Proje Yapısı

```
Backend-Case3/
│
├── gateway/                              # API Gateway
│   └── Gateway.Api/
│       ├── Program.cs                    # YARP, JWT, Serilog, Health Checks
│       ├── appsettings.json              # Route yapılandırması
│       ├── appsettings.Development.json
│       ├── Dockerfile
│       └── Gateway.Api.http              # HTTP test dosyası
│
├── services/                             # Mikroservisler
│   │
│   ├── Identity/                         # 🔐 Identity Mikroservisi (Onion Architecture)
│   │   │
│   │   ├── Identity.Api/                 # Sunum Katmanı
│   │   │   ├── Controllers/
│   │   │   │   ├── AuthController.cs     # Login, Register, Token
│   │   │   │   └── AdminController.cs    # Kullanıcı/Rol yönetimi
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   ├── Dockerfile
│   │   │   └── Identity.Api.http
│   │   │
│   │   ├── Identity.Application/         # İş Mantığı Katmanı
│   │   │   ├── Models/
│   │   │   │   ├── DTOs/
│   │   │   │   └── JwtSettings.cs
│   │   │   └── Services/
│   │   │       ├── IAuthService.cs
│   │   │       └── AuthService.cs
│   │   │
│   │   ├── Identity.Domain/              # Domain Katmanı
│   │   │   └── Entities/
│   │   │       ├── ApplicationUser.cs
│   │   │       └── RefreshToken.cs
│   │   │
│   │   └── Identity.Infrastructure/      # Altyapı Katmanı
│   │       ├── Data/
│   │       │   ├── ApplicationDbContext.cs
│   │       │   └── DbInitializer.cs      # Seed (Admin, Roller)
│   │       └── Migrations/
│   │
│   └── Products/                         # 📦 Products Mikroservisi (Onion + CQRS)
│       │
│       ├── Products.Api/                 # Sunum Katmanı
│       │   ├── Controllers/
│       │   │   ├── ProductsController.cs
│       │   │   └── CategoriesController.cs
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   ├── Dockerfile
│       │   └── Products.Api.http
│       │
│       ├── Products.Application/         # İş Mantığı Katmanı (CQRS)
│       │   ├── Commands/                 # CreateProduct, UpdateProduct, DeleteProduct, UpdateStock
│       │   ├── Queries/                  # GetAllProducts, GetProductById, SearchProducts
│       │   ├── Handlers/                 # Command & Query Handlers
│       │   ├── Validators/               # FluentValidation
│       │   ├── Behaviors/                # MediatR Pipeline (Validation)
│       │   └── DTOs/
│       │
│       ├── Products.Domain/              # Domain Katmanı
│       │   ├── Common/
│       │   │   ├── BaseEntity.cs
│       │   │   └── IDomainEvent.cs
│       │   ├── Entities/
│       │   │   ├── Product.cs
│       │   │   └── Category.cs
│       │   ├── Events/
│       │   │   ├── ProductCreatedEvent.cs
│       │   │   ├── ProductUpdatedEvent.cs
│       │   │   ├── ProductStockChangedEvent.cs
│       │   │   └── ProductDeletedEvent.cs
│       │   └── Interfaces/
│       │       ├── IRepository.cs
│       │       ├── IProductRepository.cs
│       │       ├── ICategoryRepository.cs
│       │       └── IEventPublisher.cs
│       │
│       └── Products.Infrastructure/      # Altyapı Katmanı
│           ├── Data/
│           │   ├── ProductsDbContext.cs
│           │   ├── DbInitializer.cs      # Seed (Kategoriler, Ürünler)
│           │   ├── Configurations/       # EF Core Configurations
│           │   └── Migrations/
│           ├── Repositories/
│           │   ├── Repository.cs
│           │   ├── ProductRepository.cs
│           │   └── CategoryRepository.cs
│           └── Messaging/
│               ├── KafkaSettings.cs
│               ├── KafkaEventPublisher.cs
│               └── NullEventPublisher.cs
│
├── docker-compose.yml                    # Docker Compose (SQL, Kafka, Services)
├── docker-compose.override.yml           # Development overrides
├── Backend-Case3.sln                     # Solution dosyası
└── README.md
```

## 🚀 Kurulum

### Gereksinimler
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 veya VS Code

### Docker ile Çalıştırma (Önerilen)

```bash
# Projeyi klonla
git clone <repo-url>
cd Backend-Case3

# Docker Compose ile başlat
docker-compose up -d

# Servislerin durumunu kontrol et
docker-compose ps

# Logları izle
docker-compose logs -f

# Durdur
docker-compose down
```

### Yerel Geliştirme

```bash
# SQL Server ve Kafka'yı Docker'da başlat
docker-compose up -d sqlserver kafka zookeeper

# Migration oluştur (services klasörü içinden)
dotnet ef migrations add InitialCreate --project services/Identity/Identity.Infrastructure --startup-project services/Identity/Identity.Api
dotnet ef migrations add InitialCreate --project services/Products/Products.Infrastructure --startup-project services/Products/Products.Api

# Servisleri başlat (ayrı terminallerde)
dotnet run --project services/Identity/Identity.Api
dotnet run --project services/Products/Products.Api
dotnet run --project gateway/Gateway.Api
```

### Visual Studio ile Çalıştırma
1. Solution'a sağ tıklayın → "Configure Startup Projects..."
2. "Multiple startup projects" seçin
3. `Gateway.Api`, `Identity.Api`, `Products.Api` için "Start" seçin
4. F5 ile çalıştırın

## 🔐 Varsayılan Admin Kullanıcı

Uygulama ilk çalıştırıldığında otomatik olarak oluşturulur:

| Alan | Değer |
|------|-------|
| **Email** | `admin@example.com` |
| **Şifre** | `Admin123!` |
| **Rol** | Admin |

## 📡 API Endpoint'leri

### Base URL'ler
| Servis | URL |
|--------|-----|
| API Gateway | `http://localhost:5100` |
| Identity API (Swagger) | `http://localhost:5101/swagger` |
| Products API (Swagger) | `http://localhost:5102/swagger` |
| Kafka UI | `http://localhost:8081` |

### Auth Endpoint'leri (Herkese Açık)

| Metod | Endpoint | Açıklama |
|-------|----------|----------|
| POST | `/api/auth/register` | Yeni kullanıcı kaydı |
| POST | `/api/auth/login` | Kullanıcı girişi |
| POST | `/api/auth/refresh-token` | Access token yenileme |
| POST | `/api/auth/revoke-token` | Logout (Token iptal) |
| GET | `/api/auth/me` | Mevcut kullanıcı bilgisi |

### Admin Endpoint'leri (Admin Rolü Gerekli)

| Metod | Endpoint | Açıklama |
|-------|----------|----------|
| GET | `/api/admin/users` | Tüm kullanıcıları listele |
| GET | `/api/admin/users/{id}` | Kullanıcı detayı |
| POST | `/api/admin/users/{id}/roles/{role}` | Kullanıcıya rol ata |
| DELETE | `/api/admin/users/{id}/roles/{role}` | Kullanıcıdan rol kaldır |
| GET | `/api/admin/roles` | Tüm rolleri listele |
| POST | `/api/admin/roles` | Yeni rol oluştur |
| DELETE | `/api/admin/roles/{name}` | Rol sil |

### Products Endpoint'leri

| Metod | Endpoint | Yetki | Açıklama |
|-------|----------|-------|----------|
| GET | `/api/products` | Herkese Açık | Tüm ürünler (sayfalama) |
| GET | `/api/products/{id}` | Herkese Açık | Ürün detay |
| GET | `/api/products/category/{categoryId}` | Herkese Açık | Kategoriye göre ürünler |
| GET | `/api/products/search?q=...` | Herkese Açık | Ürün arama |
| POST | `/api/products` | Admin | Ürün ekle (Kafka event) |
| PUT | `/api/products/{id}` | Admin | Ürün güncelle |
| DELETE | `/api/products/{id}` | Admin | Ürün sil |
| PATCH | `/api/products/{id}/stock` | Admin | Stok güncelle |

### Categories Endpoint'leri

| Metod | Endpoint | Yetki | Açıklama |
|-------|----------|-------|----------|
| GET | `/api/categories` | Herkese Açık | Tüm kategoriler |
| GET | `/api/categories/tree` | Herkese Açık | Kategori ağacı (hiyerarşik) |
| POST | `/api/categories` | Admin | Kategori oluştur |

### Health Check Endpoint'leri

| Endpoint | Açıklama |
|----------|----------|
| `/health` | Tüm sağlık kontrolleri (JSON) |
| `/health/live` | Liveness probe |
| `/health/ready` | Readiness probe |

## 📦 Örnek Kategoriler ve Ürünler (Seed Data)

### Kategoriler
- İşlemciler (CPU)
- Ekran Kartları (GPU)
- Bellekler (RAM)
- Depolama (SSD, HDD)
- Anakartlar
- Güç Kaynakları (PSU)
- Kasalar
- Soğutma

### Örnek Ürünler
- Intel Core i9-14900K
- AMD Ryzen 9 7950X3D
- NVIDIA GeForce RTX 4090
- AMD Radeon RX 7900 XTX
- G.Skill Trident Z5 RGB DDR5
- Samsung 990 Pro NVMe
- ASUS ROG Maximus Z790 Hero
- Corsair HX1500i PSU
- Lian Li O11 Dynamic EVO
- NZXT Kraken Z73 RGB

## 📝 Kullanım Örnekleri

### 1. Login
```http
POST http://localhost:5100/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

### 2. Ürünleri Listele
```http
GET http://localhost:5100/api/products?pageNumber=1&pageSize=10
```

### 3. Ürün Ara
```http
GET http://localhost:5100/api/products/search?q=intel
```

### 4. Yeni Ürün Ekle (Admin)
```http
POST http://localhost:5100/api/products
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Intel Core i7-14700K",
  "description": "20 çekirdekli işlemci",
  "sku": "CPU-INT-I7-14700K",
  "brand": "Intel",
  "price": 18999.99,
  "stock": 20,
  "categoryId": "category-guid"
}
```

## 🔒 Güvenlik Özellikleri

- ✅ JWT Bearer Token Authentication
- ✅ Refresh Token Mekanizması (Token Rotation)
- ✅ Role-Based Authorization (RBAC)
- ✅ Password Hashing (ASP.NET Identity)
- ✅ Token Expiration & Revocation
- ✅ CORS Politikası
- ✅ API Gateway ile Merkezi Kimlik Doğrulama
- ✅ Non-root Docker kullanıcısı
- ✅ Input Validation (FluentValidation)

## 📊 CQRS Pattern

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Controller │────▶│   MediatR   │────▶│   Handler   │
└─────────────┘     └─────────────┘     └─────────────┘
                           │                    │
                    ┌──────┴──────┐             │
                    │             │             ▼
              ┌─────┴─────┐ ┌─────┴─────┐ ┌──────────┐
              │  Command  │ │   Query   │ │Repository│
              └───────────┘ └───────────┘ └──────────┘
                    │                          │
                    ▼                          ▼
              ┌───────────┐           ┌──────────────┐
              │   Kafka   │           │   Database   │
              │   Event   │           │              │
              └───────────┘           └──────────────┘
```

## 🎯 Kafka Events

| Event | Açıklama |
|-------|----------|
| `ProductCreatedEvent` | Ürün oluşturulduğunda |
| `ProductUpdatedEvent` | Ürün güncellendiğinde |
| `ProductStockChangedEvent` | Stok değiştiğinde |
| `ProductDeletedEvent` | Ürün silindiğinde |

## 🐳 Docker Komutları

```bash
# Build
docker-compose build

# Başlat
docker-compose up -d

# Logları izle
docker-compose logs -f products-api
docker-compose logs -f kafka

# Kafka topic'leri listele
docker exec backend-kafka kafka-topics --list --bootstrap-server localhost:9092

# Durdur ve temizle
docker-compose down -v
```

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

---


**Geliştirici:** Ekrem-A


