using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class SupabaseS3Options
{
    public string AccessKey { get; set; } = default!;

    public string SecretKey { get; set; } = default!;

    public string Endpoint { get; set; } = default!;

    public string ProjectRef { get; set; } = default!;

    public string Bucket { get; set; } = default!;

    public bool PublicBucket { get; set; } = false;

    public string DefaultFolder { get; set; } = "FFP";

    public bool UseAclPublicRead { get; set; } = false;
}
