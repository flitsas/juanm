namespace Gdc.Modules.Plantillas.Application.Templates;

public sealed record GenerateDerechoPeticionRequest(Guid TemplateId);

public sealed record GenerateDerechoPeticionResponse(
    Guid DerechoPeticionId,
    Guid ComparendoId,
    Guid TemplateId,
    int TemplateVersion,
    string Estado,
    string OutputStorageKey,
    DateTimeOffset GeneratedAt);

public sealed record DerechoPeticionListItemDto(
    Guid Id,
    Guid ComparendoId,
    Guid TemplateId,
    int TemplateVersion,
    string Estado,
    DateTimeOffset GeneratedAt,
    string DownloadPath);

public sealed record DerechoPeticionListResponse(IReadOnlyList<DerechoPeticionListItemDto> Items);

public sealed record TransitionDerechoPeticionEstadoRequest(string Estado);

public sealed record TransitionDerechoPeticionEstadoResponse(
    Guid Id,
    string Estado,
    DateTimeOffset? UpdatedAt);

public sealed record RegenerateNoEnviadoDpsResponse(int TemplateVersion, int RegeneratedCount);
