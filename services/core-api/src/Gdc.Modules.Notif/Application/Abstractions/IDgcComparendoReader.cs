namespace Gdc.Modules.Notif.Application.Abstractions;

public sealed record DgcComparendoSnapshot(
    Guid Id,
    Guid TenantId,
    string NumeroComparendo,
    string Estado,
    string? InfractorNombre,
    string? Placa,
    DateOnly? FechaComparendo,
    DateOnly? FechaNotificacion,
    string? DestinoEmail);

public interface IDgcComparendoReader
{
    Task<IReadOnlyList<DgcComparendoSnapshot>> ListByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken);
}
