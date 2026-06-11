namespace Gdc.Modules.Notif.Application.Rules;

public sealed record RuleResponse(
    Guid Id,
    Guid EmailTemplateId,
    string Name,
    string TriggerType,
    int? TriggerDays,
    string? TriggerReference,
    string? TriggerEstado,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record RuleListResponse(IReadOnlyList<RuleResponse> Items);

public sealed record CreateRuleRequest(
    Guid EmailTemplateId,
    string Name,
    string TriggerType,
    int? TriggerDays,
    string? TriggerReference,
    string? TriggerEstado,
    bool IsActive = true);

public sealed record UpdateRuleRequest(
    Guid EmailTemplateId,
    string Name,
    string TriggerType,
    int? TriggerDays,
    string? TriggerReference,
    string? TriggerEstado,
    bool IsActive);

public sealed record QueueItemResponse(
    Guid Id,
    Guid NotificationRuleId,
    Guid EmailTemplateId,
    Guid ComparendoId,
    string Destino,
    string Status,
    DateTimeOffset ScheduledAt,
    DateTimeOffset? ProcessedAt,
    string? ErrorMessage);

public sealed record QueueListResponse(IReadOnlyList<QueueItemResponse> Items);

public sealed record SwitchResponse(bool DispatchEnabled);

public sealed record UpdateSwitchRequest(bool DispatchEnabled);
