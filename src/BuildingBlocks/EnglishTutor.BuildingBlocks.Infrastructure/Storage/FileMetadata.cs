namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public sealed record FileMetadata(
    string FileKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url,
    DateTime UploadedAtUtc);
