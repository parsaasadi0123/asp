# Development setup

## Prerequisites
- .NET 10 SDK
- SQL Server 2022 or a supported SQL Server instance
- Node.js 22+ and npm
- Git

## Backend secrets (PowerShell)
```powershell
cd src/InstaPilot.WebAPI
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=InstaPilot;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:SigningKey" ([Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48)))
```
Run with `dotnet run --project src/InstaPilot.WebAPI`. The API fails fast when required configuration is absent.

## Phase 2 authentication flow
1. POST `/api/v1/auth/register` with phone number, password, and display name.
2. In Development only, the verification code is written to the API log. This is a local development adapter, not an SMS gateway.
3. POST `/api/v1/auth/verify-phone` with userId and code.
4. POST `/api/v1/auth/login` to receive access and refresh tokens.
5. POST `/api/v1/auth/refresh` to rotate a refresh token; POST `/api/v1/auth/logout` to revoke it.

Production registration intentionally fails until a production `IPhoneVerificationSender` implementation is wired to the approved SMS.ir account. Do not expose development OTP logging in production.

## Frontend
```powershell
cd frontend/instapilot-web
npm install
npm run dev
```

## EF Core migrations
```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=InstaPilot;Trusted_Connection=True;TrustServerCertificate=True"
dotnet ef migrations add InitialFoundation --project src/InstaPilot.Infrastructure --startup-project src/InstaPilot.WebAPI
dotnet ef database update --project src/InstaPilot.Infrastructure --startup-project src/InstaPilot.WebAPI
```
Commit generated migrations. Do not run migrations automatically at API startup in production.

## Current scope boundary
Phases 0–2 provide architecture documentation, Identity/JWT/refresh-token foundations, phone verification via a development-only sender, and workspace membership endpoints. Production SMS.ir delivery, distributed refresh-token replay detection, rate limiting, email, social OAuth, content publishing, analytics, AI, Hangfire jobs, payments, and video rendering remain later work. Run restore/build/test and generate the SQL Server migration in a .NET 10 environment before deployment.
