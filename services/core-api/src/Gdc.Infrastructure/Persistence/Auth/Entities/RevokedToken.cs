namespace Gdc.Infrastructure.Persistence.Auth.Entities;

public sealed class RevokedToken : AuditableEntity
{
    public string TokenId { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset RevokedAt { get; set; }
}
