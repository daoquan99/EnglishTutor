namespace Microsoft.Extensions.Hosting;

public static class RepositoryEnvironment
{
    public static Dictionary<string, string> Load()
    {
        var repositoryRoot = FindRepositoryRoot();
        var environment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        MergeEnvironmentFile(environment, Path.Combine(repositoryRoot, ".env.example"));
        MergeEnvironmentFile(environment, Path.Combine(repositoryRoot, ".env"));

        return environment
            .Where(item => IsUsableEnvironmentValue(item.Value))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
    }

    public static void LoadIntoProcess()
    {
        foreach (var (key, value) in Load())
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static void MergeEnvironmentFile(IDictionary<string, string> target, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(path))
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
            if (key.Length > 0)
            {
                target[key] = value;
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EnglishTutor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private static bool IsUsableEnvironmentValue(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && !(value.StartsWith('<') && value.EndsWith('>'));
    }
}
