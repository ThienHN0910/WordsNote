namespace Application.Storage;

public interface IFileStorageService
{
    Task<UploadFileResponse> UploadAsync(UploadFileRequest request, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
    Task<string> GetFileUrlAsync(string filePath, TimeSpan? expires = null, CancellationToken ct = default);
    Task<DownloadFileResponse> DownloadAsync(string filePath, CancellationToken ct = default); 
}