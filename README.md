# Backend Microservices - Authentication & Authorization API

JWT tabanlı kimlik doğrulama ve yetkilendirme sistemi ile mikroservis mimarisi kullanan .NET 8 projesi.

**🎯 12-Factor App Prensiplerine Uygun Geliştirmeler yapılıyor.

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
            ┌───────────────┴───────────────┐
            │                               │
            ▼                               ▼
┌───────────────────────┐       ┌───────────────────────┐
│    Identity.Api       │       │    Products.Api       │
│  localhost:5101       │       │  localhost:5102       │
│                       │       │                       │
│ • Kullanıcı Kaydı     │       │ • Ürün CRUD           │
│ • Login/Logout        │       │ • Kategori Yönetimi   │
│ • JWT Token           │       │                       │
│ • Refresh Token       │       │                       │
│ • Rol Yönetimi        │       │                       │
│ • Admin İşlemleri     │       │                       │
└───────────────────────┘       └───────────────────────┘
```

## 📋 12-Factor App Uyumluluğu

| Faktör | Uygulama | Detay |
|--------|----------|-------|
| **1. Kod Tabanı** | ✅ Git | Merkezi kod deposu, `.gitignore` yapılandırılmış |
| **2. Bağımlılıklar** | ✅ NuGet | Tüm bağımlılıklar `.csproj` dosyalarında tanımlı |
| **3. Konfigürasyon** | ✅ Environment Variables | `appsettings.json` + ortam değişkenleri |
| **4. Destek Servisleri** | ✅ Ayrılmış | SQL Server, mikroservisler bağımsız |
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
| SQL Server LocalDB | - | Veritabanı |
| Microsoft Identity | 8.0 | Kimlik Yönetimi |
| JWT Bearer | 8.0 | Token Tabanlı Kimlik Doğrulama |
| Serilog | 8.0 | Yapılandırılmış Loglama |
| Docker | - | Konteynerizasyon |
| Swagger/OpenAPI | 6.6 | API Dokümantasyonu |

## 📁 Proje Yapısı

```
Backend-Case3/
├── Gateway.Api/                 # API Gateway (YARP)
│   ├── Program.cs              # Serilog, Health Checks
│   ├── Dockerfile              # Docker build
│   └── appsettings.json        # YARP route yapılandırması
│
├── Identity.Api/                # Kimlik Doğrulama API
│   ├── Controllers/
│   │   ├── AuthController.cs   # Login, Register, Token işlemleri
│   │   └── AdminController.cs  # Admin yönetim işlemleri
│   ├── Program.cs              # Serilog, Health Checks, Graceful Shutdown
│   └── Dockerfile              # Docker build
│
├── Identity.Application/        # İş mantığı katmanı
│   ├── Models/
│   │   ├── DTOs/               # Data Transfer Objects
│   │   └── JwtSettings.cs
│   └── Services/
│       ├── IAuthService.cs
│       └── AuthService.cs
│
├── Identity.Domain/             # Domain katmanı
│   └── Entities/
│       ├── ApplicationUser.cs
│       └── RefreshToken.cs
│
├── Identity.Infrastructure/     # Altyapı katmanı
│   └── Data/
│       ├── ApplicationDbContext.cs
│       └── DbInitializer.cs    # Seed Data (Admin, Roller)
│
├── docker-compose.yml          # Docker Compose
├── docker-compose.override.yml # Development override
├── .gitignore                  # Git ignore
└── README.md
```

## 🚀 Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server LocalDB (Visual Studio ile birlikte gelir)
- Docker Desktop (opsiyonel)
- Visual Studio 2022 veya VS Code

### Seçenek 1: Yerel Geliştirme

```bash
# Projeyi klonla
git clone <repo-url>
cd Backend-Case3

# Migration oluştur
dotnet ef migrations add InitialCreate --project Identity.Infrastructure --startup-project Identity.Api --output-dir Migrations

# Veritabanını güncelle
dotnet ef database update --project Identity.Infrastructure --startup-project Identity.Api

# Servisleri başlat (ayrı terminallerde)
dotnet run --project Identity.Api
dotnet run --project Gateway.Api
```

### Seçenek 2: Docker ile Çalıştırma

```bash
# Environment değişkenlerini ayarla
copy env.example.txt .env
# .env dosyasını düzenleyin

# Docker Compose ile başlat
docker-compose up -d

# Logları izle
docker-compose logs -f

# Durdur
docker-compose down
```

### Visual Studio ile Çalıştırma
1. Solution'a sağ tıklayın → "Configure Startup Projects..."
2. "Multiple startup projects" seçin
3. `Gateway.Api` ve `Identity.Api` için "Start" seçin
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

### Health Check Endpoint'leri
| Endpoint | Açıklama |
|----------|----------|
| `/health` | Tüm sağlık kontrolleri (JSON) |
| `/health/live` | Liveness probe (uygulama çalışıyor mu?) |
| `/health/ready` | Readiness probe (bağımlılıklar hazır mı?) |

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

## 📝 Kullanım Örnekleri

### 1. Kullanıcı Kaydı
```http
POST http://localhost:5100/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### 2. Login
```http
POST http://localhost:5100/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

**Yanıt:**
```json
{
  "success": true,
  "message": "Login successful.",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "abc123...",
  "accessTokenExpiration": "2026-01-27T16:00:00Z",
  "user": {
    "id": "...",
    "email": "admin@example.com",
    "firstName": "System",
    "lastName": "Admin"
  }
}
```

### 3. Health Check
```http
GET http://localhost:5100/health
```

**Yanıt:**
```json
{
  "status": "Healthy",
  "gateway": "YARP API Gateway",
  "checks": [
    { "name": "self", "status": "Healthy", "duration": 0.5 },
    { "name": "identity-api", "status": "Healthy", "duration": 15.2 }
  ],
  "totalDuration": 15.8
}
```

## ⚙️ Konfigürasyon (12-Factor: Config)

### Environment Variables
```bash
# JWT Ayarları
JWT_SECRET_KEY=your-super-secret-key-min-32-characters
JWT_ISSUER=IdentityApi
JWT_AUDIENCE=BackendServices

# Veritabanı
DATABASE_CONNECTION_STRING=Server=...;Database=IdentityDb;...

# Ortam
ASPNETCORE_ENVIRONMENT=Production
```

### appsettings.json Yapısı
```json
{
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_CONNECTION_STRING}"
  }
}
```

### Ortam Bazlı Yapılandırma
- `appsettings.json` - Temel yapılandırma (placeholder'lar)
- `appsettings.Development.json` - Development değerleri
- `appsettings.Production.json` - Production değerleri
- Environment Variables - Hassas veriler (secrets)

## 🔒 Güvenlik Özellikleri

- ✅ JWT Bearer Token Authentication
- ✅ Refresh Token Mekanizması (Token Rotation)
- ✅ Role-Based Authorization (RBAC)
- ✅ Password Hashing (ASP.NET Identity)
- ✅ Token Expiration & Revocation
- ✅ CORS Politikası
- ✅ API Gateway ile Merkezi Kimlik Doğrulama
- ✅ Non-root Docker kullanıcısı

## 📊 Roller

| Rol | Yetkiler |
|-----|----------|
| **Admin** | Tam yetki - Kullanıcı ve rol yönetimi |
| **Manager** | Yönetici işlemleri |
| **User** | Standart kullanıcı işlemleri |

## 📋 Loglama (12-Factor: Logs)

Proje Serilog kullanarak yapılandırılmış loglama sağlar:

```
[14:32:15 INF] [Identity.Api] Starting Identity.Api service...
[14:32:16 INF] [Identity.Api] Database seeding completed successfully
[14:32:16 INF] [Identity.Api] Identity.Api started successfully
[14:32:20 INF] [Identity.Api] HTTP POST /api/Auth/login responded 200 in 125.4532 ms
```

**Özellikler:**
- Yapılandırılmış JSON formatı
- Service name ile etiketleme
- Environment ve machine name enrichment
- HTTP request logging
- Console output (stdout - 12 Factor uyumlu)

## 🧪 Test

### Swagger UI
- Identity API: `http://localhost:5101/swagger`

### .http Dosyaları
Visual Studio veya VS Code REST Client ile:
- `Gateway.Api/Gateway.Api.http`
- `Identity.Api/Identity.Api.http`

### Health Checks
```bash
# Gateway health
curl http://localhost:5100/health

# Identity API health
curl http://localhost:5101/health

# Liveness probe
curl http://localhost:5100/health/live

# Readiness probe
curl http://localhost:5100/health/ready
```

## 🐳 Docker Komutları

```bash
# Build
docker-compose build

# Başlat
docker-compose up -d

# Logları izle
docker-compose logs -f gateway-api
docker-compose logs -f identity-api

# Durdur ve temizle
docker-compose down -v

# Sadece belirli servisi yeniden başlat
docker-compose restart identity-api
```

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

---

**Geliştirici:** Backend Services Case Study
