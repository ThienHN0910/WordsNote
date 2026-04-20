using Application.Dtos.FFP;

namespace Application.IServices.FFP;

public interface IDownloadPageConfigService
{
    Task<DownloadPageConfigDTO> GetAsync();

    Task<DownloadPageConfigDTO> UpsertAsync(DownloadPageConfigDTO nextConfig, string? updatedByEmail = null);

    Task ResetAsync();
}