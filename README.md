# InstaPilot AI

AI-powered Instagram management SaaS for the Iranian market.

## Repository status

This repository is being initialized from an empty GitHub repository. The implementation is organized as a phased, production-oriented Clean Architecture solution.

## Technology baseline

- Backend: ASP.NET Core 10, C#, EF Core 10, SQL Server
- Frontend: Next.js App Router, TypeScript, Tailwind CSS
- Background processing: Hangfire with SQL Server storage
- Video rendering: independent Remotion/Node.js service
- Localization: Persian (fa-IR), RTL, Jalali presentation, Asia/Tehran

## Solution layout

- `src/` — .NET solution projects
- `frontend/instapilot-web/` — Next.js application
- `RemotionRenderer/` — independent video rendering service
- `tests/` — unit, integration, and architecture tests
- `docs/` — architecture and deployment documentation

## Current phase

Phase 1: repository and solution foundation. External integrations are not considered operational until credentials are configured and live integration tests have been performed.

## Development

See `docs/development.md` for prerequisites and local setup. Never commit secrets; use user secrets in development and protected environment variables in production.
