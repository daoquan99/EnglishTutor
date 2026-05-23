using EnglishTutor.BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService, IAudioStorageService
{
    private readonly string _rootPath;
    private readonly string _baseUrl;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LocalFileStorageService(IOptions<StorageOptions> options, IDateTimeProvider dateTimeProvider)
        : this(options.Value.LocalPath, options.Value.BaseUrl, dateTimeProvider)
    {
    }

    public LocalFileStorageService(string rootPath, string baseUrl = "/storage")
        : this(rootPath, baseUrl, new DateTimeProvider(TimeProvider.System))
    {
    }

    private LocalFileStorageService(string rootPath, string baseUrl, IDateTimeProvider dateTimeProvider)
    {
        _rootPath = Path.GetFullPath(string.IsNullOrWhiteSpace(rootPath) ? "storage" : rootPath);
        _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "/storage" : baseUrl.TrimEnd('/');
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<FileMetadata> UploadAsync(Stream file, string fileName, string contentType, string? folder, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var normalizedFolder = NormalizeFolder(folder);
        var extension = Path.GetExtension(safeFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);
        var storageFileName = $"{SanitizeName(nameWithoutExtension)}-{Guid.NewGuid():N}{extension}";
        var fileKey = string.IsNullOrWhiteSpace(normalizedFolder)
            ? storageFileName
            : $"{normalizedFolder}/{storageFileName}";

        var fullPath = ResolvePath(fileKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = File.Create(fullPath);
        await file.CopyToAsync(fileStream, ct);

        return new FileMetadata(
            fileKey,
            safeFileName,
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim(),
            fileStream.Length,
            GeneratePresignedUrl(fileKey, TimeSpan.FromHours(1)),
            _dateTimeProvider.UtcNow);
    }

    public Task<Stream> DownloadAsync(string fileKey, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Stream stream = File.OpenRead(ResolvePath(fileKey));
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string fileKey, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var fullPath = ResolvePath(fileKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public string GeneratePresignedUrl(string fileKey, TimeSpan expiry)
    {
        _ = expiry;
        return $"{_baseUrl}/{Uri.EscapeDataString(fileKey).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)}";
    }

    public Task<bool> ExistsAsync(string fileKey, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolvePath(fileKey)));
    }

    private string ResolvePath(string fileKey)
    {
        var normalizedKey = fileKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalizedKey));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage path is outside the configured root.");
        }

        return fullPath;
    }

    private static string SanitizeName(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "file" : value.Trim();
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            normalized = normalized.Replace(invalidCharacter, '-');
        }

        return normalized;
    }

    private static string NormalizeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return string.Empty;
        }

        var segments = folder
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Select(Path.GetFileName)
            .Where(segment => !string.IsNullOrWhiteSpace(segment));

        return string.Join('/', segments);
    }
}
