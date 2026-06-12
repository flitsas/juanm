using Gdc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api;

internal static class DevDataSeeder
{
    /// <summary>
    /// IDs alineados con <c>identity.tenants</c> (migración SeedDevTenant) y módulo NOTIF.
    /// </summary>
    private const string DevTenantId = "22222222-2222-2222-2222-222222222222";
    private const string TestTenantId = "11111111-1111-1111-1111-111111111111";

    /// <summary>Tenants legacy de DevDataSeeder previo a la alineación NOTIF.</summary>
    private const string LegacyDevTenantId = "22222222-2222-4222-8222-222222222201";
    private const string LegacyTenantBId = "22222222-2222-4222-8222-222222222202";

    public static async Task SeedIfEmptyAsync(GdcDbContext db, CancellationToken cancellationToken = default)
    {
        await SeedCatalogsAsync(db, cancellationToken);
        await EnsureIdentityTenantsAsync(db, cancellationToken);
        await EnsureCoreTenantsAsync(db, cancellationToken);
        await SyncLegacyTenantIdsAsync(db, cancellationToken);
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

    /// <summary>
    /// FK de módulos DGC/GDC/NOTIF apuntan a <c>identity.tenants</c> (no solo <c>core.tenants</c>).
    /// </summary>
    private static async Task EnsureIdentityTenantsAsync(GdcDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            $"""
            INSERT INTO identity.tenants (id, name, created_at)
            VALUES
              ('{DevTenantId}', 'FLIT Dev Tenant', NOW()),
              ('{TestTenantId}', 'FLIT Test Tenant', NOW())
            ON CONFLICT (id) DO UPDATE
            SET name = EXCLUDED.name;
            """,
            cancellationToken);
    }

    private static async Task EnsureCoreTenantsAsync(GdcDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            $"""
            INSERT INTO core.tenants (id, name, is_active, created_at, updated_at, row_version)
            VALUES
              ('{DevTenantId}', 'FLIT Dev Tenant', true, NOW(), NOW(), '1'::xid),
              ('{TestTenantId}', 'FLIT Test Tenant', true, NOW(), NOW(), '1'::xid)
            ON CONFLICT (id) DO UPDATE
            SET name = EXCLUDED.name,
                is_active = EXCLUDED.is_active,
                updated_at = NOW();
            """,
            cancellationToken);
    }

    /// <summary>
    /// Repara BDs locales creadas con tenant IDs legacy para que la sesión coincida con NOTIF.
    /// </summary>
    private static async Task SyncLegacyTenantIdsAsync(GdcDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            $"""
            UPDATE auth.users
            SET tenant_id = '{DevTenantId}'::uuid,
                updated_at = NOW()
            WHERE tenant_id = '{LegacyDevTenantId}'::uuid;

            UPDATE auth.users
            SET tenant_id = '{TestTenantId}'::uuid,
                updated_at = NOW()
            WHERE tenant_id = '{LegacyTenantBId}'::uuid;

            DELETE FROM core.tenants
            WHERE id IN ('{LegacyDevTenantId}'::uuid, '{LegacyTenantBId}'::uuid)
              AND NOT EXISTS (
                  SELECT 1 FROM auth.users u WHERE u.tenant_id = core.tenants.id
              );
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
            INSERT INTO auth.users (id, tenant_id, email, password_hash, status, created_at, updated_at, row_version)
            VALUES
              ('33333333-3333-4333-8333-333333333301', '{DevTenantId}', 'operator@example.com', '{passwordHash}', 'Active', NOW(), NOW(), '1'::xid),
              ('33333333-3333-4333-8333-333333333303', '{DevTenantId}', 'superadmin@example.com', '{passwordHash}', 'Active', NOW(), NOW(), '1'::xid)
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
