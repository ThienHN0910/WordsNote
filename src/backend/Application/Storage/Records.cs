namespace Application.Storage;
public sealed record UploadFileResponse(string FilePath, string? Url = null);
public sealed record UploadFileRequest(
    Stream FileStream,
    string FileName,
    string ContentType,
    string? FolderName = null,
    string? CacheControl = null,
    string? ContentDisposition = null);
public sealed record DownloadFileResponse : IDisposable
{
    public Stream Stream { get; }
    public string ContentType { get; }
    public long? ContentLength { get; }
    public DateTime? LastModified { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public DownloadFileResponse(
        Stream stream,
        string contentType,
        long? contentLength = null,
        DateTime? lastModified = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Stream = stream;
        ContentType = contentType;
        ContentLength = contentLength;
        LastModified = lastModified;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    public void Dispose() => Stream.Dispose();
};
