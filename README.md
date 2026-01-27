# Backend Microservices - Authentication & Authorization API

JWT tabanlı kimlik doğrulama ve yetkilendirme sistemi ile mikroservis mimarisi kullanan .NET 8 projesi.

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

## 🛠️ Teknolojiler

| Teknoloji | Versiyon | Açıklama |
|-----------|----------|----------|
| .NET | 8.0 | Framework |
| YARP | - | Reverse Proxy (API Gateway) |
| Entity Framework Core | 8.0 | ORM |
| SQL Server LocalDB | - | Veritabanı |
| Microsoft Identity | 8.0 | Kimlik Yönetimi |
| JWT Bearer | 8.0 | Token Tabanlı Kimlik Doğrulama |
| Swagger/OpenAPI | 6.6 | API Dokümantasyonu |

## 📁 Proje Yapısı

```
Backend-Case3/
├── Gateway.Api/                 # API Gateway (YARP)
│   ├── Program.cs
│   └── appsettings.json         # YARP route yapılandırması
│
├── Identity.Api/                # Kimlik Doğrulama API
│   ├── Controllers/
│   │   ├── AuthController.cs    # Login, Register, Token işlemleri
│   │   └── AdminController.cs   # Admin yönetim işlemleri
│   └── Program.cs
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
│       └── DbInitializer.cs     # Seed Data (Admin, Roller)
│
├── Products.Api/                # Ürün API (Örnek mikroservis)
├── Products.Application/
├── Products.Domain/
└── Products.Infrastructure/
```

## 🚀 Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server LocalDB (Visual Studio ile birlikte gelir)
- Visual Studio 2022 veya VS Code

### Adım 1: Projeyi Klonlayın
```bash
git clone <repo-url>
cd Backend-Case3
```

### Adım 2: Veritabanını Oluşturun
```bash
# Migration oluştur
dotnet ef migrations add InitialCreate --project Identity.Infrastructure --startup-project Identity.Api --output-dir Migrations

# Veritabanını güncelle
dotnet ef database update --project Identity.Infrastructure --startup-project Identity.Api
```

### Adım 3: Projeyi Çalıştırın

**Terminal ile:**
```bash
# Identity.Api'yi başlat
dotnet run --project Identity.Api

# Yeni terminal - Gateway.Api'yi başlat
dotnet run --project Gateway.Api
```

**Visual Studio ile:**
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

### 3. Korumalı Endpoint'e Erişim
```http
GET http://localhost:5100/api/admin/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 4. Token Yenileme
```http
POST http://localhost:5100/api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "abc123..."
}
```

## ⚙️ Yapılandırma

### JWT Ayarları (`appsettings.json`)
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "IdentityApi",
    "Audience": "BackendServices",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Veritabanı Bağlantısı
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IdentityDb;Trusted_Connection=True;"
  }
}
```

### YARP Route Yapılandırması (`Gateway.Api/appsettings.json`)
```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/auth/{**catch-all}" }
      },
      "admin-route": {
        "ClusterId": "identity-cluster",
        "AuthorizationPolicy": "admin",
        "Match": { "Path": "/api/admin/{**catch-all}" }
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "identity-api": { "Address": "http://localhost:5101" }
        }
      }
    }
  }
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

## 📊 Roller

| Rol | Yetkiler |
|-----|----------|
| **Admin** | Tam yetki - Kullanıcı ve rol yönetimi |
| **Manager** | Yönetici işlemleri |
| **User** | Standart kullanıcı işlemleri |

## 🧪 Test

### Swagger UI
- Identity API: `http://localhost:5101/swagger`

### .http Dosyaları
Visual Studio veya VS Code REST Client ile:
- `Gateway.Api/Gateway.Api.http`
- `Identity.Api/Identity.Api.http`

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

---

**Geliştirici:** Backend Services Case Study

