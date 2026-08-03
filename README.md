# Assignment & Submission Management System

A role-based (Admin / Teacher / Student) full-stack web application for managing
school/college assignments and submissions.

> **Status:** Phase 1 — Project Setup complete. Backend solution, Clean
> Architecture project structure, and PostgreSQL wiring are in place. No
> business features (auth, entities, endpoints) exist yet — those are built in
> the phases that follow.

## 1. Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API, C# |
| Database | PostgreSQL, EF Core (Npgsql provider) |
| Frontend | Next.js, React, TypeScript *(added in Phase 7)* |
| Testing | xUnit *(business-rule tests added in Phase 8)* |

Packages are added only when the phase that needs them is implemented, to
keep the solution free of unused dependencies.

## 2. Project Structure

```
AssignmentManagementSystem/
├── AssignmentManagementSystem.sln
├── NuGet.Config                (dev machines only — see note below)
├── Backend/
│   ├── AssignmentManagement.Domain/          # Entities, enums, domain exceptions. No dependencies.
│   ├── AssignmentManagement.Application/     # Business logic, interfaces (e.g. IApplicationDbContext).
│   ├── AssignmentManagement.Infrastructure/  # EF Core + Npgsql, repositories, identity, external services.
│   ├── AssignmentManagement.API/             # ASP.NET Core Web API host, controllers, DI wiring.
│   └── AssignmentManagement.Tests/           # xUnit test project.
└── Frontend/                                 # Next.js app — added in Phase 7.
```

**Dependency direction (Clean Architecture):**
`API → Infrastructure → Application → Domain`
`API → Application` (directly, for request/response contracts)
Domain has no project references at all; Application only references Domain.

## 3. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) running locally (or via Docker — added in Phase 9)

## 4. Setup Instructions

```bash
# 1. Clone the repository
git clone <repo-url>
cd AssignmentManagementSystem

# 2. Restore & build the backend
cd Backend
dotnet restore
dotnet build
```

### Database setup

1. Create a local PostgreSQL database (any name works, default assumed is `assignment_management`).
2. Update the connection string in `Backend/AssignmentManagement.API/appsettings.Development.json`
   (or override it with the `ConnectionStrings__DefaultConnection` environment variable) to match
   your local PostgreSQL credentials.
3. EF Core migrations will be added starting in Phase 3, once the first entities exist. Running them
   will look like:
   ```bash
   dotnet ef migrations add <MigrationName> \
     --project Backend/AssignmentManagement.Infrastructure \
     --startup-project Backend/AssignmentManagement.API

   dotnet ef database update \
     --project Backend/AssignmentManagement.Infrastructure \
     --startup-project Backend/AssignmentManagement.API
   ```

### Running the API

```bash
cd Backend/AssignmentManagement.API
dotnet run
```

Swagger UI will be available at `/swagger` when running in the `Development` environment.

## 5. Assumptions & Notes

- **Entity IDs** use `Guid` rather than auto-incrementing integers (set in `BaseEntity`), to avoid
  ID-guessing across roles and to make seeding/testing easier.
- **Audit fields** (`CreatedAtUtc`, `UpdatedAtUtc`) live on a shared `BaseEntity` in the Domain layer
  so every entity gets them for free once entities are introduced.
- The Application layer depends only on an `IApplicationDbContext` abstraction, not EF Core directly,
  so business logic stays testable without a real database.
- No business entities, authentication, or API endpoints exist yet by design — this phase is
  infrastructure/setup only, per the project roadmap.
- A repo-root `NuGet.Config` may be added temporarily in restricted network environments during
  development to point restore at a local package source; it is **not** meant to be used by
  evaluators and should be removed/ignored if present — a normal `dotnet restore` against
  nuget.org is all that's required on a machine with internet access.

## 6. Known Limitations

- No database migrations yet (no entities exist to migrate).
- No authentication/authorization yet (Phase 2).
- No frontend yet (Phase 7).

## 7. Roadmap

- [x] Phase 1 — Project Setup (this phase)
- [ ] Phase 2 — Authentication (JWT, roles, authorization)
- [ ] Phase 3 — Admin Module (users, classes, subjects, teacher assignment)
- [ ] Phase 4 — Teacher Module (assignment CRUD, draft/publish)
- [ ] Phase 5 — Student Module (view/submit/update submissions)
- [ ] Phase 6 — Teacher Review (marks, feedback, submission status)
- [ ] Phase 7 — Frontend (React/Next.js)
- [ ] Phase 8 — Testing (unit tests for business rules, auth, workflows)
- [ ] Phase 9 — Deployment (Docker/IIS/Azure)
