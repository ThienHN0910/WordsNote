using Application.Helpers;

namespace FeatureFusion.Tests;

public class TextNormalizerTests
{
    [Fact]
    public void NormalizeForCompare_RemovesVietnameseDiacritics_AndLowercases()
    {
        var input = "  Tiếng   Việt Có  Dấu  ";

        var normalized = TextNormalizer.NormalizeForCompare(input);

        Assert.Equal("tieng viet co dau", normalized);
    }

    [Fact]
    public void NormalizeForCompare_CollapsesWhitespace()
    {
        var input = "hello\n\t   world";

        var normalized = TextNormalizer.NormalizeForCompare(input);

        Assert.Equal("hello world", normalized);
    }

    [Fact]
    public void NormalizeForCompare_EmptyInput_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, TextNormalizer.NormalizeForCompare(null));
        Assert.Equal(string.Empty, TextNormalizer.NormalizeForCompare("    "));
    }
}
