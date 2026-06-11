using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DgcDescuentoMatrizRls : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE dgc.descuento_matrices ENABLE ROW LEVEL SECURITY;

            DROP POLICY IF EXISTS tenant_isolation ON dgc.descuento_matrices;

            CREATE POLICY tenant_isolation ON dgc.descuento_matrices
                USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP POLICY IF EXISTS tenant_isolation ON dgc.descuento_matrices;
            ALTER TABLE dgc.descuento_matrices DISABLE ROW LEVEL SECURITY;
            """);
    }
}
