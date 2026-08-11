# Assignment & Submission Management System

## 1. Project Overview

A role-based (Admin / Teacher / Student) full-stack web application for a school or college to
manage assignments and submissions. Teachers create and publish assignments for a class/subject,
students submit and (within rules) update their answers, and teachers grade and give feedback.
Admins manage users, classes, subjects, and which teacher teaches which subject.

Built as a 9-phase Clean Architecture project: backend setup → authentication → admin module →
teacher module → student module → teacher review → frontend → deployment → testing.

## 2. Main Features

- **Admin:** manage users (create/edit/deactivate), classes, subjects, and teacher-to-subject
  assignments; read-only view of every assignment across the school.
- **Teacher:** create/edit/delete assignments for their own subjects, save as draft or publish,
  view submissions, assign marks and feedback, and return a submission for changes.
- **Student:** view published assignments for their own class, submit an answer, update it before
  the deadline (or after, if a teacher explicitly returned it for changes), and see their marks
  and feedback once graded.
- JWT authentication with role-based authorization enforced on every endpoint.
- Ownership checks throughout (a Teacher can't touch another Teacher's assignment; a Student only
  ever sees their own submissions and their own class's assignments).

## 3. Technology Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API, C#, EF Core (Npgsql provider) |
| Database | PostgreSQL |
| Frontend | Next.js 16 (App Router), React, TypeScript, Tailwind CSS |
| Auth | JWT bearer tokens, role-based `[Authorize]`, BCrypt password hashing |
| Testing | xUnit, Moq, EF Core InMemory provider |
| Deployment | Docker + Docker Compose (Postgres + API + frontend); also deployable to Render (API) + Vercel (frontend) + Neon (Postgres) |

Packages were added only when the phase that needed them was implemented, to keep the solution
free of unused dependencies.

## 4. Project Structure

```
AssignmentManagementSystem/
├── AssignmentManagementSystem.sln
├── docker-compose.yml
├── .env.example                                # copy to .env - see Database/Deployment setup
├── database/
│   ├── schema.sql                              # full schema, generated from migrations (see §6)
│   └── README.md
├── Backend/
│   ├── AssignmentManagement.Domain/            # Entities, enums. No dependencies.
│   ├── AssignmentManagement.Application/       # Business logic, DTOs, interfaces.
│   ├── AssignmentManagement.Infrastructure/    # EF Core + Npgsql, identity, JWT.
│   │   ├── Migrations/                         # EF Core migration files (source of truth)
│   │   └── Persistence/Seed/DbSeeder.cs         # auto-seeds demo accounts + sample data
│   ├── AssignmentManagement.API/               # ASP.NET Core Web API host, controllers.
│   │   └── Dockerfile
│   └── AssignmentManagement.Tests/             # xUnit tests (unit, authorization, workflow).
└── Frontend/
    └── assignment-management-ui/               # Next.js app
        └── Dockerfile
```

**Dependency direction (Clean Architecture):** `API → Infrastructure → Application → Domain`
(API also references Application directly for request/response contracts). Domain has zero
project references; Application references only Domain.

## 5. Setup Instructions

### Prerequisites

**Option A — Docker (recommended, no local .NET/Node/Postgres install needed):**
- [Docker](https://docs.docker.com/get-docker/) and Docker Compose

**Option B — running everything locally:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 14+](https://www.postgresql.org/download/)

### Clone

```bash
git clone <repo-url>
cd AssignmentManagementSystem
```

## 6. Database Setup Instructions

The evaluator does **not** need to manually create any tables — pick whichever of these fits how
you're running the project:

### If using Docker (section 7 below)
Nothing to do manually. The API container applies all EF Core migrations and seeds demo data
automatically on first boot.

### If running the backend locally (`dotnet run`)
1. Create an empty PostgreSQL database (any name; `assignment_management` is the default used
   throughout this README).
2. Copy `.env.example` (repo root) to `.env` and fill in your real Postgres connection details
   (section A of that file).
3. Apply migrations — this creates every table, index, and constraint:
```bash
   cd Backend
   dotnet ef database update \
     --project AssignmentManagement.Infrastructure \
     --startup-project AssignmentManagement.API
```
4. Run the API once (`dotnet run` — see §8 below); it seeds demo Admin/Teacher/Student accounts
   plus a sample class, subject, teacher assignment, and a couple of sample assignments/submissions
   automatically the first time it starts against an empty `Users` table.

### Alternative — plain SQL script (no `dotnet ef` tooling required)
`database/schema.sql` contains the complete schema (all migrations combined, `--idempotent`, so
it's safe to run even if some migrations are already applied). Run it directly against an empty
Postgres database with `psql` or any SQL client if you'd rather not install the EF CLI tools.
Note this only creates the schema — seed data still comes from the app's own startup seeder, not
from this script.

### Where the actual migration files live
`Backend/AssignmentManagement.Infrastructure/Migrations/` — these `.cs` files are the real
source of truth EF Core uses; `database/schema.sql` is a generated, read-only convenience copy
of the same schema.

## 7. Deployment (Docker Compose)

The easiest way to run the whole stack (Postgres + API + frontend) with one command:

```bash
cp .env.example .env
# edit .env: set POSTGRES_PASSWORD and JWT_KEY at minimum (section B of the file)
docker compose up --build
```
- Frontend: `http://localhost:3000`
- API/Swagger: `http://localhost:5011/swagger`

Stop with `docker compose down` (add `-v` to also wipe the Postgres data volume).

**Notes:**
- The API container runs on plain HTTP; put a reverse proxy in front for TLS in a real deployment.
- `NEXT_PUBLIC_API_BASE_URL` is baked into the frontend bundle at **build** time, so changing
  `API_URL` in `.env` needs `docker compose up --build` again, not just a restart.
- This project was also deployed live using Neon (Postgres) + Render (API, from the same
  Dockerfile) + Vercel (frontend) — see the live links below if provided separately.

## 8. Running the Backend (without Docker)

```bash
cd Backend
dotnet restore
# (see section 6 for database setup first)
cd AssignmentManagement.API
dotnet run
```
Swagger UI: `/swagger` (Development environment). Watch the console output for the actual
port/protocol it binds to.

## 9. Running the Frontend (without Docker)

```bash
cd Frontend/assignment-management-ui
cp .env.example .env.local   # set NEXT_PUBLIC_API_BASE_URL to match the API's real URL
npm install
npm run dev
```
Visit `http://localhost:3000`.

## 10. Running the Tests

```bash
cd Backend
dotnet test
```

Covers the three required areas:

- **Unit tests** — password hashing (BCrypt round-trip, salting), JWT generation (role claim,
  expiry).
- **Authorization tests** — a Teacher can't view/edit/grade another Teacher's assignments or
  submissions (`ForbiddenException`); a Teacher can't create an assignment for a subject they
  aren't assigned to; an Admin can't deactivate their own account; duplicate-email checks on
  register/update.
- **Workflow tests** — the full submission lifecycle: submit before/after the deadline
  (`Submitted` vs `Late`), blocking a second submission, blocking edits after the deadline,
  blocking edits to a graded submission, and the `Returned` → edit → back to `Submitted`
  resubmit flow; assignment draft/publish/unpublish transitions and the block on deleting an
  assignment that already has submissions; grading rules (marks capped at `MaxMarks`, clearing
  marks/feedback when a graded submission is returned for changes).

Tests use EF Core's InMemory provider (a real `ApplicationDbContext`, no database required) and
Moq for the few dependencies that need controlled behavior, so `dotnet test` runs standalone with
no setup.

## 11. Demo Credentials

Seeded automatically on first run (Docker or local):

| Role | Email | Password |
|---|---|---|
| Admin | admin@school.test | Admin@123 |
| Teacher | teacher@school.test | Teacher@123 |
| Student | student@school.test | Student@123 |

A demo class ("Class 10 - A"), subject ("Mathematics"), teacher assignment, and a couple of
sample assignments/submissions are seeded alongside the accounts.

## 12. Assumptions & Notes

- **Entity IDs** use `Guid` rather than auto-incrementing integers, to avoid ID-guessing across
  roles and to make seeding/testing easier.
- **No public self-registration** — only an authenticated Admin can create accounts
  (`POST /api/auth/register`), matching the brief's "Admin manages users" responsibility. A
  seeded Admin account bootstraps the very first login.
- **Submissions are text-only** (no file upload), to keep scope reasonable for the project's
  time-box.
- **Late submissions are allowed** but marked `Late` rather than rejected outright; further edits
  after the deadline are blocked unless the teacher explicitly `Returns` the submission for
  changes, which reopens editing regardless of the original deadline.
- **Soft delete for users:** deactivating a user (`DELETE /api/admin/users/{id}`) sets
  `IsActive = false` rather than removing the row, to preserve historical assignments/submissions
  tied to that user.
- **JWT stored in `localStorage`** on the frontend rather than an httpOnly cookie — a reasonable
  trade-off for this project's scope, but a real production system would prefer cookies +
  refresh tokens to reduce XSS exposure.
- Local dev secrets live in a repo-root `.env` file (loaded via a small `DotNetEnv` call in
  `Program.cs`, since ASP.NET Core doesn't read `.env` files natively) rather than
  `appsettings.Development.json`, so nothing sensitive is ever committed.

## 13. Known Limitations

- No pagination or advanced filtering (all lists load in full — acceptable at this project's
  scale, called out as an optional enhancement in the brief).
- No notifications (e.g. email on new assignment/grade).
- The Teacher submissions view doesn't show students who *haven't* submitted yet (only actual
  submissions), since that would require cross-referencing the full class roster.
- No automated CI pipeline is configured; tests are run manually via `dotnet test`.
- `database/schema.sql` covers schema only, not seed data — seeding is application-driven
  (`DbSeeder.cs`) so it stays in sync with the current model automatically; a static SQL seed
  file would risk drifting out of date as the schema evolves.

## 14. Roadmap

- [x] Phase 1 — Project Setup
- [x] Phase 2 — Authentication (JWT, roles, authorization)
- [x] Phase 3 — Admin Module (users, classes, subjects, teacher assignment)
- [x] Phase 4 — Teacher Module (assignment CRUD, draft/publish)
- [x] Phase 5 — Student Module (view/submit/update submissions)
- [x] Phase 6 — Teacher Review (marks, feedback, submission status)
- [x] Phase 7 — Frontend (Next.js/React/TypeScript)
- [x] Phase 8 — Testing (unit, authorization, and workflow tests)
- [x] Phase 9 — Deployment (Docker Compose; also deployed live via Render/Vercel/Neon)
