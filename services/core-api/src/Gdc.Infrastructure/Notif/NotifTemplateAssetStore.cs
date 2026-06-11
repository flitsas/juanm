namespace Gdc.Infrastructure.Notif;

public static class NotifTemplateAssetStore
{
    private static readonly Dictionary<string, byte[]> Storage = new();

    public static void Save(Guid tenantId, Guid assetId, string extension, Stream content)
    {
        using var memory = new MemoryStream();
        content.CopyTo(memory);
        Storage[BuildKey(tenantId, assetId, extension)] = memory.ToArray();
    }

    public static byte[]? Get(Guid tenantId, Guid assetId, string extension) =>
        Storage.TryGetValue(BuildKey(tenantId, assetId, extension), out var bytes) ? bytes : null;

    public static void Clear() => Storage.Clear();

    private static string BuildKey(Guid tenantId, Guid assetId, string extension) =>
        $"{tenantId:N}/{assetId:N}{extension}";
}
