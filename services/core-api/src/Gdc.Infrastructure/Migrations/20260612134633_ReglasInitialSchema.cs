using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReglasInitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reglas");

            migrationBuilder.CreateTable(
                name: "rule_execution_runs",
                schema: "reglas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    trigger_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    evaluated_count = table.Column<int>(type: "integer", nullable: false),
                    matched_count = table.Column<int>(type: "integer", nullable: false),
                    processed_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_execution_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "secretariat_contacts",
                schema: "reglas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    secretariat_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    secretariat_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    contact_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    contact_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secretariat_contacts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dynamic_rules",
                schema: "reglas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    pdf_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email_body_html = table.Column<string>(type: "text", nullable: false),
                    secretariat_contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dynamic_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_dynamic_rules_secretariat_contacts_secretariat_contact_id",
                        column: x => x.secretariat_contact_id,
                        principalSchema: "reglas",
                        principalTable: "secretariat_contacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "rule_conditions",
                schema: "reglas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dynamic_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    node_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    logic_operator = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    field_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    comparison_operator = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    comparison_value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_conditions", x => x.id);
                    table.ForeignKey(
                        name: "FK_rule_conditions_dynamic_rules_dynamic_rule_id",
                        column: x => x.dynamic_rule_id,
                        principalSchema: "reglas",
                        principalTable: "dynamic_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rule_conditions_rule_conditions_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "reglas",
                        principalTable: "rule_conditions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rule_processing_records",
                schema: "reglas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dynamic_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_execution_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    pdf_document_ref = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    email_send_ref = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rule_processing_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_rule_processing_records_dynamic_rules_dynamic_rule_id",
                        column: x => x.dynamic_rule_id,
                        principalSchema: "reglas",
                        principalTable: "dynamic_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rule_processing_records_rule_execution_runs_rule_execution_run_id",
                        column: x => x.rule_execution_run_id,
                        principalSchema: "reglas",
                        principalTable: "rule_execution_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dynamic_rules_secretariat_contact_id",
                schema: "reglas",
                table: "dynamic_rules",
                column: "secretariat_contact_id");

            migrationBuilder.CreateIndex(
                name: "uq_dynamic_rules_tenant_name",
                schema: "reglas",
                table: "dynamic_rules",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_rule_conditions_dynamic_rule_id",
                schema: "reglas",
                table: "rule_conditions",
                column: "dynamic_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_conditions_parent_id",
                schema: "reglas",
                table: "rule_conditions",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_execution_runs_tenant_started",
                schema: "reglas",
                table: "rule_execution_runs",
                columns: new[] { "tenant_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "IX_rule_processing_records_dynamic_rule_id",
                schema: "reglas",
                table: "rule_processing_records",
                column: "dynamic_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_processing_records_comparendo_id",
                schema: "reglas",
                table: "rule_processing_records",
                column: "comparendo_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_processing_records_run_id",
                schema: "reglas",
                table: "rule_processing_records",
                column: "rule_execution_run_id");

            migrationBuilder.CreateIndex(
                name: "uq_rule_processing_records_tenant_rule_comparendo_success",
                schema: "reglas",
                table: "rule_processing_records",
                columns: new[] { "tenant_id", "dynamic_rule_id", "comparendo_id" },
                unique: true,
                filter: "status = 'success' AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "uq_secretariat_contacts_tenant_code",
                schema: "reglas",
                table: "secretariat_contacts",
                columns: new[] { "tenant_id", "secretariat_code" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.Sql(
                """
                ALTER TABLE reglas.rule_execution_runs
                    ADD CONSTRAINT fk_rule_execution_runs_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE reglas.secretariat_contacts
                    ADD CONSTRAINT fk_secretariat_contacts_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE reglas.dynamic_rules
                    ADD CONSTRAINT fk_dynamic_rules_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE reglas.rule_conditions
                    ADD CONSTRAINT fk_rule_conditions_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE reglas.rule_processing_records
                    ADD CONSTRAINT fk_rule_processing_records_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE reglas.dynamic_rules ENABLE ROW LEVEL SECURITY;
                ALTER TABLE reglas.rule_conditions ENABLE ROW LEVEL SECURITY;
                ALTER TABLE reglas.rule_execution_runs ENABLE ROW LEVEL SECURITY;
                ALTER TABLE reglas.rule_processing_records ENABLE ROW LEVEL SECURITY;
                ALTER TABLE reglas.secretariat_contacts ENABLE ROW LEVEL SECURITY;

                CREATE POLICY tenant_isolation ON reglas.dynamic_rules
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON reglas.rule_conditions
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON reglas.rule_execution_runs
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON reglas.rule_processing_records
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON reglas.secretariat_contacts
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);

                COMMENT ON COLUMN reglas.secretariat_contacts.contact_email IS '@pii:high';
                COMMENT ON COLUMN reglas.secretariat_contacts.contact_phone IS '@pii:high';
                COMMENT ON COLUMN reglas.secretariat_contacts.contact_name IS '@pii:medium';

                ALTER TABLE reglas.dynamic_rules ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE reglas.rule_conditions ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE reglas.rule_execution_runs ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE reglas.rule_processing_records ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE reglas.secretariat_contacts ALTER COLUMN row_version SET DEFAULT '0'::xid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP POLICY IF EXISTS tenant_isolation ON reglas.secretariat_contacts;
                DROP POLICY IF EXISTS tenant_isolation ON reglas.rule_processing_records;
                DROP POLICY IF EXISTS tenant_isolation ON reglas.rule_execution_runs;
                DROP POLICY IF EXISTS tenant_isolation ON reglas.rule_conditions;
                DROP POLICY IF EXISTS tenant_isolation ON reglas.dynamic_rules;

                ALTER TABLE reglas.rule_processing_records DROP CONSTRAINT IF EXISTS fk_rule_processing_records_tenants;
                ALTER TABLE reglas.rule_conditions DROP CONSTRAINT IF EXISTS fk_rule_conditions_tenants;
                ALTER TABLE reglas.dynamic_rules DROP CONSTRAINT IF EXISTS fk_dynamic_rules_tenants;
                ALTER TABLE reglas.secretariat_contacts DROP CONSTRAINT IF EXISTS fk_secretariat_contacts_tenants;
                ALTER TABLE reglas.rule_execution_runs DROP CONSTRAINT IF EXISTS fk_rule_execution_runs_tenants;
                """);

            migrationBuilder.DropTable(
                name: "rule_conditions",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "rule_processing_records",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "dynamic_rules",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "rule_execution_runs",
                schema: "reglas");

            migrationBuilder.DropTable(
                name: "secretariat_contacts",
                schema: "reglas");
        }
    }
}
