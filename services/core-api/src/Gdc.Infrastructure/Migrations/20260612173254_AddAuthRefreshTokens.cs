using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, defaultValueSql: "'0'::xid")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444404"),
                column: "description",
                value: "Consultar reglas, logs y contactos secretaría");

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444405"),
                column: "description",
                value: "Gestionar reglas y contactos secretaría");

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-4111-8111-111111111102"),
                column: "description",
                value: "Administración de usuarios y permisos del tenant");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "auth",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_refresh_tokens_token_hash",
                schema: "auth",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true,
                filter: "deleted_at IS NULL AND revoked_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "auth");

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444404"),
                column: "description",
                value: "Consultar reglas, logs y contactos secretarÃ­a");

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444405"),
                column: "description",
                value: "Gestionar reglas y contactos secretarÃ­a");

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-4111-8111-111111111102"),
                column: "description",
                value: "AdministraciÃ³n de usuarios y permisos del tenant");
        }
    }
}
