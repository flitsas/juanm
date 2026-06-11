using Gdc.Modules.Notif.Domain.Common;

namespace Gdc.Modules.Notif.Domain.Entities;

public sealed class NotificationRule : TenantAuditableEntity
{
    public Guid EmailTemplateId { get; set; }

    public EmailTemplate? EmailTemplate { get; set; }

    public required string Name { get; set; }

    public required string TriggerType { get; set; }

    public int? TriggerDays { get; set; }

    public string? TriggerReference { get; set; }

    public string? TriggerEstado { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EmailQueue> QueueItems { get; set; } = [];
}
