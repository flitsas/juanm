using Gdc.Modules.Plantillas.Application.Abstractions;

namespace Gdc.Infrastructure.Plantillas;

public sealed class PdfBinaryAssetStore : IBinaryAssetStore
{
    private static readonly Dictionary<string, byte[]> Storage = new();

    public Task<string> SaveAsync(
        Guid tenantId,
        string category,
        Stream content,
        string fileName,
        CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        content.CopyTo(memory);
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".pdf";
        }

        var assetId = Guid.CreateVersion7();
        var storageKey = $"{tenantId:N}/{category}/{assetId:N}{extension.ToLowerInvariant()}";
        Storage[storageKey] = memory.ToArray();
        return Task.FromResult(storageKey);
    }

    public Task<byte[]?> GetAsync(Guid tenantId, string storageKey, CancellationToken cancellationToken)
    {
        if (!storageKey.StartsWith($"{tenantId:N}/", StringComparison.Ordinal))
        {
            return Task.FromResult<byte[]?>(null);
        }

        return Task.FromResult(Storage.TryGetValue(storageKey, out var bytes) ? bytes : null);
    }

    public static void Clear() => Storage.Clear();
}
