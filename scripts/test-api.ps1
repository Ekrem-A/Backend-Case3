# API Test Script
# Kubernetes'te çalýþan Backend Services API'lerini test eder

param(
    [string]$BaseUrl = "http://localhost:5100"
)

$ErrorActionPreference = "Continue"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Backend Services - API Test" -ForegroundColor Cyan
Write-Host "  Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Yellow
    Write-Host "  $Method $Url" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
            Headers = $Headers
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "  [OK] Success" -ForegroundColor Green
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  [FAIL] Status: $statusCode - $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# ==================== HEALTH CHECKS ====================
Write-Host "`n--- HEALTH CHECKS ---" -ForegroundColor Magenta

$health = Test-Endpoint -Name "Gateway Health" -Url "$BaseUrl/health"
if ($health) {
    Write-Host "  Status: $($health.status)" -ForegroundColor Cyan
    foreach ($check in $health.checks) {
        $color = if ($check.status -eq "Healthy") { "Green" } else { "Red" }
        Write-Host "    - $($check.name): $($check.status)" -ForegroundColor $color
    }
}

# ==================== AUTH TESTS ====================
Write-Host "`n--- AUTHENTICATION ---" -ForegroundColor Magenta

# Login
$loginResult = Test-Endpoint -Name "Admin Login" -Url "$BaseUrl/api/auth/login" -Method "POST" -Body @{
    email = "admin@example.com"
    password = "Admin123!"
}

$token = $null
if ($loginResult -and $loginResult.accessToken) {
    $token = $loginResult.accessToken
    Write-Host "  Access Token: $($token.Substring(0, 30))..." -ForegroundColor Cyan
    Write-Host "  Expires: $($loginResult.expiresAt)" -ForegroundColor Cyan
}

# ==================== PUBLIC ENDPOINTS ====================
Write-Host "`n--- PUBLIC ENDPOINTS ---" -ForegroundColor Magenta

# Products List
$products = Test-Endpoint -Name "Get Products" -Url "$BaseUrl/api/products"
if ($products -and $products.items) {
    Write-Host "  Total Products: $($products.totalCount)" -ForegroundColor Cyan
    Write-Host "  First 3 products:" -ForegroundColor Cyan
    $products.items | Select-Object -First 3 | ForEach-Object {
        Write-Host "    - $($_.name) ($($_.brand)) - $($_.price) TL" -ForegroundColor White
    }
}

# Categories
$categories = Test-Endpoint -Name "Get Categories" -Url "$BaseUrl/api/categories"
if ($categories) {
    Write-Host "  Categories:" -ForegroundColor Cyan
    $categories | ForEach-Object {
        Write-Host "    - $($_.name)" -ForegroundColor White
    }
}

# Product Search
$searchResult = Test-Endpoint -Name "Search Products (intel)" -Url "$BaseUrl/api/products/search?q=intel"
if ($searchResult) {
    Write-Host "  Found: $($searchResult.Count) products" -ForegroundColor Cyan
}

# ==================== PROTECTED ENDPOINTS ====================
if ($token) {
    Write-Host "`n--- PROTECTED ENDPOINTS (Admin) ---" -ForegroundColor Magenta
    
    $authHeaders = @{ Authorization = "Bearer $token" }
    
    # Admin Users
    $users = Test-Endpoint -Name "Get All Users (Admin)" -Url "$BaseUrl/api/admin/users" -Headers $authHeaders
    if ($users) {
        Write-Host "  Users:" -ForegroundColor Cyan
        $users | ForEach-Object {
            Write-Host "    - $($_.email) (Roles: $($_.roles -join ', '))" -ForegroundColor White
        }
    }
    
    # Admin Roles
    $roles = Test-Endpoint -Name "Get All Roles (Admin)" -Url "$BaseUrl/api/admin/roles" -Headers $authHeaders
    if ($roles) {
        Write-Host "  Roles: $($roles.name -join ', ')" -ForegroundColor Cyan
    }
    
    # Current User Info
    $me = Test-Endpoint -Name "Get Current User" -Url "$BaseUrl/api/auth/me" -Headers $authHeaders
    if ($me) {
        Write-Host "  Current User: $($me.email)" -ForegroundColor Cyan
    }
}

# ==================== SUMMARY ====================
Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "  Test Completed!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Manual test URLs:" -ForegroundColor Yellow
Write-Host "  - Swagger (via Gateway): Not available (direct access needed)"
Write-Host "  - Identity Swagger: kubectl port-forward svc/identity-api-service 5101:80 -n backend-services"
Write-Host "  - Products Swagger: kubectl port-forward svc/products-api-service 5102:80 -n backend-services"
Write-Host ""
