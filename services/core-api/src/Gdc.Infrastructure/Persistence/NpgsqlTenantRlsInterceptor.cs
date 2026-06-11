using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Gdc.Infrastructure.Persistence;

public sealed class NpgsqlTenantRlsInterceptor(ITenantContext tenantContext) : DbConnectionInterceptor
{
    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (connection is not Npgsql.NpgsqlConnection npgsql)
        {
            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
            return;
        }

        var tenantId = tenantContext.BypassTenantFilter
            ? string.Empty
            : tenantContext.TenantId?.ToString() ?? string.Empty;

        await using var command = npgsql.CreateCommand();
        command.CommandText = "SELECT set_config('app.current_tenant_id', @tenantId, false);";
        command.Parameters.AddWithValue("tenantId", tenantId);
        await command.ExecuteNonQueryAsync(cancellationToken);

        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }
}
