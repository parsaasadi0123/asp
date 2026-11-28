# Acceptance criteria — Phases 0–2

This checklist is the release gate for the foundation milestone. A source-code change is
not considered complete until the corresponding test or operational evidence is attached.

## Phase 0 — Product and architecture

- [x] Product scope and MVP boundary documented.
- [x] Clean Architecture project boundaries and deployment constraints documented.
- [x] SQL Server, Identity/JWT, workspace tenancy, and UTC persistence decisions documented.
- [ ] Threat-model review signed off by the product/security owner.
- [ ] Architecture decisions reviewed against the final hosting environment.

## Phase 1 — Platform foundation

- [x] .NET 10 solution and projects are present.
- [x] SQL Server EF Core DbContext and design-time factory are present.
- [x] Serilog, exception handling, health endpoints, CORS, and JWT validation are configured.
- [x] CI workflow covers backend restore/build/test and frontend install/typecheck/build.
- [x] Initial SQL Server migration source is present.
- [ ] EF Core model snapshot is generated and checked in for future migration diffs.
- [x] CI succeeds on branch head `e5612188634ff88d1912e0e21c88179662df2541` (Restore, Build, Unit Tests, Integration Tests, Frontend; run 46126383863).
- [ ] Initial migration is applied successfully to a provisioned SQL Server database.
- [ ] Production secrets are loaded from the target secret store and verified without exposing values.
- [ ] Production deployment and rollback rehearsal completed.

## Phase 2 — Identity and workspace tenancy

- [x] Phone registration and phone verification endpoints are implemented.
- [x] Password login and short-lived JWT access tokens are implemented.
- [x] Refresh tokens are opaque, stored as SHA-256 hashes, rotated on refresh, and revocable on logout.
- [x] Workspace creation and per-user workspace listing are implemented.
- [x] Workspace member add/list/role-change/remove endpoints are implemented.
- [x] Owner transfer is transactional and the owner cannot be removed or demoted directly.
- [x] Production SMS.ir Verify adapter is implemented with configurable API key and approved template.
- [x] API integration test covers registration, verification, login, refresh rotation, member access, and non-member isolation.
- [x] Integration test passes against the CI test host (run 46126383863).
- [ ] SMS.ir Sandbox delivery is verified using the account's sandbox key and approved template.
- [ ] SMS.ir Production delivery is verified after the customer supplies production credentials.
- [x] Refresh-token replay/family revocation policy is implemented and covered by the API integration flow test.
- [ ] Authorization matrix is reviewed by role, including all workspace-scoped resources.
- [ ] Rate limits are validated behind the actual reverse proxy / IIS deployment.

## External prerequisites

The repository cannot safely contain customer credentials. The following must be supplied
through GitHub Actions secrets, a deployment secret store, or environment variables:

- `ConnectionStrings__DefaultConnection`
- `Jwt__SigningKey` (at least 32 UTF-8 bytes)
- `SmsIr__ApiKey`
- `SmsIr__TemplateId` (positive numeric template ID)
- `SmsIr__CodeParameterName` (must match the approved SMS.ir template placeholder)

## Definition of done

Phases 0–2 are complete only when all unchecked release-gate items above are evidenced.
The initial migration file and SMS.ir adapter being present do not mean that a live SQL
Server database or SMS.ir account has already been provisioned or exercised.
