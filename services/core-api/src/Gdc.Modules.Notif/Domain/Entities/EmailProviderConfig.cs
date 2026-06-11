using Gdc.Modules.Notif.Domain.Common;

namespace Gdc.Modules.Notif.Domain.Entities;

public sealed class EmailProviderConfig : TenantAuditableEntity
{
    public required string ProviderType { get; set; }

    public string? CredentialsEncrypted { get; set; }

    public required string FromAddress { get; set; }

    public bool IsActive { get; set; } = true;

    public bool DispatchEnabled { get; set; } = true;
}
