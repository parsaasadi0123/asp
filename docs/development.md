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

Production uses the SMS.ir Verify API (`POST https://api.sms.ir/v1/send/verify`) with the `X-API-KEY` header and an approved template. Configure `SmsIr:ApiKey`, `SmsIr:TemplateId`, and `SmsIr:CodeParameterName` through environment variables or a secret manager; never commit credentials. For example: `SmsIr__ApiKey`, `SmsIr__TemplateId`, and `SmsIr__CodeParameterName`. The template must contain the matching named placeholder (default `CODE`). Development and automated tests use non-production senders.

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
Phases 0–2 provide architecture documentation, Identity/JWT/refresh-token foundations, phone verification via a development-only sender, and workspace membership endpoints. The SMS.ir adapter is implemented but requires the customer API key and approved template ID before real delivery can be verified. Distributed refresh-token family replay detection, email, social OAuth, content publishing, analytics, AI, Hangfire jobs, payments, and video rendering remain later work. Run restore/build/test and apply the migration against a provisioned SQL Server before deployment.
