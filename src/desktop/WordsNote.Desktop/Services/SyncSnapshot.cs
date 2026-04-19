using WordsNote.Desktop.Models;

namespace WordsNote.Desktop.Services;

public static class SyncSnapshot
{
    public static string CreateDeckMatchKey(string title)
    {
        return Normalize(title);
    }

    public static string CreateDeckFingerprint(StudyDeck deck)
    {
        return string.Join("::", CreateDeckMatchKey(deck.Title), Normalize(deck.Description));
    }

    public static string CreateCardMatchKey(string collectionId, string front, string back)
    {
        return string.Join("::", Normalize(collectionId), Normalize(front), Normalize(back));
    }

    public static string CreateCardFingerprint(StudyCard card, string? collectionIdOverride = null)
    {
        var collectionId = string.IsNullOrWhiteSpace(collectionIdOverride)
            ? card.CollectionId
            : collectionIdOverride;

        return string.Join(
            "::",
            CreateCardMatchKey(collectionId, card.Front, card.Back),
            Normalize(card.Hint),
            NormalizeTags(card.Tags));
    }

    private static string Normalize(string? value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return string.Empty;
        }

        return string.Join(
            "|",
            tags.Select(Normalize)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .OrderBy(tag => tag, StringComparer.Ordinal));
    }
}
