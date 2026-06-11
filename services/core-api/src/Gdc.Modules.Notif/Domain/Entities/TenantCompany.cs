namespace Gdc.Modules.Notif.Domain.Entities;

public sealed class TenantCompany
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Nit { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
