using Gdc.Api.Auth;
using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gdc.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

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
                var message = error?.Message ?? "Credenciales inválidas.";
                return Results.Json(
                    new { message },
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
                    InvitationErrorCode.EmailDeliveryFailed => Results.Problem(
                        detail: error.Message,
                        statusCode: StatusCodes.Status503ServiceUnavailable,
                        title: "Envío de correo no disponible"),
                    _ => Results.BadRequest(new { message = error.Message }),
                };
            }

            return Results.Created($"/api/v1/auth/users/{success.UserId}", success);
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

        group.MapPost("/password/forgot", async (
            [FromBody] ForgotPasswordRequest request,
            IPasswordRecoveryService passwordRecoveryService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.Accepted();
            }

            await passwordRecoveryService.ForgotPasswordAsync(request, cancellationToken);
            return Results.Accepted();
        })
        .WithName("AuthForgotPassword")
        .AllowAnonymous();

        group.MapPost("/password/reset", async (
            [FromBody] ResetPasswordRequest request,
            IPasswordRecoveryService passwordRecoveryService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Solicitud inválida." });
            }

            var (success, error) = await passwordRecoveryService.ResetPasswordAsync(request, cancellationToken);
            if (success is null || error is not null)
            {
                return error!.Code switch
                {
                    PasswordRecoveryErrorCode.ExpiredToken => Results.Json(
                        new { message = error.Message },
                        statusCode: StatusCodes.Status410Gone),
                    PasswordRecoveryErrorCode.InvalidToken => Results.BadRequest(new { message = error.Message }),
                    PasswordRecoveryErrorCode.InvalidPassword => Results.BadRequest(new { message = error.Message }),
                    PasswordRecoveryErrorCode.UserNotActive => Results.BadRequest(new { message = error.Message }),
                    _ => Results.BadRequest(new { message = error.Message }),
                };
            }

            return Results.Ok(success);
        })
        .WithName("AuthResetPassword")
        .AllowAnonymous();

        group.MapPut("/users/{userId:guid}/password", async (
            Guid userId,
            [FromBody] AdminSetPasswordRequest request,
            IPasswordRecoveryService passwordRecoveryService,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Solicitud inválida." });
            }

            var (success, error) = await passwordRecoveryService.AdminSetPasswordAsync(
                userId,
                request,
                cancellationToken);

            if (success is null || error is not null)
            {
                return error!.Code switch
                {
                    PasswordRecoveryErrorCode.UserNotFound => Results.NotFound(new { message = error.Message }),
                    PasswordRecoveryErrorCode.InvalidPassword => Results.BadRequest(new { message = error.Message }),
                    PasswordRecoveryErrorCode.UserNotActive => Results.BadRequest(new { message = error.Message }),
                    _ => Results.BadRequest(new { message = error.Message }),
                };
            }

            return Results.NoContent();
        })
        .WithName("AuthAdminSetPassword")
        .RequireAuthorization(AuthPolicies.SuperAdmin);

        group.MapGet("/rbac/matrix", async (
            IRbacMatrixService rbacMatrixService,
            CancellationToken cancellationToken) =>
        {
            var matrix = await rbacMatrixService.GetMatrixAsync(cancellationToken);
            return Results.Ok(matrix);
        })
        .WithName("AuthGetRbacMatrix")
        .RequireAuthorization(AuthPolicies.SuperAdmin);

        group.MapPut("/rbac/matrix", async (
            [FromBody] UpdateRbacMatrixRequest request,
            IRbacMatrixService rbacMatrixService,
            CancellationToken cancellationToken) =>
        {
            if (request.Assignments is null || request.Assignments.Count == 0)
            {
                return Results.BadRequest(new { message = "Solicitud inválida." });
            }

            await rbacMatrixService.UpdateMatrixAsync(request, cancellationToken);
            return Results.NoContent();
        })
        .WithName("AuthUpdateRbacMatrix")
        .RequireAuthorization(AuthPolicies.SuperAdmin);

        return app;
    }
}
