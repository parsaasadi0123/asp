# Phase 3–4 implementation notes

This branch starts the dashboard/admin and Meta integration vertical slices on top of
`phase-1-foundation`. It is intentionally separate from the Phase 0–2 draft PR.

## Phase 3 — Workspace dashboard foundation

- `GET /api/v1/workspaces/{workspaceId}/dashboard` returns real workspace metadata,
  membership totals by role, current user's role, and connected Meta account count.
- The endpoint is workspace-member scoped. A non-member receives `404`; it does not
  reveal workspace existence or member counts.
- Existing workspace member endpoints remain the source of truth for member management.
- Content performance, scheduled posts, inbox counts, and AI recommendations are not
  returned until those data sources are implemented. Dashboard mock visualizations in
  the frontend must not be treated as live analytics.

## Phase 4 — Meta OAuth foundation

- Owner/Admin can request an OAuth authorization URL for a workspace:
  `GET /api/v1/meta/connect/{workspaceId}`.
- OAuth state is a random GUID persisted with workspace/user ownership and a 10-minute
  expiry. Callback consumes it with a conditional database update, preventing replay.
- Callback exchanges the authorization code server-to-server, fetches the Meta profile,
  and stores the access token protected by ASP.NET Core Data Protection.
- `GET /api/v1/meta/accounts/{workspaceId}` returns safe account metadata only.
- Owner/Admin can disconnect an account; the protected token is soft-deleted with it.
- App secret and token values are never returned by API responses or written to logs.

## Required environment configuration

Use deployment/GitHub secret settings; do not commit values:

- `MetaOAuth__AppId`
- `MetaOAuth__AppSecret`
- `MetaOAuth__RedirectUri` (must exactly match the Meta app callback URL)
- `MetaOAuth__GraphApiVersion` (set to a currently supported Graph API version)
- `MetaOAuth__FrontendBaseUrl`

The callback path is `/api/v1/meta/callback`. The Meta app must be configured for
Facebook Login/OAuth and request the required scopes. Permissions may require Meta App
Review and business verification before production users can authorize them.

## Deployment/security gates

- Persist and protect the ASP.NET Core Data Protection key ring in a durable, access-
  restricted shared store in production. Losing the key ring makes stored tokens
  undecryptable; sharing it improperly exposes them.
- Configure HTTPS and the exact public callback URL.
- Apply the new SQL Server migration before deploying these endpoints.
- Complete Meta App Review, then test a real Facebook Page with a linked Instagram
  professional account. This initial slice stores the Meta profile token; Page/Instagram
  discovery, Insights, publishing, webhooks, and DM access remain subsequent work.
