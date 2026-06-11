using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SeedDgcContraventorJobConfig : Migration
{
    private const string DevTenantId = "22222222-2222-2222-2222-222222222222";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            INSERT INTO dgc.contraventor_job_configs (
                id, cron_expression, is_active, window_order,
                tenant_id, created_at, row_version)
            VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', '0 8 * * *', true, 1,
                 '{DevTenantId}', now(), DEFAULT),
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', '0 12 * * *', true, 2,
                 '{DevTenantId}', now(), DEFAULT),
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', '0 18 * * *', true, 3,
                 '{DevTenantId}', now(), DEFAULT)
            ON CONFLICT (id) DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM dgc.contraventor_job_configs
            WHERE id IN (
                'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01',
                'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02',
                'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03');
            """);
    }
}
