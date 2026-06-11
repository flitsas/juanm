namespace Gdc.Modules.Notif.Application.TenantAdmin;

public sealed record CompanyResponse(
    Guid Id,
    string Name,
    string? Nit,
    string? ContactPhone,
    string? ContactEmail,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CreateCompanyRequest(
    string Name,
    string? Nit,
    string? ContactPhone,
    string? ContactEmail);

public sealed record UpdateCompanyRequest(
    string Name,
    string? Nit,
    string? ContactPhone,
    string? ContactEmail,
    bool? IsActive);

public sealed record TenantProfileResponse(
    Guid TenantId,
    string Name,
    string? ContactPhone,
    string? ContactEmail);

public sealed record UpdateTenantProfileRequest(
    string? ContactPhone,
    string? ContactEmail);

public sealed record CompanyListResponse(IReadOnlyList<CompanyResponse> Items);
