using System.Text.Json;
using System.IO;
using WordsNote.Desktop.Models;
using WordsNote.Desktop.Services.Serialization;

namespace WordsNote.Desktop.Services;

public sealed class LocalManageStorageService
{
    private readonly string _filePath;

    public LocalManageStorageService(IAppDataPathProvider pathProvider)
    {
        _filePath = pathProvider.GetFilePath("manage-local-data.json");
    }

    public async Task<(List<StudyDeck> Decks, List<StudyCard> Cards)> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return ([], []);
        }

        await using var stream = File.OpenRead(_filePath);
        var data = await JsonSerializer.DeserializeAsync(
            stream,
            WordsNoteJsonSerializerContext.Default.LocalManageData,
            cancellationToken);
        return (data?.Decks ?? [], data?.Cards ?? []);
    }

    public async Task SaveAsync(IEnumerable<StudyDeck> decks, IEnumerable<StudyCard> cards, CancellationToken cancellationToken = default)
    {
        var payload = new LocalManageData
        {
            Decks = decks.Select(CloneDeck).ToList(),
            Cards = cards.Select(CloneCard).ToList(),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
        };

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(
            stream,
            payload,
            WordsNoteJsonSerializerContext.Default.LocalManageData,
            cancellationToken);
    }

    private static StudyDeck CloneDeck(StudyDeck deck)
    {
        return new StudyDeck
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt,
        };
    }

    private static StudyCard CloneCard(StudyCard card)
    {
        return new StudyCard
        {
            Id = card.Id,
            CollectionId = card.CollectionId,
            DeckId = card.DeckId,
            Front = card.Front,
            Back = card.Back,
            Hint = card.Hint,
            Tags = [.. card.Tags],
            DueAt = card.DueAt,
            LastReviewedAt = card.LastReviewedAt,
            Streak = card.Streak,
        };
    }

}

internal sealed class LocalManageData
{
    public string UpdatedAt { get; set; } = string.Empty;

    public List<StudyDeck> Decks { get; set; } = [];

    public List<StudyCard> Cards { get; set; } = [];
}
