namespace Atlas.SharedKernel.Configuration;

public static class DotEnvLoader
{
    public static void Load(string fileName = ".env")
    {
        var path = FindFile(fileName);
        if (path is null)
            return;

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
                line = line["export ".Length..].TrimStart();

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (key.Length == 0 || Environment.GetEnvironmentVariable(key) is not null)
                continue;

            Environment.SetEnvironmentVariable(key, Unquote(value));
        }
    }

    private static string? FindFile(string fileName)
    {
        foreach (var start in GetSearchRoots())
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, fileName);
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        yield return Directory.GetCurrentDirectory();
        yield return AppContext.BaseDirectory;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2
            && ((value[0] == '"' && value[^1] == '"')
                || (value[0] == '\'' && value[^1] == '\'')))
            return value[1..^1];

        return value;
    }
}
