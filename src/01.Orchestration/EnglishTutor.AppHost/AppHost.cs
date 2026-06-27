var builder = DistributedApplication.CreateBuilder(args);

var environment = LoadRepositoryEnvironment();

var api = builder.AddProject<Projects.EnglishTutor_Api>("api");
foreach (var (key, value) in environment)
{
    api.WithEnvironment(key, value);
}

var worker = builder.AddProject<Projects.EnglishTutor_Worker>("worker");
foreach (var (key, value) in environment)
{
    worker.WithEnvironment(key, value);
}

builder.Build().Run();

static Dictionary<string, string> LoadRepositoryEnvironment()
{
    var repositoryRoot = FindRepositoryRoot();
    var environment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    MergeEnvironmentFile(environment, Path.Combine(repositoryRoot, ".env.example"));
    MergeEnvironmentFile(environment, Path.Combine(repositoryRoot, ".env"));

    return environment
        .Where(item => IsUsableEnvironmentValue(item.Value))
        .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
}

static void MergeEnvironmentFile(IDictionary<string, string> target, string path)
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

static string FindRepositoryRoot()
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

static bool IsUsableEnvironmentValue(string value)
{
    return !string.IsNullOrWhiteSpace(value)
        && !(value.StartsWith('<') && value.EndsWith('>'));
}
