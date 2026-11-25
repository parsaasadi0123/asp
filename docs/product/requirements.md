# InstaPilot AI — Product Requirements (Phase 0 baseline)

## Product vision
InstaPilot AI is a Persian-first, multi-tenant SaaS for planning, creating, publishing, analyzing, and automating social-media content. The first delivery target is a secure workspace and identity foundation; integrations and AI capabilities are delivered in later phases.

## Users and roles
- Individual creator: owns one or more workspaces.
- Workspace owner: full control and ownership transfer.
- Workspace admin: manages members and workspace settings.
- Content manager: manages content workflows (later phase).
- Editor: creates and edits content (later phase).
- Analyst: read-only analytics access (later phase).
- Platform operator: cross-tenant administration (future phase 13; separate from workspace roles).

## Product principles
1. Tenant isolation is mandatory: every tenant-owned record is scoped by WorkspaceId and authorization is checked server-side.
2. Official provider APIs only; no scraping or unsupported automation.
3. Secrets, access tokens, OTPs, and payment credentials are never committed to source control.
4. All timestamps are stored in UTC; Persian/Jalali calendar is a presentation concern.
5. Background and external side effects are idempotent, observable, and retry-safe.
6. The UI is Persian-first and RTL, while APIs use stable language-neutral identifiers.

## Phase 0 acceptance
- Architecture boundaries and technology decisions documented.
- Domain, API, persistence, security, and delivery conventions documented.
- MVP scope and explicit non-goals recorded.

## Phase 1 acceptance
- .NET 10 solution restores/builds/tests in CI.
- SQL Server DbContext and Identity schema are migration-ready.
- Configuration is validated at startup; secrets are externalized.
- Health/readiness endpoints and structured error responses exist.
- Architecture and domain tests cover foundational invariants.

## Phase 2 acceptance
- User can register with a phone number and password.
- Phone verification is required before login/token issuance.
- Login returns a short-lived JWT access token and rotating refresh token.
- Refresh tokens are hashed at rest, rotated, and revocable.
- User can create/list workspaces and manage workspace memberships.
- Workspace endpoints enforce membership and role checks server-side.
- Cross-workspace access tests exist.

## MVP scope (after phases 0–2)
Connect supported social accounts, compose/schedule content, content calendar, basic analytics, inbox, and an initial AI assistant. Video rendering, advanced agent orchestration, billing, and super-admin operations follow as separately planned phases.

## Explicit non-goals for phases 0–2
Instagram/Facebook OAuth, publishing, analytics ingestion, AI providers, workflow engine, Remotion rendering, SMS.ir production adapter, ZarinPal, subscription billing, and production deployment.
