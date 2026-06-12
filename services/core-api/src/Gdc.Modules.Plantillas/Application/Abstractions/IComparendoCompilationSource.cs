namespace Gdc.Modules.Plantillas.Application.Abstractions;

public sealed record ContraventorCompilationSnapshot(
    string Nombre,
    string Documento,
    string? Correo);

public sealed record ComparendoCompilationSnapshot(
    Guid Id,
    bool PendienteContraventor,
    string NumeroComparendo,
    string Estado,
    string? InfractorNombre,
    string? Documento,
    string? Placa,
    DateOnly? FechaComparendo,
    DateOnly? FechaNotificacion,
    decimal TotalValor,
    ContraventorCompilationSnapshot? Contraventor,
    string TenantNombre,
    string? SecretariaDestino);

public interface IComparendoCompilationSource
{
    Task<ComparendoCompilationSnapshot?> GetByIdAsync(
        Guid comparendoId,
        Guid tenantId,
        CancellationToken cancellationToken);
}
