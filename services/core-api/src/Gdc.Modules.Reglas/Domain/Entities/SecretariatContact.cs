using Gdc.Modules.Reglas.Domain.Common;

namespace Gdc.Modules.Reglas.Domain.Entities;

public sealed class SecretariatContact : TenantAuditableEntity
{
    public required string SecretariatCode { get; set; }

    public required string SecretariatName { get; set; }

    public required string ContactName { get; set; }

    public required string ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<DynamicRule> DynamicRules { get; set; } = [];
}
