using Gdc.Modules.Reglas.Domain.Entities;

namespace Gdc.Modules.Reglas.Application.Abstractions;

public interface IReglasSecretariatResolver
{
    Task<SecretariatContact?> ResolveAsync(
        Guid tenantId,
        Guid comparendoId,
        Guid? ruleContactId,
        CancellationToken cancellationToken);
}
