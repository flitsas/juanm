namespace Gdc.Modules.Reglas.Application.Secretariat;

public sealed record ReglasSecretariatContactResponse(
    Guid Id,
    string SecretariatCode,
    string SecretariatName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ReglasSecretariatContactListResponse(
    IReadOnlyList<ReglasSecretariatContactResponse> Items);

public sealed record CreateReglasSecretariatContactRequest(
    string SecretariatCode,
    string SecretariatName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    bool IsActive = true);

public sealed record UpdateReglasSecretariatContactRequest(
    string SecretariatCode,
    string SecretariatName,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    bool IsActive);
