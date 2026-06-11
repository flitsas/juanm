using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Rules;
using Gdc.Modules.Notif.Domain.Entities;

namespace Gdc.Infrastructure.Notif;

public static class NotifRuleEvaluator
{
    public static bool IsEligible(NotificationRule rule, DgcComparendoSnapshot comparendo, DateOnly today)
    {
        if (!rule.IsActive || string.IsNullOrWhiteSpace(comparendo.DestinoEmail))
        {
            return false;
        }

        return rule.TriggerType switch
        {
            RuleTriggerTypes.Chronological => IsChronologicalEligible(rule, comparendo, today),
            RuleTriggerTypes.State => IsStateEligible(rule, comparendo),
            _ => false,
        };
    }

    public static DateTimeOffset ResolveScheduledAt(NotificationRule rule, DgcComparendoSnapshot comparendo)
    {
        if (rule.TriggerType == RuleTriggerTypes.Chronological
            && rule.TriggerDays is >= 0
            && TryGetReferenceDate(rule, comparendo, out var reference))
        {
            var scheduledDate = reference.AddDays(rule.TriggerDays.Value);
            return new DateTimeOffset(scheduledDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        }

        return DateTimeOffset.UtcNow;
    }

    private static bool IsChronologicalEligible(
        NotificationRule rule,
        DgcComparendoSnapshot comparendo,
        DateOnly today)
    {
        if (rule.TriggerDays is null or < 0 || !TryGetReferenceDate(rule, comparendo, out var reference))
        {
            return false;
        }

        var target = reference.AddDays(rule.TriggerDays.Value);
        return today >= target;
    }

    private static bool IsStateEligible(NotificationRule rule, DgcComparendoSnapshot comparendo) =>
        !string.IsNullOrWhiteSpace(rule.TriggerEstado)
        && string.Equals(comparendo.Estado, rule.TriggerEstado, StringComparison.OrdinalIgnoreCase);

    private static bool TryGetReferenceDate(
        NotificationRule rule,
        DgcComparendoSnapshot comparendo,
        out DateOnly reference)
    {
        reference = default;
        if (string.Equals(rule.TriggerReference, TriggerReferences.FechaNotificacion, StringComparison.OrdinalIgnoreCase))
        {
            if (comparendo.FechaNotificacion is not { } fechaNotificacion)
            {
                return false;
            }

            reference = fechaNotificacion;
            return true;
        }

        if (string.Equals(rule.TriggerReference, TriggerReferences.FechaComparendo, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(rule.TriggerReference))
        {
            if (comparendo.FechaComparendo is not { } fechaComparendo)
            {
                return false;
            }

            reference = fechaComparendo;
            return true;
        }

        return false;
    }
}
