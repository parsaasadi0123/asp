# Development setup

## Prerequisites

- .NET 10 SDK
- SQL Server 2022 or supported SQL Server instance
- Node.js 22+ and npm
- Git

## Backend configuration

Use .NET user-secrets; never commit credentials:

```powershell
cd src/InstaPilot.WebAPI
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=InstaPilot;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:SigningKey" "REPLACE-WITH-A-RANDOM-32-BYTE-OR-LONGER-SECRET"
```

Run the API with `dotnet run --project src/InstaPilot.WebAPI`. The API fails fast if the connection string or strong JWT key is absent.

## Frontend

```powershell
cd frontend/instapilot-web
npm install
npm run dev
```

## Database migrations

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialFoundation --project src/InstaPilot.Infrastructure --startup-project src/InstaPilot.WebAPI
dotnet ef database update --project src/InstaPilot.Infrastructure --startup-project src/InstaPilot.WebAPI
```

Commit generated migrations. Do not run migrations automatically at API startup in production.

## Current limitations

OTP, refresh-token persistence and rotation, tenant authorization policies, payments, Meta OAuth, AI providers, Hangfire jobs, and media rendering are not implemented in this foundation. They must not be represented as live features. Integration tests and actual builds must run in a .NET 10 and Node.js environment before deployment.
