# Security baseline

## Assets
Identity credentials, phone verification codes, refresh tokens, workspace data, social-provider tokens, media assets, billing records, and AI usage records.

## Trust boundaries
Browser/API; API/database; API/external providers; worker/queue; tenant-to-tenant boundary; platform operator boundary.

## Required controls
- Password hashing through ASP.NET Core Identity; never implement custom password hashing.
- Require verified phone numbers before issuing tokens.
- Hash refresh tokens at rest and rotate them on every refresh.
- Validate JWT issuer, audience, signature, and lifetime.
- Enforce workspace membership and role authorization on every tenant endpoint.
- Use parameterized EF Core queries and database constraints.
- Rate-limit registration, verification, login, and refresh endpoints before public launch.
- Apply HTTPS, secure cookies if cookie auth is introduced, CSRF controls where applicable, and strict CORS origins.
- Validate uploaded file type, size, and content in the media phase.
- Store secrets outside the repository; use managed secret storage in production.
- Keep audit records for membership and ownership changes.
- Never trust WorkspaceId supplied by a client without membership authorization.

## Operational requirements
Rotate signing keys with a planned key-id strategy; back up SQL Server; test restore procedures; alert on repeated refresh-token reuse, authentication failures, and cross-tenant authorization denials.
