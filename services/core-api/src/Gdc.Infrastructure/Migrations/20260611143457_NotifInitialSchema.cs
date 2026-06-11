using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotifInitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notif");

            migrationBuilder.CreateTable(
                name: "email_provider_configs",
                schema: "notif",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    credentials_encrypted = table.Column<string>(type: "text", nullable: true),
                    from_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    dispatch_enabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_email_provider_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                schema: "notif",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    html_body = table.Column<string>(type: "text", nullable: false),
                    banner_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    footer_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_email_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_rules",
                schema: "notif",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    trigger_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    trigger_days = table.Column<int>(type: "integer", nullable: true),
                    trigger_reference = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    trigger_estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
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
                    table.PrimaryKey("PK_notification_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_rules_email_templates_email_template_id",
                        column: x => x.email_template_id,
                        principalSchema: "notif",
                        principalTable: "email_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_queues",
                schema: "notif",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_email_queues", x => x.id);
                    table.ForeignKey(
                        name: "FK_email_queues_email_templates_email_template_id",
                        column: x => x.email_template_id,
                        principalSchema: "notif",
                        principalTable: "email_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_email_queues_notification_rules_notification_rule_id",
                        column: x => x.notification_rule_id,
                        principalSchema: "notif",
                        principalTable: "notification_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_send_logs",
                schema: "notif",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_queue_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comparendo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    provider_message_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    dgc_email_log_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_email_send_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_email_send_logs_email_queues_email_queue_id",
                        column: x => x.email_queue_id,
                        principalSchema: "notif",
                        principalTable: "email_queues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_email_provider_configs_tenant_active",
                schema: "notif",
                table: "email_provider_configs",
                columns: new[] { "tenant_id", "is_active" },
                unique: true,
                filter: "is_active = true AND deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_email_queues_comparendo_id",
                schema: "notif",
                table: "email_queues",
                column: "comparendo_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_queues_email_template_id",
                schema: "notif",
                table: "email_queues",
                column: "email_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_queues_notification_rule_id",
                schema: "notif",
                table: "email_queues",
                column: "notification_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_queues_tenant_status_scheduled",
                schema: "notif",
                table: "email_queues",
                columns: new[] { "tenant_id", "status", "scheduled_at" });

            migrationBuilder.CreateIndex(
                name: "ix_email_send_logs_comparendo_id",
                schema: "notif",
                table: "email_send_logs",
                column: "comparendo_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_send_logs_email_queue_id",
                schema: "notif",
                table: "email_send_logs",
                column: "email_queue_id");

            migrationBuilder.CreateIndex(
                name: "uq_email_templates_tenant_name",
                schema: "notif",
                table: "email_templates",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_notification_rules_email_template_id",
                schema: "notif",
                table: "notification_rules",
                column: "email_template_id");

            migrationBuilder.Sql(
                """
                ALTER TABLE notif.email_provider_configs
                    ADD CONSTRAINT fk_email_provider_configs_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE notif.email_templates
                    ADD CONSTRAINT fk_email_templates_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE notif.notification_rules
                    ADD CONSTRAINT fk_notification_rules_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE notif.email_queues
                    ADD CONSTRAINT fk_email_queues_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE notif.email_send_logs
                    ADD CONSTRAINT fk_email_send_logs_tenants
                    FOREIGN KEY (tenant_id) REFERENCES identity.tenants(id);

                ALTER TABLE notif.email_provider_configs ENABLE ROW LEVEL SECURITY;
                ALTER TABLE notif.email_templates ENABLE ROW LEVEL SECURITY;
                ALTER TABLE notif.notification_rules ENABLE ROW LEVEL SECURITY;
                ALTER TABLE notif.email_queues ENABLE ROW LEVEL SECURITY;
                ALTER TABLE notif.email_send_logs ENABLE ROW LEVEL SECURITY;

                CREATE POLICY tenant_isolation ON notif.email_provider_configs
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON notif.email_templates
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON notif.notification_rules
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON notif.email_queues
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
                CREATE POLICY tenant_isolation ON notif.email_send_logs
                    USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);

                COMMENT ON COLUMN notif.email_provider_configs.credentials_encrypted IS '@pii:high';
                COMMENT ON COLUMN notif.email_queues.destino IS '@pii:high';
                COMMENT ON COLUMN notif.email_send_logs.destino IS '@pii:high';

                ALTER TABLE notif.email_provider_configs ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE notif.email_templates ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE notif.notification_rules ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE notif.email_queues ALTER COLUMN row_version SET DEFAULT '0'::xid;
                ALTER TABLE notif.email_send_logs ALTER COLUMN row_version SET DEFAULT '0'::xid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP POLICY IF EXISTS tenant_isolation ON notif.email_send_logs;
                DROP POLICY IF EXISTS tenant_isolation ON notif.email_queues;
                DROP POLICY IF EXISTS tenant_isolation ON notif.notification_rules;
                DROP POLICY IF EXISTS tenant_isolation ON notif.email_templates;
                DROP POLICY IF EXISTS tenant_isolation ON notif.email_provider_configs;

                ALTER TABLE notif.email_send_logs DROP CONSTRAINT IF EXISTS fk_email_send_logs_tenants;
                ALTER TABLE notif.email_queues DROP CONSTRAINT IF EXISTS fk_email_queues_tenants;
                ALTER TABLE notif.notification_rules DROP CONSTRAINT IF EXISTS fk_notification_rules_tenants;
                ALTER TABLE notif.email_templates DROP CONSTRAINT IF EXISTS fk_email_templates_tenants;
                ALTER TABLE notif.email_provider_configs DROP CONSTRAINT IF EXISTS fk_email_provider_configs_tenants;
                """);

            migrationBuilder.DropTable(
                name: "email_provider_configs",
                schema: "notif");

            migrationBuilder.DropTable(
                name: "email_send_logs",
                schema: "notif");

            migrationBuilder.DropTable(
                name: "email_queues",
                schema: "notif");

            migrationBuilder.DropTable(
                name: "notification_rules",
                schema: "notif");

            migrationBuilder.DropTable(
                name: "email_templates",
                schema: "notif");
        }
    }
}
