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
