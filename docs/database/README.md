# Database migrations

The initial SQL Server migration creates ASP.NET Core Identity tables, workspaces,
workspace memberships, and hashed refresh-token storage.

## Apply

Set the SQL Server connection string through a secure environment variable or user
secrets; never commit credentials:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=InstaPilot;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=False"
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef database update --project src/InstaPilot.Infrastructure --startup-project src/InstaPilot.WebAPI
```

The design-time factory reads `ConnectionStrings__DefaultConnection`. The migration
is SQL Server-specific (including the SQL Server `rowversion` column). The initial migration is explicit SQL; an EF Core model snapshot has not yet been scaffolded. Before adding future model-driven migrations with `dotnet ef migrations add`, generate and commit a matching `InstaPilotDbContextModelSnapshot`.

Before applying to an existing database, take a backup and verify that the database
is empty or that its schema is compatible. Production deployments should run reviewed
migrations as a release step; the API does not auto-migrate on startup.

## Notes

The migration is intentionally explicit SQL to preserve the SQL Server schema and
constraints. After changing the EF model, create and review a new migration and keep
the model snapshot synchronized before shipping schema changes.
