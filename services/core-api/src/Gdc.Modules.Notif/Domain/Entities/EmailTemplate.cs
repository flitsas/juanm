using Gdc.Modules.Notif.Domain.Common;

namespace Gdc.Modules.Notif.Domain.Entities;

public sealed class EmailTemplate : TenantAuditableEntity
{
    public required string Name { get; set; }

    public required string Subject { get; set; }

    public required string HtmlBody { get; set; }

    public string? BannerUrl { get; set; }

    public string? FooterUrl { get; set; }

    public ICollection<NotificationRule> Rules { get; set; } = [];
}
