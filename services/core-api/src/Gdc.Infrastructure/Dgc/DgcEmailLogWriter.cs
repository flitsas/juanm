using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Domain.Entities;
using Gdc.Modules.Notif.Application.Abstractions;

namespace Gdc.Infrastructure.Dgc;

public sealed class DgcEmailLogWriter(GdcDbContext db, TimeProvider timeProvider) : IEmailLogWriter
{
    public async Task<Guid> WriteAsync(EmailLogWriteRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.CreateVersion7();
        var now = timeProvider.GetUtcNow();

        var log = new EmailLog
        {
            Id = id,
            TenantId = request.TenantId,
            ComparendoId = request.ComparendoId,
            SentAt = request.SentAt,
            Origen = request.Origen,
            Destino = request.Destino,
            Cc = request.Cc,
            TipoAlerta = request.TipoAlerta,
            EstadoEntrega = request.EstadoEntrega,
            HtmlEvidencia = request.HtmlEvidencia,
            CreatedAt = now,
        };

        db.EmailLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);
        return id;
    }
}
