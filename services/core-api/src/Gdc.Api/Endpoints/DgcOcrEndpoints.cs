using Gdc.Infrastructure.Dgc;
using Gdc.Modules.Dgc.Application.Ocr;

namespace Gdc.Api.Endpoints;

public static class DgcOcrEndpoints
{
    public static RouteGroupBuilder MapDgcOcrEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dgc/ocr").WithTags("DGC OCR");

        group.MapPost("/lotes", UploadLoteAsync)
            .DisableAntiforgery()
            .WithName("UploadOcrLote");

        group.MapGet("/lotes/{loteId:guid}/items", GetLoteItemsAsync)
            .WithName("GetOcrLoteItems");

        group.MapPost("/items/{itemId:guid}/confirm", ConfirmItemAsync)
            .WithName("ConfirmOcrItem");

        return group;
    }

    private static async Task<IResult> UploadLoteAsync(
        HttpRequest request,
        OcrIngestionService service,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest(new { code = "DGC_INVALID_REQUEST", message = "multipart/form-data required." });
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var files = form.Files;
        if (files.Count == 0)
        {
            return Results.BadRequest(new { code = "DGC_NO_FILES", message = "At least one file is required." });
        }

        var uploads = new List<OcrUploadFile>(files.Count);
        foreach (var file in files)
        {
            uploads.Add(new OcrUploadFile(
                file.FileName,
                file.ContentType ?? "application/octet-stream",
                file.OpenReadStream()));
        }

        try
        {
            var result = await service.UploadLoteAsync(uploads, cancellationToken);
            return Results.Created($"/api/v1/dgc/ocr/lotes/{result.LoteId}/items", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { code = "DGC_UNSUPPORTED_FILE", message = ex.Message });
        }
    }

    private static async Task<IResult> GetLoteItemsAsync(
        Guid loteId,
        OcrIngestionService service,
        CancellationToken cancellationToken)
    {
        var items = await service.GetLoteItemsAsync(loteId, cancellationToken);
        return Results.Ok(new { loteId, items });
    }

    private static async Task<IResult> ConfirmItemAsync(
        Guid itemId,
        ConfirmOcrItemRequest request,
        OcrIngestionService service,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroComparendo))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["numeroComparendo"] = ["Numero comparendo is required."],
            });
        }

        try
        {
            var result = await service.ConfirmItemAsync(itemId, request, cancellationToken);
            if (!result.Success)
            {
                return Results.Conflict(new
                {
                    code = result.ErrorCode,
                    message = result.ErrorMessage,
                });
            }

            return Results.Created($"/api/v1/dgc/comparendos/{result.ComparendoId}", result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { code = "DGC_OCR_ITEM_NOT_FOUND", message = $"Item {itemId} not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { code = "DGC_INVALID_STATE", message = ex.Message });
        }
    }
}
