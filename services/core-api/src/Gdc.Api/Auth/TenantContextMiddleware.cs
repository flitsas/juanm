using System.Security.Claims;
using Gdc.Infrastructure.Persistence;

namespace Gdc.Api.Auth;

public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirstValue("tenant_id");
            Guid? tenantId = Guid.TryParse(tenantClaim, out var parsed) ? parsed : null;
            var role = context.User.FindFirstValue(ClaimTypes.Role);
            tenantContext.SetFromClaims(tenantId, role);
        }

        if (context.User.Identity?.IsAuthenticated == true
            && Guid.TryParse(context.Request.Headers["X-Tenant-Id"].FirstOrDefault(), out var headerTenantId))
        {
            tenantContext.ApplyHeaderTenant(headerTenantId);
        }

        await next(context);
    }
}

public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantContextMiddleware>();
}
