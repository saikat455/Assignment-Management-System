CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804035938_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FullName" character varying(150) NOT NULL,
        "Email" character varying(200) NOT NULL,
        "PasswordHash" text NOT NULL,
        "Role" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804035938_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804035938_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260804035938_InitialCreate', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    ALTER TABLE "Users" ADD "ClassId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE TABLE "Classes" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Section" character varying(50),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_Classes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE TABLE "Subjects" (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Code" character varying(30) NOT NULL,
        "ClassId" uuid NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_Subjects" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Subjects_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE TABLE "TeacherSubjectAssignments" (
        "Id" uuid NOT NULL,
        "TeacherId" uuid NOT NULL,
        "SubjectId" uuid NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_TeacherSubjectAssignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TeacherSubjectAssignments_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES "Subjects" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_TeacherSubjectAssignments_Users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE INDEX "IX_Users_ClassId" ON "Users" ("ClassId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE UNIQUE INDEX "IX_Subjects_ClassId_Code" ON "Subjects" ("ClassId", "Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE INDEX "IX_TeacherSubjectAssignments_SubjectId" ON "TeacherSubjectAssignments" ("SubjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    CREATE UNIQUE INDEX "IX_TeacherSubjectAssignments_TeacherId_SubjectId" ON "TeacherSubjectAssignments" ("TeacherId", "SubjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804053839_AddAdminModule') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260804053839_AddAdminModule', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804090817_AddAssignments') THEN
    CREATE TABLE "Assignments" (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(4000) NOT NULL,
        "DeadlineUtc" timestamp with time zone NOT NULL,
        "MaxMarks" integer NOT NULL,
        "Status" character varying(20) NOT NULL,
        "SubjectId" uuid NOT NULL,
        "TeacherId" uuid NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_Assignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Assignments_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES "Subjects" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Assignments_Users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804090817_AddAssignments') THEN
    CREATE INDEX "IX_Assignments_SubjectId" ON "Assignments" ("SubjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804090817_AddAssignments') THEN
    CREATE INDEX "IX_Assignments_TeacherId" ON "Assignments" ("TeacherId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260804090817_AddAssignments') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260804090817_AddAssignments', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806044624_AddSubmissions') THEN
    CREATE TABLE "Submissions" (
        "Id" uuid NOT NULL,
        "AssignmentId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "AnswerText" character varying(4000) NOT NULL,
        "SubmittedAtUtc" timestamp with time zone NOT NULL,
        "Status" character varying(20) NOT NULL,
        "MarksObtained" integer,
        "Feedback" character varying(2000),
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "UpdatedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_Submissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Submissions_Assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES "Assignments" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Submissions_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806044624_AddSubmissions') THEN
    CREATE UNIQUE INDEX "IX_Submissions_AssignmentId_StudentId" ON "Submissions" ("AssignmentId", "StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806044624_AddSubmissions') THEN
    CREATE INDEX "IX_Submissions_StudentId" ON "Submissions" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260806044624_AddSubmissions') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260806044624_AddSubmissions', '8.0.4');
    END IF;
END $EF$;
COMMIT;

