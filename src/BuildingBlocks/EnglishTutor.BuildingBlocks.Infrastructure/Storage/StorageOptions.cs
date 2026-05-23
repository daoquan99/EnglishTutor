namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public sealed class StorageOptions
{
    public StorageProvider Provider { get; set; } = StorageProvider.Local;
    public string? BucketName { get; set; }
    public string? Region { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? ServiceUrl { get; set; }
    public string BaseUrl { get; set; } = "/storage";
    public string LocalPath { get; set; } = "storage";
}
