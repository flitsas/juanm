namespace Gdc.Infrastructure.Dgc.Renting;

internal static class RentingCertificatePathResolver
{
    internal static string Resolve(string configuredPath, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("Dgc:RentingApi:CertificatePath is not configured.");
        }

        if (Path.IsPathRooted(configuredPath) && File.Exists(configuredPath))
        {
            return configuredPath;
        }

        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(contentRootPath, configuredPath)),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "..", "..", configuredPath)),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "..", "..", "..", configuredPath)),
        };

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(
            $"Renting API certificate not found. Configured path: {configuredPath}. Content root: {contentRootPath}");
    }
}
