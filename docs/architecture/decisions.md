# Architecture decisions

## ADR-001: Modular Clean Architecture
- Domain contains business entities and invariants; it does not depend on frameworks.
- Application contains use-case contracts, validation, and orchestration.
- Infrastructure implements persistence, Identity, and external adapters.
- WebAPI is the HTTP composition root.
- Worker hosts background processing; it does not own business rules.

## ADR-002: Persistence
SQL Server is the system of record. EF Core 10 owns persistence mappings and migrations. Production schema changes are deployed as reviewed migration artifacts; the API must not silently migrate a production database on startup.

## ADR-003: Identity and tenancy
ASP.NET Core Identity uses Guid keys. A user may belong to multiple workspaces. Workspace membership is the authorization boundary for tenant-owned resources. Platform administration is a separate future role system and must never be inferred from workspace ownership.

## ADR-004: Authentication
JWT access tokens are short-lived. Refresh tokens are random opaque values; only SHA-256 hashes are persisted. Refresh tokens rotate on use and can be revoked. Signing keys come from secret storage or environment configuration.

## ADR-005: Time and localization
Persist UTC timestamps. Convert to the user's selected timezone at API/UI boundaries. Jalali dates are formatting/input concerns, not database storage types.

## ADR-006: External integrations
Integrations use provider-specific adapters behind interfaces. OAuth credentials and provider tokens are encrypted at rest. Only official documented APIs and permitted scopes are allowed.

## ADR-007: Observability
Use structured logs with correlation identifiers, health/readiness checks, and auditable security-sensitive actions. Do not log passwords, OTPs, refresh tokens, authorization headers, or provider secrets.
