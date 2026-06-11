using Gdc.Infrastructure.Notif;
using Gdc.Modules.Notif.Application.Rules;

namespace Gdc.Api.Endpoints;

public static class NotifRuleEndpoints
{
    public static RouteGroupBuilder MapNotifRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var rules = app.MapGroup("/api/v1/notif/rules").WithTags("NOTIF Rules");
        rules.MapGet("/", ListRulesAsync).WithName("ListNotifRules");
        rules.MapGet("/{id:guid}", GetRuleAsync).WithName("GetNotifRule");
        rules.MapPost("/", CreateRuleAsync).WithName("CreateNotifRule");
        rules.MapPut("/{id:guid}", UpdateRuleAsync).WithName("UpdateNotifRule");
        rules.MapDelete("/{id:guid}", DeleteRuleAsync).WithName("DeleteNotifRule");

        app.MapGet("/api/v1/notif/queue", ListQueueAsync).WithTags("NOTIF Queue").WithName("ListNotifQueue");

        var switchGroup = app.MapGroup("/api/v1/notif/switch").WithTags("NOTIF Switch");
        switchGroup.MapGet("/", GetSwitchAsync).WithName("GetNotifSwitch");
        switchGroup.MapPut("/", UpdateSwitchAsync).WithName("UpdateNotifSwitch");

        return rules;
    }

    private static async Task<IResult> ListRulesAsync(
        NotifRuleService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListAsync(cancellationToken));

    private static async Task<IResult> GetRuleAsync(
        Guid id,
        NotifRuleService service,
        CancellationToken cancellationToken)
    {
        var rule = await service.GetByIdAsync(id, cancellationToken);
        return rule is null
            ? Results.NotFound(new { code = "NOTIF_RULE_NOT_FOUND", message = $"Rule {id} not found." })
            : Results.Ok(rule);
    }

    private static async Task<IResult> CreateRuleAsync(
        CreateRuleRequest request,
        NotifRuleService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/notif/rules/{rule.Id}", rule);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_RULE", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateRuleAsync(
        Guid id,
        UpdateRuleRequest request,
        NotifRuleService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.UpdateAsync(id, request, cancellationToken);
            return rule is null
                ? Results.NotFound(new { code = "NOTIF_RULE_NOT_FOUND", message = $"Rule {id} not found." })
                : Results.Ok(rule);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_RULE", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteRuleAsync(
        Guid id,
        NotifRuleService service,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted
            ? Results.NoContent()
            : Results.NotFound(new { code = "NOTIF_RULE_NOT_FOUND", message = $"Rule {id} not found." });
    }

    private static async Task<IResult> ListQueueAsync(
        NotifQueueService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListAsync(cancellationToken));

    private static async Task<IResult> GetSwitchAsync(
        NotifSwitchService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.GetAsync(cancellationToken));

    private static async Task<IResult> UpdateSwitchAsync(
        UpdateSwitchRequest request,
        NotifSwitchService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.UpdateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_NO_PROVIDER", message = ex.Message });
        }
    }
}
