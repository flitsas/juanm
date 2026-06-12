using Gdc.Modules.Reglas.Domain.Common;

namespace Gdc.Modules.Reglas.Domain.Entities;

public sealed class RuleCondition : TenantAuditableEntity
{
    public Guid DynamicRuleId { get; set; }

    public DynamicRule? DynamicRule { get; set; }

    public Guid? ParentId { get; set; }

    public RuleCondition? Parent { get; set; }

    public ICollection<RuleCondition> Children { get; set; } = [];

    public required string NodeType { get; set; }

    public string? LogicOperator { get; set; }

    public string? FieldKey { get; set; }

    public string? ComparisonOperator { get; set; }

    public string? ComparisonValue { get; set; }

    public int SortOrder { get; set; }
}
