using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DgcRowVersionDefault : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE dgc.comparendos ALTER COLUMN row_version SET DEFAULT '0'::xid;
            ALTER TABLE dgc.contraventors ALTER COLUMN row_version SET DEFAULT '0'::xid;
            ALTER TABLE dgc.contraventor_job_configs ALTER COLUMN row_version SET DEFAULT '0'::xid;
            ALTER TABLE dgc.ocr_lotes ALTER COLUMN row_version SET DEFAULT '0'::xid;
            ALTER TABLE dgc.ocr_items ALTER COLUMN row_version SET DEFAULT '0'::xid;
            ALTER TABLE dgc.email_logs ALTER COLUMN row_version SET DEFAULT '0'::xid;
            ALTER TABLE dgc.descuento_matrices ALTER COLUMN row_version SET DEFAULT '0'::xid;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE dgc.comparendos ALTER COLUMN row_version DROP DEFAULT;
            ALTER TABLE dgc.contraventors ALTER COLUMN row_version DROP DEFAULT;
            ALTER TABLE dgc.contraventor_job_configs ALTER COLUMN row_version DROP DEFAULT;
            ALTER TABLE dgc.ocr_lotes ALTER COLUMN row_version DROP DEFAULT;
            ALTER TABLE dgc.ocr_items ALTER COLUMN row_version DROP DEFAULT;
            ALTER TABLE dgc.email_logs ALTER COLUMN row_version DROP DEFAULT;
            ALTER TABLE dgc.descuento_matrices ALTER COLUMN row_version DROP DEFAULT;
            """);
    }
}
