using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gdc.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SeedDgcEmailLogDevFixture : Migration
{
    private const string DevTenantId = "22222222-2222-2222-2222-222222222222";
    private const string DemoComparendoId = "cccccccc-cccc-cccc-cccc-cccccccccc01";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            INSERT INTO dgc.comparendos (
                id, numero_comparendo, estado, infractor_nombre, documento, placa,
                infraccion_codigo, fecha_comparendo, fecha_notificacion, total_valor,
                estado_pago, fuente, pendiente_contraventor, tenant_id, created_at, row_version)
            VALUES (
                '{DemoComparendoId}',
                'SEED-DGC-EMAIL-DEMO',
                'Pendiente',
                'Demo Infractor FLIT',
                '1234567890',
                'ABC123',
                'C29',
                '2026-05-15',
                '2026-05-20',
                450000.00,
                'Pendiente',
                'manual',
                false,
                '{DevTenantId}',
                now(),
                DEFAULT)
            ON CONFLICT (id) DO NOTHING;
            """);

        migrationBuilder.Sql(
            $"""
            INSERT INTO dgc.email_logs (
                id, comparendo_id, sent_at, origen, destino, cc,
                tipo_alerta, estado_entrega, html_evidencia,
                tenant_id, created_at, row_version)
            VALUES
                (
                    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01',
                    '{DemoComparendoId}',
                    '2026-06-01 10:00:00+00',
                    'notificaciones@flit.dev',
                    'contraventor.demo@flit.dev',
                    'supervisor@flit.dev',
                    'Notificación inicial',
                    'Entregado',
                    '<html><body><h1>Comparendo SEED-DGC-EMAIL-DEMO</h1><p>Notificación de infracción de tránsito.</p></body></html>',
                    '{DevTenantId}',
                    now(),
                    DEFAULT),
                (
                    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02',
                    '{DemoComparendoId}',
                    '2026-06-05 14:30:00+00',
                    'notificaciones@flit.dev',
                    'contraventor.demo@flit.dev',
                    NULL,
                    'Recordatorio',
                    'Entregado',
                    '<html><body><p>Recordatorio de pago con descuento.</p></body></html>',
                    '{DevTenantId}',
                    now(),
                    DEFAULT)
            ON CONFLICT (id) DO NOTHING;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM dgc.email_logs
            WHERE id IN (
                'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01',
                'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02');
            """);

        migrationBuilder.Sql(
            """
            DELETE FROM dgc.comparendos
            WHERE id = 'cccccccc-cccc-cccc-cccc-cccccccccc01';
            """);
    }
}
