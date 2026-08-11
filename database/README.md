# Database Files

- schema.sql - full PostgreSQL schema (all migrations combined), generated via:
  dotnet ef migrations script --idempotent
  Safe to run directly against an empty Postgres database with psql or any SQL client.

- The real source-of-truth migration files are in:
  Backend/AssignmentManagement.Infrastructure/Migrations/

- Seed/demo data is NOT in this script - it's applied automatically by the app itself
  on first startup (see Backend/AssignmentManagement.Infrastructure/Persistence/Seed/DbSeeder.cs).
