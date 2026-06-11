using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DgcInitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dgc");

            migrationBuilder.CreateTable(
                name: "comparendos",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_comparendo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    infractor_nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    documento = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    placa = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    infraccion_codigo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    fecha_comparendo = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_notificacion = table.Column<DateOnly>(type: "date", nullable: true),
                    secretaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_valor = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    estado_pago = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    dp_referencia = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    fuente = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    pendiente_contraventor = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_intento_asociacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_comparendos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contraventor_job_configs",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cron_expression = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    window_order = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_contraventor_job_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ocr_lotes",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_ocr_lotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contraventors",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    documento = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    correo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    asociacion_automatica = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_contraventors", x => x.id);
                    table.ForeignKey(
                        name: "FK_contraventors_comparendos_comparendo_id",
                        column: x => x.comparendo_id,
                        principalSchema: "dgc",
                        principalTable: "comparendos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email_logs",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    origen = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    destino = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    cc = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    tipo_alerta = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    estado_entrega = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    html_evidencia = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_email_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_email_logs_comparendos_comparendo_id",
                        column: x => x.comparendo_id,
                        principalSchema: "dgc",
                        principalTable: "comparendos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ocr_items",
                schema: "dgc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ocr_lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    archivo_uri = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ocr_payload_json = table.Column<string>(type: "jsonb", nullable: true),
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
                    table.PrimaryKey("PK_ocr_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_ocr_items_comparendos_comparendo_id",
                        column: x => x.comparendo_id,
                        principalSchema: "dgc",
                        principalTable: "comparendos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ocr_items_ocr_lotes_ocr_lote_id",
                        column: x => x.ocr_lote_id,
                        principalSchema: "dgc",
                        principalTable: "ocr_lotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uq_comparendos_tenant_numero",
                schema: "dgc",
                table: "comparendos",
                columns: new[] { "tenant_id", "numero_comparendo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_contraventor_job_configs_tenant_window",
                schema: "dgc",
                table: "contraventor_job_configs",
                columns: new[] { "tenant_id", "window_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_contraventors_comparendo_id",
                schema: "dgc",
                table: "contraventors",
                column: "comparendo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_email_logs_comparendo_id",
                schema: "dgc",
                table: "email_logs",
                column: "comparendo_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocr_items_comparendo_id",
                schema: "dgc",
                table: "ocr_items",
                column: "comparendo_id");

            migrationBuilder.CreateIndex(
                name: "ix_ocr_items_ocr_lote_id",
                schema: "dgc",
                table: "ocr_items",
                column: "ocr_lote_id");

            migrationBuilder.Sql(
                """
                CREATE SCHEMA IF NOT EXISTS identity;

                CREATE TABLE IF NOT EXISTS identity.tenants (
                    id uuid PRIMARY KEY,
                    name text NOT NULL,
                    created_at timestamptz NOT NULL DEFAULT now()
                );

                ALTER TABLE dgc.comparendos
                    ADD CONSTRAINT fk_comparendos_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE dgc.contraventors
                    ADD CONSTRAINT fk_contraventors_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE dgc.ocr_lotes
                    ADD CONSTRAINT fk_ocr_lotes_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE dgc.ocr_items
                    ADD CONSTRAINT fk_ocr_items_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE dgc.email_logs
                    ADD CONSTRAINT fk_email_logs_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE dgc.contraventor_job_configs
                    ADD CONSTRAINT fk_contraventor_job_configs_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE dgc.comparendos ENABLE ROW LEVEL SECURITY;
                ALTER TABLE dgc.contraventors ENABLE ROW LEVEL SECURITY;
                ALTER TABLE dgc.ocr_lotes ENABLE ROW LEVEL SECURITY;
                ALTER TABLE dgc.ocr_items ENABLE ROW LEVEL SECURITY;
                ALTER TABLE dgc.email_logs ENABLE ROW LEVEL SECURITY;
                ALTER TABLE dgc.contraventor_job_configs ENABLE ROW LEVEL SECURITY;

                CREATE POLICY tenant_isolation ON dgc.comparendos
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON dgc.contraventors
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON dgc.ocr_lotes
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON dgc.ocr_items
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON dgc.email_logs
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON dgc.contraventor_job_configs
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);

                COMMENT ON COLUMN dgc.comparendos.documento IS '@pii:high';
                COMMENT ON COLUMN dgc.comparendos.infractor_nombre IS '@pii:medium';
                COMMENT ON COLUMN dgc.contraventors.documento IS '@pii:high';
                COMMENT ON COLUMN dgc.contraventors.correo IS '@pii:high';
                COMMENT ON COLUMN dgc.email_logs.html_evidencia IS '@pii:medium';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP POLICY IF EXISTS tenant_isolation ON dgc.contraventor_job_configs;
                DROP POLICY IF EXISTS tenant_isolation ON dgc.email_logs;
                DROP POLICY IF EXISTS tenant_isolation ON dgc.ocr_items;
                DROP POLICY IF EXISTS tenant_isolation ON dgc.ocr_lotes;
                DROP POLICY IF EXISTS tenant_isolation ON dgc.contraventors;
                DROP POLICY IF EXISTS tenant_isolation ON dgc.comparendos;

                ALTER TABLE dgc.contraventor_job_configs DROP CONSTRAINT IF EXISTS fk_contraventor_job_configs_tenants;
                ALTER TABLE dgc.email_logs DROP CONSTRAINT IF EXISTS fk_email_logs_tenants;
                ALTER TABLE dgc.ocr_items DROP CONSTRAINT IF EXISTS fk_ocr_items_tenants;
                ALTER TABLE dgc.ocr_lotes DROP CONSTRAINT IF EXISTS fk_ocr_lotes_tenants;
                ALTER TABLE dgc.contraventors DROP CONSTRAINT IF EXISTS fk_contraventors_tenants;
                ALTER TABLE dgc.comparendos DROP CONSTRAINT IF EXISTS fk_comparendos_tenants;
                """);

            migrationBuilder.DropTable(
                name: "contraventor_job_configs",
                schema: "dgc");

            migrationBuilder.DropTable(
                name: "contraventors",
                schema: "dgc");

            migrationBuilder.DropTable(
                name: "email_logs",
                schema: "dgc");

            migrationBuilder.DropTable(
                name: "ocr_items",
                schema: "dgc");

            migrationBuilder.DropTable(
                name: "comparendos",
                schema: "dgc");

            migrationBuilder.DropTable(
                name: "ocr_lotes",
                schema: "dgc");
        }
    }
}
