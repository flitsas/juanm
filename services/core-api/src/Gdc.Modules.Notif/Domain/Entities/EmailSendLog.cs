using Gdc.Modules.Notif.Domain.Common;

namespace Gdc.Modules.Notif.Domain.Entities;

public sealed class EmailSendLog : TenantAuditableEntity
{
    public Guid? EmailQueueId { get; set; }

    public EmailQueue? EmailQueue { get; set; }

    public Guid ComparendoId { get; set; }

    public required string Destino { get; set; }

    public required string Status { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public string? ProviderMessageId { get; set; }

    public Guid? DgcEmailLogId { get; set; }
}
