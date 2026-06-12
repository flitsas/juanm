using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GdcPlantillasInitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gdc");

            migrationBuilder.CreateTable(
                name: "pdf_templates",
                schema: "gdc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_pdf_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "derechos_peticion",
                schema: "gdc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pdf_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_version = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    output_storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_derechos_peticion", x => x.id);
                    table.ForeignKey(
                        name: "FK_derechos_peticion_pdf_templates_pdf_template_id",
                        column: x => x.pdf_template_id,
                        principalSchema: "gdc",
                        principalTable: "pdf_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pdf_template_fields",
                schema: "gdc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pdf_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acroform_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    field_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    system_variable = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    choice_options_json = table.Column<string>(type: "jsonb", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_pdf_template_fields", x => x.id);
                    table.ForeignKey(
                        name: "FK_pdf_template_fields_pdf_templates_pdf_template_id",
                        column: x => x.pdf_template_id,
                        principalSchema: "gdc",
                        principalTable: "pdf_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_derechos_peticion_comparendo_id",
                schema: "gdc",
                table: "derechos_peticion",
                column: "comparendo_id");

            migrationBuilder.CreateIndex(
                name: "ix_derechos_peticion_pdf_template_id",
                schema: "gdc",
                table: "derechos_peticion",
                column: "pdf_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_derechos_peticion_tenant_comparendo_estado",
                schema: "gdc",
                table: "derechos_peticion",
                columns: new[] { "tenant_id", "comparendo_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix_pdf_template_fields_pdf_template_id",
                schema: "gdc",
                table: "pdf_template_fields",
                column: "pdf_template_id");

            migrationBuilder.CreateIndex(
                name: "uq_pdf_template_fields_template_acroform",
                schema: "gdc",
                table: "pdf_template_fields",
                columns: new[] { "pdf_template_id", "acroform_name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "uq_pdf_templates_tenant_name",
                schema: "gdc",
                table: "pdf_templates",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.Sql(
                """
                ALTER TABLE gdc.pdf_templates
                    ADD CONSTRAINT fk_pdf_templates_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE gdc.pdf_template_fields
                    ADD CONSTRAINT fk_pdf_template_fields_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE gdc.derechos_peticion
                    ADD CONSTRAINT fk_derechos_peticion_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE gdc.derechos_peticion
                    ADD CONSTRAINT fk_derechos_peticion_comparendos
                    FOREIGN KEY (comparendo_id) REFERENCES dgc.compareendos(id);

                ALTER TABLE gdc.pdf_templates ENABLE ROW LEVEL SECURITY;
                ALTER TABLE gdc.pdf_template_fields ENABLE ROW LEVEL SECURITY;
                ALTER TABLE gdc.derechos_peticion ENABLE ROW LEVEL SECURITY;

                CREATE POLICY tenant_isolation ON gdc.pdf_templates
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON gdc.pdf_template_fields
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON gdc.derechos_peticion
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);

                ALTER TABLE gdc.pdf_templates ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE gdc.pdf_template_fields ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE gdc.derechos_peticion ALTER COLUMN row_version SET DEFAULT '0'::xid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pdf_template_fields",
                schema: "gdc");

            migrationBuilder.DropTable(
                name: "derechos_peticion",
                schema: "gdc");

            migrationBuilder.DropTable(
                name: "pdf_templates",
                schema: "gdc");
        }
    }
}
