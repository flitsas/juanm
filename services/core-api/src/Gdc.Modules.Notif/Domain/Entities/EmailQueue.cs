using Gdc.Modules.Notif.Domain.Common;

namespace Gdc.Modules.Notif.Domain.Entities;

public sealed class EmailQueue : TenantAuditableEntity
{
    public Guid NotificationRuleId { get; set; }

    public NotificationRule? NotificationRule { get; set; }

    public Guid EmailTemplateId { get; set; }

    public EmailTemplate? EmailTemplate { get; set; }

    public Guid ComparendoId { get; set; }

    public required string Destino { get; set; }

    public required string Status { get; set; }

    public DateTimeOffset ScheduledAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public ICollection<EmailSendLog> SendLogs { get; set; } = [];
}
