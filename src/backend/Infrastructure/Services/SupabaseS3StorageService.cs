using Amazon.S3;
using Amazon.S3.Model;
using Application.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;


namespace Infrastructure.Services;

public sealed class SupabaseS3StorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly SupabaseS3Options _opt;
    private readonly ILogger<SupabaseS3StorageService> _logger;

    private static readonly Regex InvalidKeyChars = new(@"[^a-zA-Z0-9_\-\./]", RegexOptions.Compiled);

    public SupabaseS3StorageService(
        IOptions<SupabaseS3Options> options,
        ILogger<SupabaseS3StorageService> logger)
    {
        _opt = options.Value;
        _logger = logger;

        var config = new AmazonS3Config
        {
            ServiceURL = _opt.Endpoint.TrimEnd('/'),
            ForcePathStyle = true
        };

        _s3 = new AmazonS3Client(_opt.AccessKey, _opt.SecretKey, config);
    }

    public async Task<UploadFileResponse> UploadAsync(UploadFileRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request.FileStream);
        if (!request.FileStream.CanRead) throw new ArgumentException("Stream is not readable.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.FileName)) throw new ArgumentException("FileName is required.", nameof(request.FileName));

        var safeFileName = SanitizeFileName(request.FileName);
        var folder = string.IsNullOrWhiteSpace(request.FolderName)
            ? _opt.DefaultFolder.Trim('/').Trim()
            : request.FolderName.Trim('/').Trim();

        var key = string.IsNullOrEmpty(folder)
            ? $"{Guid.NewGuid():N}_{safeFileName}"
            : $"{folder}/{Guid.NewGuid():N}_{safeFileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key,
            InputStream = request.FileStream,
            ContentType = request.ContentType ?? "application/octet-stream",
            AutoCloseStream = false
        };

        if (!string.IsNullOrWhiteSpace(request.CacheControl))
            putRequest.Headers.CacheControl = request.CacheControl;

        if (!string.IsNullOrWhiteSpace(request.ContentDisposition))
            putRequest.Headers.ContentDisposition = request.ContentDisposition;

        if (_opt.PublicBucket && _opt.UseAclPublicRead)
            putRequest.CannedACL = S3CannedACL.PublicRead;

        await _s3.PutObjectAsync(putRequest, ct);

        var url = _opt.PublicBucket ? BuildPublicUrl(key) : null;
        return new UploadFileResponse(key, url);
    }

    public Task DeleteAsync(string filePath, CancellationToken ct = default)
        => _s3.DeleteObjectAsync(_opt.Bucket, filePath.Trim(), ct);

    public Task<string> GetFileUrlAsync(string filePath, TimeSpan? expires = null, CancellationToken ct = default)
    {
        filePath = filePath.Trim();

        if (_opt.PublicBucket)
            return Task.FromResult(BuildPublicUrl(filePath));

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _opt.Bucket,
            Key = filePath,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expires ?? TimeSpan.FromHours(1))
        };

        return Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public async Task<DownloadFileResponse> DownloadAsync(string filePath, CancellationToken ct = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = filePath.Trim()
        };

        GetObjectResponse response;
        try
        {
            response = await _s3.GetObjectAsync(request, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException($"File not found: {filePath}", filePath);
        }

        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        response.Dispose();

        var metadata = response.Metadata.Keys
            .ToDictionary(k => k, k => response.Metadata[k], StringComparer.OrdinalIgnoreCase);

        return new DownloadFileResponse(
            stream: memoryStream,
            contentType: response.Headers.ContentType ?? "application/octet-stream",
            contentLength: response.ContentLength > 0 ? response.ContentLength : null,
            lastModified: response.LastModified == DateTime.MinValue ? null : response.LastModified?.ToUniversalTime(),
            metadata: metadata
        );
    }

    private string BuildPublicUrl(string key)
        => $"https://{_opt.ProjectRef}.supabase.co/storage/v1/object/public/{_opt.Bucket}/{key.TrimStart('/')}";

    private static string SanitizeFileName(string fileName)
        => InvalidKeyChars.Replace(Path.GetFileName(fileName).Trim(), "_");
}