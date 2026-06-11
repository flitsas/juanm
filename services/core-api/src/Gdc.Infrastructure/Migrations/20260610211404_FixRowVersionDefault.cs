using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionDefault : Migration
    {
        private static readonly (string Schema, string Table)[] AuditableTables =
        [
            ("auth", "activation_tokens"),
            ("auth", "password_reset_tokens"),
            ("auth", "permissions"),
            ("auth", "revoked_tokens"),
            ("auth", "roles"),
            ("auth", "users"),
            ("core", "tenants"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (schema, table) in AuditableTables)
            {
                migrationBuilder.AlterColumn<uint>(
                    name: "row_version",
                    schema: schema,
                    table: table,
                    type: "xid",
                    rowVersion: true,
                    nullable: false,
                    defaultValueSql: "'0'::xid");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (schema, table) in AuditableTables)
            {
                migrationBuilder.AlterColumn<uint>(
                    name: "row_version",
                    schema: schema,
                    table: table,
                    type: "xid",
                    rowVersion: true,
                    nullable: false,
                    oldDefaultValueSql: "'0'::xid");
            }
        }
    }
}
