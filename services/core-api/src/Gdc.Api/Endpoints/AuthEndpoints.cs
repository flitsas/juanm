using Gdc.Api.Auth;
using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IAuthSessionService authSessionService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.Json(
                    new { message = "Credenciales inválidas." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var (success, error) = await authSessionService.LoginAsync(request, cancellationToken);

            if (success is null || error is not null)
            {
                return Results.Json(
                    new { message = error?.Message ?? "Credenciales inválidas." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Ok(success);
        })
        .WithName("AuthLogin")
        .AllowAnonymous();

        group.MapPost("/logout", async (
            HttpContext httpContext,
            IAuthSessionService authSessionService,
            CancellationToken cancellationToken) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            await authSessionService.LogoutAsync(httpContext.User, cancellationToken);
            return Results.NoContent();
        })
        .WithName("AuthLogout")
        .RequireAuthorization();

        group.MapGet("/users", async (
            IUserReadService userReadService,
            CancellationToken cancellationToken) =>
        {
            var users = await userReadService.ListCurrentTenantUsersAsync(cancellationToken);
            return Results.Ok(users);
        })
        .WithName("AuthListTenantUsers")
        .RequireAuthorization();

        group.MapGet("/users/{userId:guid}", async (
            Guid userId,
            IUserReadService userReadService,
            CancellationToken cancellationToken) =>
        {
            var user = await userReadService.GetUserByIdAsync(userId, cancellationToken);
            return user is null ? Results.NotFound() : Results.Ok(user);
        })
        .WithName("AuthGetTenantUser")
        .RequireAuthorization();

        group.MapGet("/admin/users", async (
            IUserReadService userReadService,
            CancellationToken cancellationToken) =>
        {
            var users = await userReadService.ListAllUsersForSuperAdminAsync(cancellationToken);
            return Results.Ok(users);
        })
        .WithName("AuthAdminListAllUsers")
        .RequireAuthorization(AuthPolicies.SuperAdmin);

        return app;
    }
}
