using Gdc.Infrastructure.Plantillas;
using Gdc.Modules.Plantillas.Application.Templates;

namespace Gdc.Api.Endpoints;

public static class GdcDpEndpoints
{
    public static IEndpointRouteBuilder MapGdcDpEndpoints(this IEndpointRouteBuilder app)
    {
        var comparendos = app.MapGroup("/api/v1/gdc/comparendos").WithTags("GDC Derechos de Petición");
        comparendos.MapGet("/{comparendoId:guid}/derechos-peticion", ListByComparendoAsync)
            .WithName("ListGdcDerechosPeticionByComparendo");
        comparendos.MapPost("/{comparendoId:guid}/derechos-peticion", GenerateAsync)
            .WithName("GenerateGdcDerechoPeticion");

        var dps = app.MapGroup("/api/v1/gdc/derechos-peticion").WithTags("GDC Derechos de Petición");
        dps.MapPost("/{id:guid}/estado", TransitionEstadoAsync).WithName("TransitionGdcDerechoPeticionEstado");
        dps.MapGet("/{id:guid}/download", DownloadAsync).WithName("DownloadGdcDerechoPeticionPdf");

        return app;
    }

    private static async Task<IResult> ListByComparendoAsync(
        Guid comparendoId,
        GdcDpService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListByComparendoAsync(comparendoId, cancellationToken));

    private static async Task<IResult> GenerateAsync(
        Guid comparendoId,
        GenerateDerechoPeticionRequest request,
        GdcDpService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.GenerateAsync(comparendoId, request, cancellationToken);
            return response is null
                ? Results.NotFound(new
                {
                    code = "GDC_COMPARENDO_NOT_FOUND",
                    message = $"Comparendo {comparendoId} not found.",
                })
                : Results.Created(
                    $"/api/v1/gdc/comparendos/{comparendoId}/derechos-peticion/{response.DerechoPeticionId}",
                    response);
        }
        catch (InvalidOperationException ex) when (ex.Message == "GDC_CONTRAVENTOR_REQUIRED")
        {
            return Results.BadRequest(new
            {
                code = "GDC_CONTRAVENTOR_REQUIRED",
                message = "Comparendo requires an identified contraventor before generating a Derecho de Petición.",
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "GDC_INVALID_REQUEST", message = ex.Message });
        }
    }

    private static async Task<IResult> TransitionEstadoAsync(
        Guid id,
        TransitionDerechoPeticionEstadoRequest request,
        GdcDpService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.TransitionEstadoAsync(id, request, cancellationToken);
            return response is null
                ? Results.NotFound(new
                {
                    code = "GDC_DP_NOT_FOUND",
                    message = $"Derecho de Petición {id} not found.",
                })
                : Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message == "GDC_DP_INVALID_TRANSITION")
        {
            return Results.BadRequest(new
            {
                code = "GDC_DP_INVALID_TRANSITION",
                message = "The requested estado transition is not allowed for the current DP state.",
            });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "GDC_INVALID_REQUEST", message = ex.Message });
        }
    }

    private static async Task<IResult> DownloadAsync(
        Guid id,
        GdcDpService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.GetDownloadAsync(id, cancellationToken);
            return result is null
                ? Results.NotFound(new
                {
                    code = "GDC_DP_NOT_FOUND",
                    message = $"Derecho de Petición {id} not found.",
                })
                : Results.File(result.Value.Pdf, "application/pdf", result.Value.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { code = "GDC_INVALID_REQUEST", message = ex.Message });
        }
    }
}
