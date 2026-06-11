using Gdc.Modules.Dgc.Domain.Common;

namespace Gdc.Modules.Dgc.Domain.Entities;

public sealed class ContraventorJobConfig : TenantAuditableEntity
{
    public required string CronExpression { get; set; }

    public bool IsActive { get; set; } = true;

    public int WindowOrder { get; set; }
}
