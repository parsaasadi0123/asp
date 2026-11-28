# Architecture baseline

The solution follows inward dependency direction. Domain contains business entities and invariants; Application contains use cases and persistence abstractions; Infrastructure implements EF Core, SQL Server, Identity persistence, and external adapters; Contracts contains transport contracts; WebAPI is the HTTP composition root; Worker hosts asynchronous workloads as a Windows Service.

Workspace-owned records must carry WorkspaceId. Membership and authorization must be checked in application use cases. EF query filters are defense-in-depth, not a substitute for authorization. Cross-tenant operations require explicit privileged workflows and audit records.

Persist instants in UTC and convert to Asia/Tehran only at presentation and scheduling boundaries. Financial values are integer Toman amounts.
