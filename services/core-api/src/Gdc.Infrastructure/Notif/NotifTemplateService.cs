using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Templates;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifTemplateService(
    GdcDbContext db,
    ITenantContext tenantContext,
    TimeProvider timeProvider)
{
    public async Task<TemplateListResponse> ListAsync(CancellationToken cancellationToken)
    {
        var items = await db.EmailTemplates
            .Where(t => t.TenantId == tenantContext.TenantId)
            .OrderBy(t => t.Name)
            .Select(t => ToResponse(t))
            .ToListAsync(cancellationToken);

        return new TemplateListResponse(items);
    }

    public async Task<TemplateResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantContext.TenantId, cancellationToken);

        return template is null ? null : ToResponse(template);
    }

    public async Task<TemplateResponse> CreateAsync(
        CreateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        ValidateTemplate(request.Name, request.Subject, request.HtmlBody, request.BannerUrl, request.FooterUrl);

        var now = timeProvider.GetUtcNow();
        var template = new EmailTemplate
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantContext.TenantId,
            Name = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            HtmlBody = request.HtmlBody,
            BannerUrl = NormalizeOptional(request.BannerUrl),
            FooterUrl = NormalizeOptional(request.FooterUrl),
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    public async Task<TemplateResponse?> UpdateAsync(
        Guid id,
        UpdateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        ValidateTemplate(request.Name, request.Subject, request.HtmlBody, request.BannerUrl, request.FooterUrl);

        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantContext.TenantId, cancellationToken);

        if (template is null)
        {
            return null;
        }

        template.Name = request.Name.Trim();
        template.Subject = request.Subject.Trim();
        template.HtmlBody = request.HtmlBody;
        template.BannerUrl = NormalizeOptional(request.BannerUrl);
        template.FooterUrl = NormalizeOptional(request.FooterUrl);
        template.UpdatedAt = timeProvider.GetUtcNow();
        template.UpdatedBy = tenantContext.UserId;

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(template);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantContext.TenantId, cancellationToken);

        if (template is null)
        {
            return false;
        }

        template.DeletedAt = timeProvider.GetUtcNow();
        template.DeletedBy = tenantContext.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PreviewTemplateResponse?> PreviewAsync(
        Guid id,
        PreviewTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantContext.TenantId, cancellationToken);

        if (template is null)
        {
            return null;
        }

        var variables = request.Variables ?? TemplateHtmlComposer.DefaultSampleVariables();
        var subject = TemplateHtmlComposer.MergeVariables(template.Subject, variables);
        var mergedBody = TemplateHtmlComposer.MergeVariables(template.HtmlBody, variables);
        var html = TemplateHtmlComposer.ComposeHtml(mergedBody, template.BannerUrl, template.FooterUrl);

        return new PreviewTemplateResponse(subject, html);
    }

    public UploadAssetResponse SaveAsset(Stream content, string fileName, string contentType)
    {
        TemplateHtmlComposer.ValidateUploadContentType(contentType);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".png" or ".jpg" or ".jpeg"))
        {
            throw new ArgumentException("Only PNG or JPG images are allowed.");
        }

        var assetId = Guid.CreateVersion7();
        var relativeUrl = $"/api/v1/notif/templates/assets/{tenantContext.TenantId:N}/{assetId:N}{extension}";
        NotifTemplateAssetStore.Save(tenantContext.TenantId, assetId, extension, content);

        return new UploadAssetResponse(relativeUrl, contentType);
    }

    private static void ValidateTemplate(
        string name,
        string subject,
        string htmlBody,
        string? bannerUrl,
        string? footerUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name is required.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Template subject is required.");
        }

        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new ArgumentException("Template htmlBody is required.");
        }

        TemplateHtmlComposer.ValidateImageUrl(bannerUrl, nameof(bannerUrl));
        TemplateHtmlComposer.ValidateImageUrl(footerUrl, nameof(footerUrl));
    }

    private static TemplateResponse ToResponse(EmailTemplate template) =>
        new(
            template.Id,
            template.Name,
            template.Subject,
            template.HtmlBody,
            template.BannerUrl,
            template.FooterUrl,
            template.CreatedAt,
            template.UpdatedAt);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
