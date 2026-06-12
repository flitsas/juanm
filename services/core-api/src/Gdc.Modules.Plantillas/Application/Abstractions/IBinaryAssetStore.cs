namespace Gdc.Modules.Plantillas.Application.Abstractions;

public interface IBinaryAssetStore
{
    Task<string> SaveAsync(
        Guid tenantId,
        string category,
        Stream content,
        string fileName,
        CancellationToken cancellationToken);

    Task<byte[]?> GetAsync(Guid tenantId, string storageKey, CancellationToken cancellationToken);
}
