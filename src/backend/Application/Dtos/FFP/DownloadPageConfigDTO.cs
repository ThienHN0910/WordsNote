namespace Application.Dtos.FFP;

public class DownloadPageConfigDTO
{
    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? Repo { get; set; }

    public int MaxVisibleVersions { get; set; } = 8;

    public string? FeaturedTag { get; set; }

    public List<DownloadVersionManualLinksDTO> ManualLinksByVersion { get; set; } = [];

    public string? UpdatedByEmail { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class DownloadVersionManualLinksDTO
{
    public string TagName { get; set; } = string.Empty;

    public List<DownloadManualLinkDTO> Links { get; set; } = [];
}

public class DownloadManualLinkDTO
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string Kind { get; set; } = "other";
}