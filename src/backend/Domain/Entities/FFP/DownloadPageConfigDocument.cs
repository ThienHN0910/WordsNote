using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities.FFP;

public static class DownloadPageConfigKeys
{
    public const string Page = "download-page";
}

public class DownloadPageConfigDocument
{
    [BsonId]
    public string Key { get; set; } = DownloadPageConfigKeys.Page;

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? AppStoreUrl { get; set; }

    public string? EdgeAddonsUrl { get; set; }

    public string? Repo { get; set; }

    public int MaxVisibleVersions { get; set; } = 8;

    public string? FeaturedTag { get; set; }

    public List<DownloadVersionManualLinksDocument> ManualLinksByVersion { get; set; } = [];

    public string? UpdatedByEmail { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class DownloadVersionManualLinksDocument
{
    public string TagName { get; set; } = string.Empty;

    public List<DownloadManualLinkDocument> Links { get; set; } = [];
}

public class DownloadManualLinkDocument
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Kind { get; set; } = "other";
}