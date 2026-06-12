using FluentValidation;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Reglas.Application.Rules;

namespace Gdc.Api.Endpoints;

public static class ReglasRuleEndpoints
{
    public static RouteGroupBuilder MapReglasRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var rules = app.MapGroup("/api/v1/reglas/rules").WithTags("Reglas");
        rules.MapGet("/", ListRulesAsync).WithName("ListReglasRules");
        rules.MapGet("/{id:guid}", GetRuleAsync).WithName("GetReglasRule");
        rules.MapPost("/", CreateRuleAsync).WithName("CreateReglasRule");
        rules.MapPut("/{id:guid}", UpdateRuleAsync).WithName("UpdateReglasRule");
        rules.MapDelete("/{id:guid}", DeleteRuleAsync).WithName("DeleteReglasRule");
        return rules;
    }

    private static async Task<IResult> ListRulesAsync(
        ReglasRuleService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.ListAsync(cancellationToken));

    private static async Task<IResult> GetRuleAsync(
        Guid id,
        ReglasRuleService service,
        CancellationToken cancellationToken)
    {
        var rule = await service.GetByIdAsync(id, cancellationToken);
        return rule is null
            ? Results.NotFound(new { code = "REGLAS_RULE_NOT_FOUND", message = $"Rule {id} not found." })
            : Results.Ok(rule);
    }

    private static async Task<IResult> CreateRuleAsync(
        CreateReglasRuleRequest request,
        IValidator<CreateReglasRuleRequest> validator,
        ReglasRuleService service,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        try
        {
            var rule = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/reglas/rules/{rule.Id}", rule);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "REGLAS_INVALID_RULE", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateRuleAsync(
        Guid id,
        UpdateReglasRuleRequest request,
        IValidator<UpdateReglasRuleRequest> validator,
        ReglasRuleService service,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        try
        {
            var rule = await service.UpdateAsync(id, request, cancellationToken);
            return rule is null
                ? Results.NotFound(new { code = "REGLAS_RULE_NOT_FOUND", message = $"Rule {id} not found." })
                : Results.Ok(rule);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "REGLAS_INVALID_RULE", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteRuleAsync(
        Guid id,
        ReglasRuleService service,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted
            ? Results.NoContent()
            : Results.NotFound(new { code = "REGLAS_RULE_NOT_FOUND", message = $"Rule {id} not found." });
    }

    private static IResult ValidationProblem(FluentValidation.Results.ValidationResult validation) =>
        Results.BadRequest(new
        {
            code = "REGLAS_VALIDATION_FAILED",
            message = validation.Errors.First().ErrorMessage,
            errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
        });
}
