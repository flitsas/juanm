using Gdc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api;

internal static class DevDataSeeder
{
    public static async Task SeedIfEmptyAsync(GdcDbContext db, CancellationToken cancellationToken = default)
    {
        await SeedCatalogsAsync(db, cancellationToken);
        await SeedUsersAsync(db, cancellationToken);
    }

    private static async Task SeedCatalogsAsync(GdcDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO auth.roles (id, code, name, description, is_active, created_at, updated_at, row_version)
            VALUES
              ('11111111-1111-4111-8111-111111111101', 'SuperAdmin', 'Super Administrador', 'Acceso global de gobierno multi-tenant', true, TIMESTAMPTZ '2026-06-10T00:00:00Z', TIMESTAMPTZ '2026-06-10T00:00:00Z', '1'::xid),
              ('11111111-1111-4111-8111-111111111102', 'TenantAdmin', 'Administrador de tenant', 'Administración de usuarios y permisos del tenant', true, TIMESTAMPTZ '2026-06-10T00:00:00Z', TIMESTAMPTZ '2026-06-10T00:00:00Z', '1'::xid),
              ('11111111-1111-4111-8111-111111111103', 'Operator', 'Operador', 'Usuario operativo con permisos funcionales asignados', true, TIMESTAMPTZ '2026-06-10T00:00:00Z', TIMESTAMPTZ '2026-06-10T00:00:00Z', '1'::xid)
            ON CONFLICT (id) DO NOTHING;
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO auth.permissions (id, code, module, action, description, is_active, created_at, updated_at, row_version)
            VALUES
              ('44444444-4444-4444-8444-444444444401', 'auth.users.read', 'auth', 'read', 'Consultar usuarios', true, TIMESTAMPTZ '2026-06-10T00:00:00Z', TIMESTAMPTZ '2026-06-10T00:00:00Z', '1'::xid),
              ('44444444-4444-4444-8444-444444444402', 'auth.users.write', 'auth', 'write', 'Gestionar usuarios', true, TIMESTAMPTZ '2026-06-10T00:00:00Z', TIMESTAMPTZ '2026-06-10T00:00:00Z', '1'::xid),
              ('44444444-4444-4444-8444-444444444403', 'auth.rbac.manage', 'auth', 'manage', 'Administrar matriz RBAC', true, TIMESTAMPTZ '2026-06-10T00:00:00Z', TIMESTAMPTZ '2026-06-10T00:00:00Z', '1'::xid)
            ON CONFLICT (id) DO NOTHING;
            """,
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO auth.role_permissions (role_id, permission_id)
            VALUES
              ('11111111-1111-4111-8111-111111111101', '44444444-4444-4444-8444-444444444401'),
              ('11111111-1111-4111-8111-111111111101', '44444444-4444-4444-8444-444444444402'),
              ('11111111-1111-4111-8111-111111111101', '44444444-4444-4444-8444-444444444403'),
              ('11111111-1111-4111-8111-111111111102', '44444444-4444-4444-8444-444444444401'),
              ('11111111-1111-4111-8111-111111111102', '44444444-4444-4444-8444-444444444402'),
              ('11111111-1111-4111-8111-111111111103', '44444444-4444-4444-8444-444444444401')
            ON CONFLICT (role_id, permission_id) DO NOTHING;
            """,
            cancellationToken);
    }

    private static async Task SeedUsersAsync(GdcDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Users.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        const string passwordHash =
            "AQAAAAIAAYagAAAAEJmoN2nXFROorX8sLx3q8CCPvP1u+J1RNcx2GeFSkE12qhLQQsA3zHnxfVDhdKxIvA==";

        await db.Database.ExecuteSqlRawAsync(
            $"""
            INSERT INTO core.tenants (id, name, is_active, created_at, updated_at, row_version)
            VALUES
              ('22222222-2222-4222-8222-222222222201', 'Tenant Demo', true, NOW(), NOW(), '1'::xid),
              ('22222222-2222-4222-8222-222222222202', 'Tenant B', true, NOW(), NOW(), '1'::xid)
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO auth.users (id, tenant_id, email, password_hash, status, created_at, updated_at, row_version)
            VALUES
              ('33333333-3333-4333-8333-333333333301', '22222222-2222-4222-8222-222222222201', 'operator@example.com', '{passwordHash}', 'Active', NOW(), NOW(), '1'::xid),
              ('33333333-3333-4333-8333-333333333303', '22222222-2222-4222-8222-222222222201', 'superadmin@example.com', '{passwordHash}', 'Active', NOW(), NOW(), '1'::xid)
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO auth.user_roles (user_id, role_id)
            VALUES
              ('33333333-3333-4333-8333-333333333301', '11111111-1111-4111-8111-111111111103'),
              ('33333333-3333-4333-8333-333333333303', '11111111-1111-4111-8111-111111111101')
            ON CONFLICT (user_id, role_id) DO NOTHING;
            """,
            cancellationToken);
    }
}
