using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Reglas.Application.Execution;

namespace Gdc.Api.Endpoints;

public static class ReglasExecutionEndpoints
{
    public static RouteGroupBuilder MapReglasExecutionEndpoints(this IEndpointRouteBuilder app)
    {
        var runs = app.MapGroup("/api/v1/reglas/runs").WithTags("Reglas");
        runs.MapGet("/", ListRunsAsync).WithName("ListReglasRuns");
        runs.MapGet("/{id:guid}", GetRunAsync).WithName("GetReglasRun");
        runs.MapPost("/", TriggerRunAsync).WithName("TriggerReglasRun");

        app.MapGet("/api/v1/reglas/matches", ListMatchesAsync)
            .WithTags("Reglas")
            .WithName("ListReglasMatches");

        return runs;
    }

    private static async Task<IResult> ListRunsAsync(
        ReglasExecutionService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListRunsAsync(cancellationToken));

    private static async Task<IResult> GetRunAsync(
        Guid id,
        ReglasExecutionService service,
        CancellationToken cancellationToken)
    {
        var run = await service.GetRunAsync(id, cancellationToken);
        return run is null
            ? Results.NotFound(new { code = "REGLAS_RUN_NOT_FOUND", message = $"Run {id} not found." })
            : Results.Ok(run);
    }

    private static async Task<IResult> TriggerRunAsync(
        ReglasExecutionService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.TriggerManualRunAsync(cancellationToken);
            return Results.Accepted($"/api/v1/reglas/runs/{result.RunId}", result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { code = "REGLAS_RUN_FAILED", message = ex.Message });
        }
    }

    private static async Task<IResult> ListMatchesAsync(
        Guid? runId,
        Guid? ruleId,
        ReglasExecutionService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListMatchesAsync(runId, ruleId, cancellationToken));
}
