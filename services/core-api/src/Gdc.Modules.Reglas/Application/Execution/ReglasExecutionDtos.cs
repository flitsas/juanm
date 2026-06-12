namespace Gdc.Modules.Reglas.Application.Execution;

public sealed record ReglasRunResponse(
    Guid Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string Status,
    string TriggerType,
    int EvaluatedCount,
    int MatchedCount,
    int ProcessedCount,
    int FailedCount,
    string? ErrorMessage);

public sealed record ReglasRunListResponse(IReadOnlyList<ReglasRunResponse> Items);

public sealed record ReglasMatchResponse(
    Guid Id,
    Guid DynamicRuleId,
    string RuleName,
    Guid ComparendoId,
    string ComparendoNumero,
    Guid? RuleExecutionRunId,
    string Status,
    DateTimeOffset? ProcessedAt,
    DateTimeOffset CreatedAt);

public sealed record ReglasMatchListResponse(IReadOnlyList<ReglasMatchResponse> Items);

public sealed record TriggerReglasRunResponse(Guid RunId, int MatchedCount);
