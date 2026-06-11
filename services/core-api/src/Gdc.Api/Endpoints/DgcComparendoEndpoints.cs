using Gdc.Infrastructure.Dgc;
using Gdc.Modules.Dgc.Application.Contraventor;
using Gdc.Modules.Dgc.Application.Maestra;

namespace Gdc.Api.Endpoints;

public static class DgcComparendoEndpoints
{
    public static RouteGroupBuilder MapDgcComparendoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dgc/comparendos").WithTags("DGC Maestra");

        group.MapGet("/", ListAsync).WithName("ListComparendosMaestra");
        group.MapGet("/{id:guid}", GetByIdAsync).WithName("GetComparendoMaestra");
        group.MapPut("/{id:guid}/contraventor", UpsertContraventorAsync).WithName("UpsertContraventor");

        return group;
    }

    private static async Task<IResult> ListAsync(
        ComparendoMaestraService service,
        string? estado,
        string? placa,
        string? search,
        string? secretaria,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        string? sortBy,
        string? sortDir,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var query = new ComparendoMaestraQuery(
            estado,
            placa,
            search,
            secretaria,
            fechaDesde,
            fechaHasta,
            sortBy ?? "fechaComparendo",
            sortDir ?? "desc",
            page ?? 1,
            pageSize ?? 20);

        var result = await service.ListAsync(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ComparendoMaestraService service,
        CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null
            ? Results.NotFound(new { code = "DGC_COMPARENDO_NOT_FOUND", message = $"Comparendo {id} not found." })
            : Results.Ok(item);
    }

    private static async Task<IResult> UpsertContraventorAsync(
        Guid id,
        UpsertContraventorRequest request,
        ContraventorManualService service,
        CancellationToken cancellationToken)
    {
        var (success, response, errorCode) = await service.UpsertAsync(id, request, cancellationToken);

        return (success, errorCode) switch
        {
            (true, _) => Results.Ok(response),
            (false, "DGC_COMPARENDO_NOT_FOUND") => Results.NotFound(new
            {
                code = errorCode,
                message = $"Comparendo {id} not found.",
            }),
            (false, "DGC_CONTRAVENTOR_INVALID") => Results.BadRequest(new
            {
                code = errorCode,
                message = "Nombre and Documento are required.",
            }),
            _ => Results.Problem("Unexpected contraventor upsert error."),
        };
    }
}
