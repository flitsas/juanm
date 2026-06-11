using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DgcDescuentoMatriz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "descuento_matrices",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    secretaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dias_descuento = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_descuento_matrices", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "uq_descuento_matrices_tenant_secretaria",
                schema: "dgc",
                table: "descuento_matrices",
                columns: new[] { "tenant_id", "secretaria_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "descuento_matrices",
                schema: "dgc");
        }
    }
}
