using Gdc.Infrastructure.Plantillas;
using Gdc.Modules.Plantillas.Application.Templates;

namespace Gdc.Api.Endpoints;

public static class GdcDpEndpoints
{
    public static RouteGroupBuilder MapGdcDpEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/gdc/comparendos").WithTags("GDC Derechos de Petición");

        group.MapPost("/{comparendoId:guid}/derechos-peticion", GenerateAsync)
            .WithName("GenerateGdcDerechoPeticion");

        return group;
    }

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
}
