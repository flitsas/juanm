using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Rules;

namespace Gdc.Modules.Reglas.Application.Evaluation;

public static class ReglasRuleEvaluator
{
    public static bool Matches(ConditionNodeDto? root, DgcComparendoSnapshot comparendo)
    {
        if (root is null)
        {
            return false;
        }

        return EvaluateNode(root, comparendo);
    }

    private static bool EvaluateNode(ConditionNodeDto node, DgcComparendoSnapshot comparendo)
    {
        if (ReglasNodeTypes.Predicate.Equals(node.NodeType, StringComparison.OrdinalIgnoreCase))
        {
            return EvaluatePredicate(node, comparendo);
        }

        if (node.Children is not { Count: > 0 })
        {
            return false;
        }

        var isAnd = ReglasLogicOperators.And.Equals(node.LogicOperator, StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrWhiteSpace(node.LogicOperator);

        return isAnd
            ? node.Children.All(child => EvaluateNode(child, comparendo))
            : node.Children.Any(child => EvaluateNode(child, comparendo));
    }

    private static bool EvaluatePredicate(ConditionNodeDto node, DgcComparendoSnapshot comparendo)
    {
        if (string.IsNullOrWhiteSpace(node.FieldKey)
            || string.IsNullOrWhiteSpace(node.ComparisonOperator)
            || node.ComparisonValue is null)
        {
            return false;
        }

        var actual = ResolveFieldValue(node.FieldKey, comparendo);
        if (actual is null)
        {
            return false;
        }

        var expected = node.ComparisonValue.Trim();
        var op = node.ComparisonOperator.Trim();

        if (ReglasComparisonOperators.Equal.Equals(op, StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        if (ReglasComparisonOperators.NotEqual.Equals(op, StringComparison.OrdinalIgnoreCase))
        {
            return !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        if (ReglasComparisonOperators.Contains.Equals(op, StringComparison.OrdinalIgnoreCase))
        {
            return actual.Contains(expected, StringComparison.OrdinalIgnoreCase);
        }

        if (ReglasComparisonOperators.NotContains.Equals(op, StringComparison.OrdinalIgnoreCase))
        {
            return !actual.Contains(expected, StringComparison.OrdinalIgnoreCase);
        }

        if (ReglasComparisonOperators.GreaterThan.Equals(op, StringComparison.OrdinalIgnoreCase)
            || ReglasComparisonOperators.LessThan.Equals(op, StringComparison.OrdinalIgnoreCase))
        {
            if (DateOnly.TryParse(actual, out var actualDate) && DateOnly.TryParse(expected, out var expectedDate))
            {
                return ReglasComparisonOperators.GreaterThan.Equals(op, StringComparison.OrdinalIgnoreCase)
                    ? actualDate > expectedDate
                    : actualDate < expectedDate;
            }

            if (decimal.TryParse(actual, out var actualNumber) && decimal.TryParse(expected, out var expectedNumber))
            {
                return ReglasComparisonOperators.GreaterThan.Equals(op, StringComparison.OrdinalIgnoreCase)
                    ? actualNumber > expectedNumber
                    : actualNumber < expectedNumber;
            }

            return false;
        }

        return false;
    }

    private static string? ResolveFieldValue(string fieldKey, DgcComparendoSnapshot comparendo) =>
        fieldKey.Trim().ToLowerInvariant() switch
        {
            ReglasFieldKeys.Estado => comparendo.Estado,
            ReglasFieldKeys.NumeroComparendo => comparendo.NumeroComparendo,
            ReglasFieldKeys.Placa => comparendo.Placa,
            ReglasFieldKeys.InfractorNombre => comparendo.InfractorNombre,
            ReglasFieldKeys.FechaComparendo => comparendo.FechaComparendo?.ToString("yyyy-MM-dd"),
            ReglasFieldKeys.FechaNotificacion => comparendo.FechaNotificacion?.ToString("yyyy-MM-dd"),
            ReglasFieldKeys.DestinoEmail => comparendo.DestinoEmail,
            _ => null,
        };
}
