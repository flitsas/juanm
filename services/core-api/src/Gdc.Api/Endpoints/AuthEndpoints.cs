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

        group.MapPost("/users/invite", async (
            [FromBody] InviteUserRequest request,
            IUserInvitationService invitationService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || request.TenantId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "Solicitud inválida." });
            }

            var (success, error) = await invitationService.InviteAsync(request, cancellationToken);
            if (success is null || error is not null)
            {
                return error!.Code switch
                {
                    InvitationErrorCode.TenantNotFound => Results.NotFound(new { message = error.Message }),
                    InvitationErrorCode.DuplicateEmail => Results.Conflict(new { message = error.Message }),
                    InvitationErrorCode.InvalidRole => Results.BadRequest(new { message = error.Message }),
                    _ => Results.BadRequest(new { message = error.Message }),
                };
            }

            return Results.Created($"/auth/users/{success.UserId}", success);
        })
        .WithName("AuthInviteUser")
        .RequireAuthorization(AuthPolicies.SuperAdmin);

        group.MapPost("/users/activate", async (
            [FromBody] ActivateUserRequest request,
            IUserInvitationService invitationService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Solicitud inválida." });
            }

            var (success, error) = await invitationService.ActivateAsync(request, cancellationToken);
            if (success is null || error is not null)
            {
                return error!.Code switch
                {
                    InvitationErrorCode.ExpiredToken => Results.Json(
                        new { message = error.Message },
                        statusCode: StatusCodes.Status410Gone),
                    InvitationErrorCode.InvalidToken => Results.BadRequest(new { message = error.Message }),
                    InvitationErrorCode.InvalidPassword => Results.BadRequest(new { message = error.Message }),
                    InvitationErrorCode.UserNotPending => Results.BadRequest(new { message = error.Message }),
                    _ => Results.BadRequest(new { message = error.Message }),
                };
            }

            return Results.Ok(success);
        })
        .WithName("AuthActivateUser")
        .AllowAnonymous();

        return app;
    }
}
