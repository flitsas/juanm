using Gdc.Infrastructure.Dgc;

namespace Gdc.Api.Endpoints;

public static class DgcEmailLogEndpoints
{
    public static RouteGroupBuilder MapDgcEmailLogEndpoints(this IEndpointRouteBuilder app)
    {
        var comparendos = app.MapGroup("/api/v1/dgc/comparendos").WithTags("DGC Email Log");
        comparendos.MapGet("/{id:guid}/emails", ListByComparendoAsync).WithName("ListEmailLogsByComparendo");

        var emails = app.MapGroup("/api/v1/dgc/emails").WithTags("DGC Email Log");
        emails.MapGet("/{id:guid}/evidence", GetEvidenceAsync).WithName("GetEmailEvidence");

        return emails;
    }

    private static async Task<IResult> ListByComparendoAsync(
        Guid id,
        EmailLogQueryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ListByComparendoAsync(id, cancellationToken);
        return result is null
            ? Results.NotFound(new { code = "DGC_COMPARENDO_NOT_FOUND", message = $"Comparendo {id} not found." })
            : Results.Ok(result);
    }

    private static async Task<IResult> GetEvidenceAsync(
        Guid id,
        EmailLogQueryService service,
        CancellationToken cancellationToken)
    {
        var evidence = await service.GetEvidenceAsync(id, cancellationToken);
        return evidence is null
            ? Results.NotFound(new { code = "DGC_EMAIL_EVIDENCE_NOT_FOUND", message = $"Email evidence {id} not found." })
            : Results.Ok(evidence);
    }
}
