using Gdc.Infrastructure.Plantillas;

namespace Gdc.Api.Endpoints;

public static class GdcPlantillaEndpoints
{
    public static RouteGroupBuilder MapGdcPlantillaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/gdc/templates").WithTags("GDC Plantillas");

        group.MapGet("/", ListAsync).WithName("ListGdcPdfTemplates");
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetGdcPdfTemplate");
        group.MapPost("/upload", UploadAsync).DisableAntiforgery().WithName("UploadGdcPdfTemplate");

        return group;
    }

    private static async Task<IResult> ListAsync(
        GdcPlantillaService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListAsync(cancellationToken));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GdcPlantillaService service,
        CancellationToken cancellationToken)
    {
        var template = await service.GetByIdAsync(id, cancellationToken);
        return template is null
            ? Results.NotFound(new { code = "GDC_TEMPLATE_NOT_FOUND", message = $"Template {id} not found." })
            : Results.Ok(template);
    }

    private static async Task<IResult> UploadAsync(
        HttpRequest request,
        GdcPlantillaService service,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest(new { code = "GDC_INVALID_REQUEST", message = "Multipart form expected." });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { code = "GDC_INVALID_REQUEST", message = "PDF file is required." });
        }

        var name = form["name"].FirstOrDefault();

        try
        {
            await using var stream = file.OpenReadStream();
            var response = await service.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                name,
                cancellationToken);

            return Results.Created($"/api/v1/gdc/templates/{response.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "GDC_INVALID_REQUEST", message = ex.Message });
        }
    }
}
