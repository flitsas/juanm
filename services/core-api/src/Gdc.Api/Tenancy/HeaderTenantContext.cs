using Gdc.Modules.Dgc.Application.Abstractions;

namespace Gdc.Api.Tenancy;

public sealed class HeaderTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId =>
        TryGetGuidHeader("X-Tenant-Id")
        ?? throw new InvalidOperationException("Missing or invalid X-Tenant-Id header.");

    public Guid? UserId => TryGetGuidHeader("X-User-Id");

    private Guid? TryGetGuidHeader(string headerName)
    {
        var value = httpContextAccessor.HttpContext?.Request.Headers[headerName].FirstOrDefault();
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
