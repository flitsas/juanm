using FluentValidation;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Reglas.Application.Secretariat;

namespace Gdc.Api.Endpoints;

public static class ReglasSecretariatContactEndpoints
{
    public static RouteGroupBuilder MapReglasSecretariatContactEndpoints(this IEndpointRouteBuilder app)
    {
        var contacts = app.MapGroup("/api/v1/reglas/contacts").WithTags("Reglas");
        contacts.MapGet("/", ListContactsAsync).WithName("ListReglasSecretariatContacts");
        contacts.MapGet("/{id:guid}", GetContactAsync).WithName("GetReglasSecretariatContact");
        contacts.MapPost("/", CreateContactAsync).WithName("CreateReglasSecretariatContact");
        contacts.MapPut("/{id:guid}", UpdateContactAsync).WithName("UpdateReglasSecretariatContact");
        contacts.MapDelete("/{id:guid}", DeleteContactAsync).WithName("DeleteReglasSecretariatContact");
        return contacts;
    }

    private static async Task<IResult> ListContactsAsync(
        ReglasSecretariatContactService service,
        CancellationToken cancellationToken)
    {
        try
        {
            return Results.Ok(await service.ListAsync(cancellationToken));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
    }

    private static async Task<IResult> GetContactAsync(
        Guid id,
        ReglasSecretariatContactService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var contact = await service.GetByIdAsync(id, cancellationToken);
            return contact is null
                ? Results.NotFound(new { code = "REGLAS_CONTACT_NOT_FOUND", message = $"Contact {id} not found." })
                : Results.Ok(contact);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
    }

    private static async Task<IResult> CreateContactAsync(
        CreateReglasSecretariatContactRequest request,
        IValidator<CreateReglasSecretariatContactRequest> validator,
        ReglasSecretariatContactService service,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        try
        {
            var contact = await service.CreateAsync(request, cancellationToken);
            return Results.Created($"/api/v1/reglas/contacts/{contact.Id}", contact);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "REGLAS_INVALID_CONTACT", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateContactAsync(
        Guid id,
        UpdateReglasSecretariatContactRequest request,
        IValidator<UpdateReglasSecretariatContactRequest> validator,
        ReglasSecretariatContactService service,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        try
        {
            var contact = await service.UpdateAsync(id, request, cancellationToken);
            return contact is null
                ? Results.NotFound(new { code = "REGLAS_CONTACT_NOT_FOUND", message = $"Contact {id} not found." })
                : Results.Ok(contact);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "REGLAS_INVALID_CONTACT", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteContactAsync(
        Guid id,
        ReglasSecretariatContactService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await service.DeleteAsync(id, cancellationToken);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { code = "REGLAS_CONTACT_NOT_FOUND", message = $"Contact {id} not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "REGLAS_INVALID_CONTACT", message = ex.Message });
        }
    }

    private static IResult ValidationProblem(FluentValidation.Results.ValidationResult validation) =>
        Results.BadRequest(new
        {
            code = "REGLAS_VALIDATION_FAILED",
            message = validation.Errors.First().ErrorMessage,
            errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
        });

    private static IResult Forbidden() =>
        Results.Json(
            new { code = "REGLAS_FORBIDDEN", message = "Insufficient role for this REGLAS operation." },
            statusCode: StatusCodes.Status403Forbidden);
}
