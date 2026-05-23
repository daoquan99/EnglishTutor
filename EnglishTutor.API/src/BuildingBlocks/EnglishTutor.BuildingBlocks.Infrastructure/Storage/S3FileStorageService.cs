using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Storage;

public sealed class S3FileStorageService : IFileStorageService, IAudioStorageService
{
    private readonly HttpClient _httpClient;
    private readonly StorageOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public S3FileStorageService(IOptions<StorageOptions> options)
        : this(options, new StandaloneHttpClientFactory())
    {
    }

    public S3FileStorageService(IOptions<StorageOptions> options, IHttpClientFactory httpClientFactory)
        : this(options, httpClientFactory, new DateTimeProvider(TimeProvider.System))
    {
    }

    public S3FileStorageService(
        IOptions<StorageOptions> options,
        IHttpClientFactory httpClientFactory,
        IDateTimeProvider dateTimeProvider)
    {
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient(StorageServiceRegistration.StorageHttpClientName);
        _dateTimeProvider = dateTimeProvider;
        ValidateOptions(_options);
    }

    public async Task<FileMetadata> UploadAsync(Stream file, string fileName, string contentType, string? folder, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var fileKey = BuildFileKey(safeFileName, folder);
        using var content = new StreamContent(file);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim());

        using var response = await _httpClient.PutAsync(CreatePresignedUri("PUT", fileKey, TimeSpan.FromMinutes(15)), content, ct);
        response.EnsureSuccessStatusCode();

        return new FileMetadata(
            fileKey,
            safeFileName,
            content.Headers.ContentType.MediaType ?? "application/octet-stream",
            TryGetLength(file),
            GeneratePresignedUrl(fileKey, TimeSpan.FromHours(1)),
            _dateTimeProvider.UtcNow);
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(CreatePresignedUri("GET", fileKey, TimeSpan.FromMinutes(15)), ct);
        response.EnsureSuccessStatusCode();
        return new MemoryStream(await response.Content.ReadAsByteArrayAsync(ct), writable: false);
    }

    public async Task DeleteAsync(string fileKey, CancellationToken ct)
    {
        using var response = await _httpClient.DeleteAsync(CreatePresignedUri("DELETE", fileKey, TimeSpan.FromMinutes(15)), ct);
        response.EnsureSuccessStatusCode();
    }

    public string GeneratePresignedUrl(string fileKey, TimeSpan expiry) =>
        CreatePresignedUri("GET", fileKey, expiry).ToString();

    public async Task<bool> ExistsAsync(string fileKey, CancellationToken ct)
    {
        using var response = await _httpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Head, CreatePresignedUri("HEAD", fileKey, TimeSpan.FromMinutes(15))),
            ct);

        return response.IsSuccessStatusCode;
    }

    private Uri CreatePresignedUri(string method, string fileKey, TimeSpan expiry)
    {
        var now = _dateTimeProvider.UtcNow;
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var dateStamp = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var region = string.IsNullOrWhiteSpace(_options.Region) ? "auto" : _options.Region.Trim();
        var service = "s3";
        var credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
        var endpoint = GetEndpoint();
        var canonicalUri = $"/{Uri.EscapeDataString(_options.BucketName!)}/{EncodePath(fileKey)}";
        var expires = Math.Clamp((int)expiry.TotalSeconds, 1, 604800);

        var queryParameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["X-Amz-Algorithm"] = "AWS4-HMAC-SHA256",
            ["X-Amz-Credential"] = $"{_options.AccessKey}/{credentialScope}",
            ["X-Amz-Date"] = amzDate,
            ["X-Amz-Expires"] = expires.ToString(CultureInfo.InvariantCulture),
            ["X-Amz-SignedHeaders"] = "host"
        };

        var canonicalQuery = string.Join('&', queryParameters.Select(pair => $"{UrlEncode(pair.Key)}={UrlEncode(pair.Value)}"));
        var canonicalHeaders = $"host:{endpoint.Host}\n";
        var signedHeaders = "host";
        var payloadHash = "UNSIGNED-PAYLOAD";
        var canonicalRequest = string.Join('\n', method, canonicalUri, canonicalQuery, canonicalHeaders, signedHeaders, payloadHash);
        var stringToSign = string.Join(
            '\n',
            "AWS4-HMAC-SHA256",
            amzDate,
            credentialScope,
            ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest))));

        var signingKey = GetSignatureKey(_options.SecretKey!, dateStamp, region, service);
        var signature = ToHex(HmacSha256(signingKey, stringToSign));
        var uriBuilder = new UriBuilder(endpoint)
        {
            Path = canonicalUri,
            Query = $"{canonicalQuery}&X-Amz-Signature={signature}"
        };

        return uriBuilder.Uri;
    }

    private Uri GetEndpoint()
    {
        if (!string.IsNullOrWhiteSpace(_options.ServiceUrl))
        {
            return new Uri(_options.ServiceUrl.TrimEnd('/'));
        }

        var region = string.IsNullOrWhiteSpace(_options.Region) ? "us-east-1" : _options.Region.Trim();
        return new Uri($"https://s3.{region}.amazonaws.com");
    }

    private static string BuildFileKey(string safeFileName, string? folder)
    {
        var extension = Path.GetExtension(safeFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(safeFileName);
        var fileName = $"{SanitizeSegment(nameWithoutExtension)}-{Guid.NewGuid():N}{extension}";
        var normalizedFolder = NormalizeFolder(folder);
        return string.IsNullOrWhiteSpace(normalizedFolder) ? fileName : $"{normalizedFolder}/{fileName}";
    }

    private static string NormalizeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return string.Empty;
        }

        return string.Join(
            '/',
            folder.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
                .Select(SanitizeSegment)
                .Where(segment => !string.IsNullOrWhiteSpace(segment)));
    }

    private static string SanitizeSegment(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "file" : value.Trim();
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            normalized = normalized.Replace(invalidCharacter, '-');
        }

        return normalized;
    }

    private static string EncodePath(string fileKey) =>
        string.Join('/', fileKey.Split('/').Select(Uri.EscapeDataString));

    private static string UrlEncode(string value) =>
        Uri.EscapeDataString(value).Replace("%20", "+", StringComparison.Ordinal);

    private static byte[] GetSignatureKey(string secretKey, string dateStamp, string regionName, string serviceName)
    {
        var dateKey = HmacSha256(Encoding.UTF8.GetBytes("AWS4" + secretKey), dateStamp);
        var dateRegionKey = HmacSha256(dateKey, regionName);
        var dateRegionServiceKey = HmacSha256(dateRegionKey, serviceName);
        return HmacSha256(dateRegionServiceKey, "aws4_request");
    }

    private static byte[] HmacSha256(byte[] key, string data) =>
        HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(data));

    private static string ToHex(byte[] bytes) =>
        Convert.ToHexString(bytes).ToLowerInvariant();

    private static long TryGetLength(Stream stream)
    {
        try
        {
            return stream.CanSeek ? stream.Length : 0;
        }
        catch (NotSupportedException)
        {
            return 0;
        }
    }

    private static void ValidateOptions(StorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException("Storage:BucketName is required for S3/R2 storage.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey) || string.IsNullOrWhiteSpace(options.SecretKey))
        {
            throw new InvalidOperationException("Storage:AccessKey and Storage:SecretKey are required for S3/R2 storage.");
        }
    }

    private sealed class StandaloneHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client = new();

        public HttpClient CreateClient(string name) => _client;
    }
}
