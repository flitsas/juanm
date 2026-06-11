namespace Gdc.Api.Configuration;

internal static class DotEnvLoader
{
    internal static void LoadIfPresent(string? path = null)
    {
        var envPath = path ?? FindEnvFile();
        LoadFile(envPath, onlyIfUnset: true);
    }

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

    private static string? FindEnvFile()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
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

    private static void LoadFile(string? path, bool onlyIfUnset = false)
    {
        if (path is null || !File.Exists(path))
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
            var value = Unquote(line[(separatorIndex + 1)..].Trim());
            if (key.Length == 0)
            {
                continue;
            }

            if (!onlyIfUnset || Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2
            && ((value.StartsWith('\'') && value.EndsWith('\''))
                || (value.StartsWith('"') && value.EndsWith('"'))))
        {
            return value[1..^1];
        }

        return value;
    }
}
