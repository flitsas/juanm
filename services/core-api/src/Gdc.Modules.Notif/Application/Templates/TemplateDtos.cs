namespace Gdc.Modules.Notif.Application.Templates;

public sealed record TemplateResponse(
    Guid Id,
    string Name,
    string Subject,
    string HtmlBody,
    string? BannerUrl,
    string? FooterUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record TemplateListResponse(IReadOnlyList<TemplateResponse> Items);

public sealed record CreateTemplateRequest(
    string Name,
    string Subject,
    string HtmlBody,
    string? BannerUrl,
    string? FooterUrl);

public sealed record UpdateTemplateRequest(
    string Name,
    string Subject,
    string HtmlBody,
    string? BannerUrl,
    string? FooterUrl);

public sealed record PreviewTemplateRequest(
    IReadOnlyDictionary<string, string>? Variables);

public sealed record PreviewTemplateResponse(string Subject, string HtmlBody);

public sealed record UploadAssetResponse(string Url, string ContentType);
