namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<FileMetadata> UploadAsync(Stream file, string fileName, string contentType, string? folder, CancellationToken ct);

    Task<Stream> DownloadAsync(string fileKey, CancellationToken ct);

    Task DeleteAsync(string fileKey, CancellationToken ct);

    string GeneratePresignedUrl(string fileKey, TimeSpan expiry);

    Task<bool> ExistsAsync(string fileKey, CancellationToken ct);
}
