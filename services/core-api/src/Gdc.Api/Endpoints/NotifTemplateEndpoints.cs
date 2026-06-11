using Gdc.Infrastructure.Notif;
using Gdc.Modules.Notif.Application.Templates;

namespace Gdc.Api.Endpoints;

public static class NotifTemplateEndpoints
{
    public static RouteGroupBuilder MapNotifTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notif/templates").WithTags("NOTIF Templates");

        group.MapGet("/", ListAsync).WithName("ListNotifTemplates");
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetNotifTemplate");
        group.MapPost("/", CreateAsync).WithName("CreateNotifTemplate");
        group.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateNotifTemplate");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteNotifTemplate");
        group.MapPost("/{id:guid}/preview", PreviewAsync).WithName("PreviewNotifTemplate");
        group.MapPost("/assets", UploadAssetAsync).DisableAntiforgery().WithName("UploadNotifTemplateAsset");
        group.MapGet("/assets/{tenantId:guid}/{assetFileName}", GetAssetAsync).WithName("GetNotifTemplateAsset");

        return group;
    }

    private static async Task<IResult> ListAsync(
        NotifTemplateService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListAsync(cancellationToken));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        NotifTemplateService service,
        CancellationToken cancellationToken)
    {
        var template = await service.GetByIdAsync(id, cancellationToken);
        return template is null
            ? Results.NotFound(new { code = "NOTIF_TEMPLATE_NOT_FOUND", message = $"Template {id} not found." })
            : Results.Ok(template);
    }

    private static async Task<IResult> CreateAsync(
        CreateTemplateRequest request,
        NotifTemplateService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/notif/templates/{template.Id}", template);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTemplateRequest request,
        NotifTemplateService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await service.UpdateAsync(id, request, cancellationToken);
            return template is null
                ? Results.NotFound(new { code = "NOTIF_TEMPLATE_NOT_FOUND", message = $"Template {id} not found." })
                : Results.Ok(template);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        NotifTemplateService service,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted
            ? Results.NoContent()
            : Results.NotFound(new { code = "NOTIF_TEMPLATE_NOT_FOUND", message = $"Template {id} not found." });
    }

    private static async Task<IResult> PreviewAsync(
        Guid id,
        PreviewTemplateRequest request,
        NotifTemplateService service,
        CancellationToken cancellationToken)
    {
        var preview = await service.PreviewAsync(id, request, cancellationToken);
        return preview is null
            ? Results.NotFound(new { code = "NOTIF_TEMPLATE_NOT_FOUND", message = $"Template {id} not found." })
            : Results.Ok(preview);
    }

    private static async Task<IResult> UploadAssetAsync(
        HttpRequest request,
        NotifTemplateService service,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = "multipart/form-data required." });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { code = "NOTIF_NO_FILE", message = "file is required." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = service.SaveAsset(stream, file.FileName, file.ContentType);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_ASSET", message = ex.Message });
        }
    }

    private static IResult GetAssetAsync(Guid tenantId, string assetFileName)
    {
        var extension = Path.GetExtension(assetFileName).ToLowerInvariant();
        if (extension is not (".png" or ".jpg" or ".jpeg"))
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_ASSET", message = "Invalid asset extension." });
        }

        if (!Guid.TryParse(Path.GetFileNameWithoutExtension(assetFileName), out var assetId))
        {
            return Results.NotFound();
        }

        var bytes = NotifTemplateAssetStore.Get(tenantId, assetId, extension);
        if (bytes is null)
        {
            return Results.NotFound();
        }

        var contentType = extension is ".png" ? "image/png" : "image/jpeg";
        return Results.File(bytes, contentType);
    }
}
