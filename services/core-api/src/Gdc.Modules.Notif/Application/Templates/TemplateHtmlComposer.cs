namespace Gdc.Modules.Notif.Application.Templates;

public static class TemplateHtmlComposer
{
    private static readonly string[] AllowedImageExtensions = [".png", ".jpg", ".jpeg"];

    public static void ValidateImageUrl(string? url, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        if (!HasAllowedImageExtension(url))
        {
            throw new ArgumentException($"{fieldName} must reference a PNG or JPG image.", fieldName);
        }
    }

    public static void ValidateUploadContentType(string? contentType)
    {
        if (contentType is not ("image/png" or "image/jpeg"))
        {
            throw new ArgumentException("Only PNG or JPG images are allowed.");
        }
    }

    public static string MergeVariables(string template, IReadOnlyDictionary<string, string>? variables)
    {
        if (variables is null || variables.Count == 0)
        {
            return template;
        }

        var result = template;
        foreach (var (key, value) in variables)
        {
            result = result.Replace($"{{{{{key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    public static string ComposeHtml(string htmlBody, string? bannerUrl, string? footerUrl)
    {
        var body = htmlBody;
        if (!string.IsNullOrWhiteSpace(bannerUrl))
        {
            body = $"""<div class="notif-banner"><img src="{bannerUrl}" alt="banner" style="max-width:100%;" /></div>{body}""";
        }

        if (!string.IsNullOrWhiteSpace(footerUrl))
        {
            body = $"""{body}<div class="notif-footer"><img src="{footerUrl}" alt="footer" style="max-width:100%;" /></div>""";
        }

        return body;
    }

    public static IReadOnlyDictionary<string, string> DefaultSampleVariables() =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["numero_comparendo"] = "CMP-12345",
            ["placa"] = "ABC123",
            ["infractor"] = "Juan Pérez",
            ["documento"] = "1234567890",
            ["dias_restantes"] = "15",
            ["total"] = "$350.000",
            ["secretaria"] = "Secretaría de Movilidad",
        };

    private static bool HasAllowedImageExtension(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var absolute))
        {
            return IsAllowedExtension(Path.GetExtension(url));
        }

        return IsAllowedExtension(Path.GetExtension(absolute.AbsolutePath));
    }

    private static bool IsAllowedExtension(string extension) =>
        AllowedImageExtensions.Contains(extension.ToLowerInvariant());
}
