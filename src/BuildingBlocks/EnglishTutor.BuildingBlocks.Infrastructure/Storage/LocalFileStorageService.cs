namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService()
        : this(Path.Combine(AppContext.BaseDirectory, "storage"))
    {
    }

    public LocalFileStorageService(string rootPath) =>
        _rootPath = Path.GetFullPath(rootPath);

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        Directory.CreateDirectory(_rootPath);

        var storagePath = $"{Guid.NewGuid():N}_{safeFileName}";
        var fullPath = ResolvePath(storagePath);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        return storagePath;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var fullPath = ResolvePath(storagePath);
        Stream stream = File.OpenRead(fullPath);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var fullPath = ResolvePath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string storagePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storagePath));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage path is outside the configured root.");
        }

        return fullPath;
    }
}
