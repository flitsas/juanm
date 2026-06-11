using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SeedDevTenant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO identity.tenants (id, name, created_at)
            VALUES
                ('22222222-2222-2222-2222-222222222222', 'FLIT Dev Tenant', now()),
                ('11111111-1111-1111-1111-111111111111', 'FLIT Test Tenant', now())
            ON CONFLICT (id) DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM identity.tenants
            WHERE id IN (
                '22222222-2222-2222-2222-222222222222',
                '11111111-1111-1111-1111-111111111111'
            );
            """);
    }
}
