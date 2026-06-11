using Gdc.Infrastructure.Notif;
using Gdc.Modules.Notif.Application.TenantAdmin;

namespace Gdc.Api.Endpoints;

public static class NotifCompanyEndpoints
{
    public static RouteGroupBuilder MapNotifCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var companies = app.MapGroup("/api/v1/notif/companies").WithTags("NOTIF Admin");
        companies.MapGet("/", ListCompaniesAsync).WithName("ListNotifCompanies");
        companies.MapGet("/{id:guid}", GetCompanyAsync).WithName("GetNotifCompany");
        companies.MapPost("/", CreateCompanyAsync).WithName("CreateNotifCompany");
        companies.MapPut("/{id:guid}", UpdateCompanyAsync).WithName("UpdateNotifCompany");
        companies.MapDelete("/{id:guid}", DeleteCompanyAsync).WithName("DeleteNotifCompany");

        var profile = app.MapGroup("/api/v1/notif/profile").WithTags("NOTIF Admin");
        profile.MapGet("/", GetProfileAsync).WithName("GetNotifTenantProfile");
        profile.MapPut("/", UpdateProfileAsync).WithName("UpdateNotifTenantProfile");

        return companies;
    }

    private static async Task<IResult> ListCompaniesAsync(
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.ListCompaniesAsync(cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
    }

    private static async Task<IResult> GetCompanyAsync(
        Guid id,
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await service.GetCompanyAsync(id, cancellationToken);
            return company is null
                ? Results.NotFound(new { code = "NOTIF_COMPANY_NOT_FOUND", message = $"Company {id} not found." })
                : Results.Ok(company);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
    }

    private static async Task<IResult> CreateCompanyAsync(
        CreateCompanyRequest request,
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await service.CreateCompanyAsync(request, cancellationToken);
            return Results.Created($"/api/v1/notif/companies/{company.Id}", company);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCompanyAsync(
        Guid id,
        UpdateCompanyRequest request,
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var company = await service.UpdateCompanyAsync(id, request, cancellationToken);
            return company is null
                ? Results.NotFound(new { code = "NOTIF_COMPANY_NOT_FOUND", message = $"Company {id} not found." })
                : Results.Ok(company);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCompanyAsync(
        Guid id,
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await service.DeleteCompanyAsync(id, cancellationToken);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { code = "NOTIF_COMPANY_NOT_FOUND", message = $"Company {id} not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden();
        }
    }

    private static async Task<IResult> GetProfileAsync(
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        var profile = await service.GetProfileAsync(cancellationToken);
        return profile is null
            ? Results.NotFound(new { code = "NOTIF_TENANT_NOT_FOUND", message = "Tenant profile not found." })
            : Results.Ok(profile);
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateTenantProfileRequest request,
        NotifCompanyService service,
        CancellationToken cancellationToken)
    {
        var profile = await service.UpdateProfileAsync(request, cancellationToken);
        return profile is null
            ? Results.NotFound(new { code = "NOTIF_TENANT_NOT_FOUND", message = "Tenant profile not found." })
            : Results.Ok(profile);
    }

    private static IResult Forbidden() =>
        Results.Json(
            new { code = "NOTIF_FORBIDDEN", message = "Super Admin role required." },
            statusCode: StatusCodes.Status403Forbidden);
}
