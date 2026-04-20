using Application.IRepositories.FFP;
using Domain.Entities.FFP;
using MongoDB.Driver;

namespace Infrastructure.Repositories.FFP;

public class DownloadPageConfigRepository : IDownloadPageConfigRepository
{
    private const string ConfigCollectionName = "wordsnote_download_page_configs";

    private readonly IMongoCollection<DownloadPageConfigDocument> _configs;

    public DownloadPageConfigRepository(IMongoDatabase database)
    {
        _configs = database.GetCollection<DownloadPageConfigDocument>(ConfigCollectionName);
    }

    public async Task<DownloadPageConfigDocument?> GetAsync()
    {
        return await _configs.Find(config => config.Key == DownloadPageConfigKeys.Page).FirstOrDefaultAsync();
    }

    public async Task<DownloadPageConfigDocument> UpsertAsync(DownloadPageConfigDocument config)
    {
        config.Key = DownloadPageConfigKeys.Page;

        await _configs.ReplaceOneAsync(
            existing => existing.Key == DownloadPageConfigKeys.Page,
            config,
            new ReplaceOptions { IsUpsert = true });

        return config;
    }

    public async Task ResetAsync()
    {
        await _configs.DeleteOneAsync(config => config.Key == DownloadPageConfigKeys.Page);
    }
}