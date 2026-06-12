using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReglasRbacPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tenants",
                schema: "identity",
                table: "tenants");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "users",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "core",
                table: "tenants",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "roles",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "revoked_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "permissions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "password_reset_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "activation_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "'0'::xid",
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenants1",
                schema: "identity",
                table: "tenants",
                column: "id");

            migrationBuilder.InsertData(
                schema: "auth",
                table: "permissions",
                columns: new[] { "id", "action", "code", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_active", "module", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-8444-444444444401"), "read", "auth.users.read", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Consultar usuarios", true, "auth", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("44444444-4444-4444-8444-444444444402"), "write", "auth.users.write", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Gestionar usuarios", true, "auth", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("44444444-4444-4444-8444-444444444403"), "manage", "auth.rbac.manage", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Administrar matriz RBAC", true, "auth", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("44444444-4444-4444-8444-444444444404"), "read", "reglas.read", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Consultar reglas, logs y contactos secretaría", true, "reglas", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("44444444-4444-4444-8444-444444444405"), "write", "reglas.write", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Gestionar reglas y contactos secretaría", true, "reglas", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null },
                    { new Guid("44444444-4444-4444-8444-444444444406"), "execute", "reglas.execute", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "Ejecutar motor REGLAS manualmente", true, "reglas", new DateTimeOffset(new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null }
                });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-8444-444444444401"), new Guid("11111111-1111-4111-8111-111111111101") },
                    { new Guid("44444444-4444-4444-8444-444444444402"), new Guid("11111111-1111-4111-8111-111111111101") },
                    { new Guid("44444444-4444-4444-8444-444444444403"), new Guid("11111111-1111-4111-8111-111111111101") },
                    { new Guid("44444444-4444-4444-8444-444444444404"), new Guid("11111111-1111-4111-8111-111111111101") },
                    { new Guid("44444444-4444-4444-8444-444444444405"), new Guid("11111111-1111-4111-8111-111111111101") },
                    { new Guid("44444444-4444-4444-8444-444444444406"), new Guid("11111111-1111-4111-8111-111111111101") },
                    { new Guid("44444444-4444-4444-8444-444444444401"), new Guid("11111111-1111-4111-8111-111111111102") },
                    { new Guid("44444444-4444-4444-8444-444444444402"), new Guid("11111111-1111-4111-8111-111111111102") },
                    { new Guid("44444444-4444-4444-8444-444444444404"), new Guid("11111111-1111-4111-8111-111111111102") },
                    { new Guid("44444444-4444-4444-8444-444444444405"), new Guid("11111111-1111-4111-8111-111111111102") },
                    { new Guid("44444444-4444-4444-8444-444444444406"), new Guid("11111111-1111-4111-8111-111111111102") },
                    { new Guid("44444444-4444-4444-8444-444444444401"), new Guid("11111111-1111-4111-8111-111111111103") },
                    { new Guid("44444444-4444-4444-8444-444444444404"), new Guid("11111111-1111-4111-8111-111111111103") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tenants1",
                schema: "identity",
                table: "tenants");

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444401"), new Guid("11111111-1111-4111-8111-111111111101") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444402"), new Guid("11111111-1111-4111-8111-111111111101") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444403"), new Guid("11111111-1111-4111-8111-111111111101") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444404"), new Guid("11111111-1111-4111-8111-111111111101") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444405"), new Guid("11111111-1111-4111-8111-111111111101") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444406"), new Guid("11111111-1111-4111-8111-111111111101") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444401"), new Guid("11111111-1111-4111-8111-111111111102") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444402"), new Guid("11111111-1111-4111-8111-111111111102") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444404"), new Guid("11111111-1111-4111-8111-111111111102") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444405"), new Guid("11111111-1111-4111-8111-111111111102") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444406"), new Guid("11111111-1111-4111-8111-111111111102") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444401"), new Guid("11111111-1111-4111-8111-111111111103") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("44444444-4444-4444-8444-444444444404"), new Guid("11111111-1111-4111-8111-111111111103") });

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444401"));

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444402"));

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444403"));

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444404"));

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444405"));

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-8444-444444444406"));

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "users",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "core",
                table: "tenants",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "roles",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "revoked_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "permissions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "password_reset_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AlterColumn<uint>(
                name: "row_version",
                schema: "auth",
                table: "activation_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "xid",
                oldRowVersion: true,
                oldDefaultValueSql: "'0'::xid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenants",
                schema: "identity",
                table: "tenants",
                column: "id");
        }
    }
}
