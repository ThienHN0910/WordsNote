using Application.Dtos.FFP;
using Application.IRepositories.FFP;
using Application.IServices.FFP;
using Domain.Entities.FFP;

namespace Application.Services.FFP;

public class DownloadPageConfigService : IDownloadPageConfigService
{
    private readonly IDownloadPageConfigRepository _repository;

    public DownloadPageConfigService(IDownloadPageConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<DownloadPageConfigDTO> GetAsync()
    {
        var document = await _repository.GetAsync();
        return document == null ? BuildDefaultConfig() : MapToDto(document);
    }

    public async Task<DownloadPageConfigDTO> UpsertAsync(DownloadPageConfigDTO nextConfig, string? updatedByEmail = null)
    {
        var sanitized = Sanitize(nextConfig);
        var document = new DownloadPageConfigDocument
        {
            Key = DownloadPageConfigKeys.Page,
            Title = sanitized.Title,
            Summary = sanitized.Summary,
            Repo = sanitized.Repo,
            MaxVisibleVersions = sanitized.MaxVisibleVersions,
            FeaturedTag = sanitized.FeaturedTag,
            ManualLinksByVersion = sanitized.ManualLinksByVersion
                .Select(MapToDocument)
                .ToList(),
            UpdatedByEmail = NormalizeText(updatedByEmail),
            UpdatedAt = DateTime.UtcNow,
        };

        var persisted = await _repository.UpsertAsync(document);
        return MapToDto(persisted);
    }

    public Task ResetAsync()
    {
        return _repository.ResetAsync();
    }

    private static DownloadPageConfigDTO BuildDefaultConfig()
    {
        return new DownloadPageConfigDTO
        {
            MaxVisibleVersions = 8,
            ManualLinksByVersion = [],
            UpdatedAt = null,
            UpdatedByEmail = null,
        };
    }

    private static DownloadPageConfigDTO MapToDto(DownloadPageConfigDocument document)
    {
        return new DownloadPageConfigDTO
        {
            Title = NormalizeText(document.Title),
            Summary = NormalizeText(document.Summary),
            Repo = NormalizeText(document.Repo),
            MaxVisibleVersions = NormalizeCount(document.MaxVisibleVersions),
            FeaturedTag = NormalizeText(document.FeaturedTag),
            ManualLinksByVersion = (document.ManualLinksByVersion ?? [])
                .Select(MapToDto)
                .ToList(),
            UpdatedByEmail = NormalizeText(document.UpdatedByEmail),
            UpdatedAt = document.UpdatedAt,
        };
    }

    private static DownloadVersionManualLinksDTO MapToDto(DownloadVersionManualLinksDocument document)
    {
        return new DownloadVersionManualLinksDTO
        {
            TagName = NormalizeText(document.TagName),
            Links = (document.Links ?? [])
                .Select(MapToDto)
                .Where(link => !string.IsNullOrWhiteSpace(link.Name) && !string.IsNullOrWhiteSpace(link.Url))
                .ToList(),
        };
    }

    private static DownloadManualLinkDTO MapToDto(DownloadManualLinkDocument document)
    {
        return new DownloadManualLinkDTO
        {
            Name = NormalizeText(document.Name),
            Url = NormalizeText(document.Url),
            Kind = NormalizeKind(document.Kind),
        };
    }

    private static DownloadVersionManualLinksDocument MapToDocument(DownloadVersionManualLinksDTO dto)
    {
        return new DownloadVersionManualLinksDocument
        {
            TagName = NormalizeText(dto.TagName),
            Links = (dto.Links ?? [])
                .Select(MapToDocument)
                .Where(link => !string.IsNullOrWhiteSpace(link.Name) && !string.IsNullOrWhiteSpace(link.Url))
                .ToList(),
        };
    }

    private static DownloadManualLinkDocument MapToDocument(DownloadManualLinkDTO dto)
    {
        return new DownloadManualLinkDocument
        {
            Name = NormalizeText(dto.Name),
            Url = NormalizeText(dto.Url),
            Kind = NormalizeKind(dto.Kind),
        };
    }

    private static DownloadPageConfigDTO Sanitize(DownloadPageConfigDTO source)
    {
        var cleanedVersions = (source.ManualLinksByVersion ?? [])
            .Select(version => new DownloadVersionManualLinksDTO
            {
                TagName = NormalizeText(version.TagName),
                Links = (version.Links ?? [])
                    .Select(link => new DownloadManualLinkDTO
                    {
                        Name = NormalizeText(link.Name),
                        Url = NormalizeText(link.Url),
                        Kind = NormalizeKind(link.Kind),
                    })
                    .Where(link => !string.IsNullOrWhiteSpace(link.Name) && !string.IsNullOrWhiteSpace(link.Url))
                    .ToList(),
            })
            .Where(version => !string.IsNullOrWhiteSpace(version.TagName) && version.Links.Count > 0)
            .ToList();

        return new DownloadPageConfigDTO
        {
            Title = NormalizeText(source.Title),
            Summary = NormalizeText(source.Summary),
            Repo = NormalizeText(source.Repo),
            MaxVisibleVersions = NormalizeCount(source.MaxVisibleVersions),
            FeaturedTag = NormalizeText(source.FeaturedTag),
            ManualLinksByVersion = cleanedVersions,
            UpdatedByEmail = NormalizeText(source.UpdatedByEmail),
            UpdatedAt = source.UpdatedAt,
        };
    }

    private static string NormalizeText(string? raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? string.Empty : raw.Trim();
    }

    private static int NormalizeCount(int raw)
    {
        return Math.Min(30, Math.Max(1, raw));
    }

    private static string NormalizeKind(string? raw)
    {
        var normalized = NormalizeText(raw).ToLowerInvariant();
        return normalized switch
        {
            "installer" => "installer",
            "archive" => "archive",
            _ => "other",
        };
    }
}