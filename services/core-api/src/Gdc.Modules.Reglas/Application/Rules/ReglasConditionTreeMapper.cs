using Gdc.Modules.Reglas.Domain.Entities;

namespace Gdc.Modules.Reglas.Application.Rules;

public static class ReglasConditionTreeMapper
{
    public static ConditionNodeDto? BuildTree(IReadOnlyList<RuleCondition> conditions)
    {
        if (conditions.Count == 0)
        {
            return null;
        }

        var lookup = conditions.ToDictionary(c => c.Id);
        var childrenByParent = conditions
            .Where(c => c.ParentId.HasValue)
            .GroupBy(c => c.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.SortOrder).ToList());

        RuleCondition? root = conditions.FirstOrDefault(c => c.ParentId is null);
        return root is null ? null : ToNode(root, childrenByParent);
    }

    public static IReadOnlyList<RuleCondition> Flatten(
        Guid dynamicRuleId,
        Guid tenantId,
        Guid? createdBy,
        DateTimeOffset createdAt,
        ConditionNodeDto? root)
    {
        if (root is null)
        {
            return [];
        }

        var result = new List<RuleCondition>();
        FlattenNode(root, dynamicRuleId, tenantId, createdBy, createdAt, parentId: null, sortOrder: 0, result);
        return result;
    }

    public static int CountPredicates(ConditionNodeDto? root)
    {
        if (root is null)
        {
            return 0;
        }

        if (ReglasNodeTypes.Predicate.Equals(root.NodeType, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return root.Children?.Sum(CountPredicates) ?? 0;
    }

    private static void FlattenNode(
        ConditionNodeDto node,
        Guid dynamicRuleId,
        Guid tenantId,
        Guid? createdBy,
        DateTimeOffset createdAt,
        Guid? parentId,
        int sortOrder,
        IList<RuleCondition> result)
    {
        var entity = new RuleCondition
        {
            Id = Guid.CreateVersion7(),
            DynamicRuleId = dynamicRuleId,
            TenantId = tenantId,
            ParentId = parentId,
            NodeType = node.NodeType.Trim(),
            LogicOperator = NormalizeOptional(node.LogicOperator),
            FieldKey = NormalizeOptional(node.FieldKey),
            ComparisonOperator = NormalizeOptional(node.ComparisonOperator),
            ComparisonValue = NormalizeOptional(node.ComparisonValue),
            SortOrder = sortOrder,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
        };

        result.Add(entity);

        if (node.Children is null)
        {
            return;
        }

        var childOrder = 0;
        foreach (var child in node.Children)
        {
            FlattenNode(child, dynamicRuleId, tenantId, createdBy, createdAt, entity.Id, childOrder++, result);
        }
    }

    private static ConditionNodeDto ToNode(
        RuleCondition condition,
        IReadOnlyDictionary<Guid, List<RuleCondition>> childrenByParent)
    {
        IReadOnlyList<ConditionNodeDto>? children = null;
        if (childrenByParent.TryGetValue(condition.Id, out var childEntities))
        {
            children = childEntities.Select(c => ToNode(c, childrenByParent)).ToList();
        }

        return new ConditionNodeDto(
            condition.NodeType,
            condition.LogicOperator,
            condition.FieldKey,
            condition.ComparisonOperator,
            condition.ComparisonValue,
            children);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
