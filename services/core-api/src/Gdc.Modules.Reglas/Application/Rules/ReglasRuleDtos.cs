namespace Gdc.Modules.Reglas.Application.Rules;

public sealed record ConditionNodeDto(
    string NodeType,
    string? LogicOperator,
    string? FieldKey,
    string? ComparisonOperator,
    string? ComparisonValue,
    IReadOnlyList<ConditionNodeDto>? Children);

public sealed record ReglasRuleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    Guid PdfTemplateId,
    string EmailSubject,
    string EmailBodyHtml,
    Guid? SecretariatContactId,
    ConditionNodeDto? ConditionRoot,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ReglasRuleListResponse(IReadOnlyList<ReglasRuleResponse> Items);

public sealed record CreateReglasRuleRequest(
    string Name,
    string? Description,
    bool IsActive,
    Guid PdfTemplateId,
    string EmailSubject,
    string EmailBodyHtml,
    Guid? SecretariatContactId,
    ConditionNodeDto? ConditionRoot);

public sealed record UpdateReglasRuleRequest(
    string Name,
    string? Description,
    bool IsActive,
    Guid PdfTemplateId,
    string EmailSubject,
    string EmailBodyHtml,
    Guid? SecretariatContactId,
    ConditionNodeDto? ConditionRoot);
