using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotifTenantCompanyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE identity.tenants
                    ADD COLUMN IF NOT EXISTS nit varchar(32),
                    ADD COLUMN IF NOT EXISTS contact_phone varchar(32),
                    ADD COLUMN IF NOT EXISTS contact_email varchar(256),
                    ADD COLUMN IF NOT EXISTS is_active boolean NOT NULL DEFAULT true,
                    ADD COLUMN IF NOT EXISTS updated_at timestamptz,
                    ADD COLUMN IF NOT EXISTS deleted_at timestamptz;

                COMMENT ON COLUMN identity.tenants.contact_phone IS '@pii:medium';
                COMMENT ON COLUMN identity.tenants.contact_email IS '@pii:high';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE identity.tenants
                    DROP COLUMN IF EXISTS deleted_at,
                    DROP COLUMN IF EXISTS updated_at,
                    DROP COLUMN IF EXISTS is_active,
                    DROP COLUMN IF EXISTS contact_email,
                    DROP COLUMN IF EXISTS contact_phone,
                    DROP COLUMN IF EXISTS nit;
                """);
        }
    }
}
