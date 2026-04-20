using Domain.Entities.FFP;

namespace Application.IRepositories.FFP;

public interface IDownloadPageConfigRepository
{
    Task<DownloadPageConfigDocument?> GetAsync();

    Task<DownloadPageConfigDocument> UpsertAsync(DownloadPageConfigDocument config);

    Task ResetAsync();
}