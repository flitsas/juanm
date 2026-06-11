using Gdc.Infrastructure.Notif;
using Gdc.Modules.Notif.Application.Provider;

namespace Gdc.Api.Endpoints;

public static class NotifProviderEndpoints
{
    public static RouteGroupBuilder MapNotifProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notif/provider").WithTags("NOTIF Provider");

        group.MapGet("/", GetProviderAsync).WithName("GetNotifProvider");
        group.MapPut("/", SaveProviderAsync).WithName("SaveNotifProvider");
        group.MapPost("/test", TestProviderAsync).WithName("TestNotifProvider");

        return group;
    }

    private static async Task<IResult> GetProviderAsync(
        NotifProviderService service,
        CancellationToken cancellationToken)
    {
        var provider = await service.GetActiveProviderAsync(cancellationToken);
        return provider is null
            ? Results.NotFound(new { code = "NOTIF_PROVIDER_NOT_FOUND", message = "No active email provider configured." })
            : Results.Ok(provider);
    }

    private static async Task<IResult> SaveProviderAsync(
        SaveProviderRequest request,
        NotifProviderService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var provider = await service.SaveProviderAsync(request, cancellationToken);
            return Results.Ok(provider);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_PROVIDER_INVALID", message = ex.Message });
        }
    }

    private static async Task<IResult> TestProviderAsync(
        TestProviderRequest request,
        NotifProviderService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.TestProviderAsync(request, cancellationToken);
            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(new { code = "NOTIF_PROVIDER_TEST_FAILED", message = result.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_INVALID_REQUEST", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { code = "NOTIF_PROVIDER_NOT_FOUND", message = ex.Message });
        }
    }
}
