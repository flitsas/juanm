namespace Gdc.Api.Configuration;

internal static class DotEnvLoader
{
    internal static void LoadRentingEnvFromRepoRoot()
    {
        var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory())
            ?? FindRepoRoot(AppContext.BaseDirectory);

        if (repoRoot is null)
        {
            return;
        }

        LoadFile(Path.Combine(repoRoot, ".env.renting"));
    }

    private static string? FindRepoRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, ".gitignore"))
                && Directory.Exists(Path.Combine(directory.FullName, "services")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static void LoadFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"');
            if (key.Length == 0)
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
