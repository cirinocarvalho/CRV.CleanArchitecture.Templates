/*
    Adds the IsActive flag to identity users and ensures a "None" company exists.
    Run against the application database (Identity + App share the same database).
    Idempotent: safe to run multiple times.

    Only needed for a database created before IsActive existed: the InitialIdentity
    migration and Identity_CreateObjects.sql both create the column already.

    Existing users default to active (true) so no one is locked out; new registrations
    are created inactive (false) by the application and approved via the Admin page.
*/

BEGIN;

-------------------------------------------------------------------------------
-- AspNetUsers.IsActive
-------------------------------------------------------------------------------
DO $$
BEGIN
    IF to_regclass('identity."AspNetUsers"') IS NOT NULL THEN
        ALTER TABLE identity."AspNetUsers"
            ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT true;
    END IF;
END $$;

-------------------------------------------------------------------------------
-- "None" company (default assignment for new registrations)
-------------------------------------------------------------------------------
DO $$
BEGIN
    IF to_regclass('public."Companies"') IS NOT NULL THEN
        INSERT INTO public."Companies" ("Name")
        SELECT 'None'
        WHERE NOT EXISTS (SELECT 1 FROM public."Companies" WHERE "Name" = 'None');
    END IF;
END $$;

COMMIT;
