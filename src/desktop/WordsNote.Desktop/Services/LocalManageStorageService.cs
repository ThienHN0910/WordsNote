using System.Text.Json;
using System.IO;
using WordsNote.Desktop.Models;
using WordsNote.Desktop.Services.Serialization;

namespace WordsNote.Desktop.Services;

public sealed class LocalManageStorageService
{
    private readonly string _filePath;
    private readonly string _syncStateFilePath;

    public LocalManageStorageService(IAppDataPathProvider pathProvider)
    {
        _filePath = pathProvider.GetFilePath("manage-local-data.json");
        _syncStateFilePath = pathProvider.GetFilePath("manage-local-sync-state.json");
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

    public async Task<LocalCloudSyncState> LoadSyncStateAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_syncStateFilePath))
        {
            return new LocalCloudSyncState();
        }

        await using var stream = File.OpenRead(_syncStateFilePath);
        var state = await JsonSerializer.DeserializeAsync(
            stream,
            WordsNoteJsonSerializerContext.Default.LocalCloudSyncState,
            cancellationToken);

        return state is null
            ? new LocalCloudSyncState()
            : new LocalCloudSyncState
            {
                DeckIdMap = state.DeckIdMap
                    .Where(item => !string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    .ToDictionary(item => item.Key.Trim(), item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase),
                CardIdMap = state.CardIdMap
                    .Where(item => !string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    .ToDictionary(item => item.Key.Trim(), item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase),
            };
    }

    public async Task SaveSyncStateAsync(LocalCloudSyncState state, CancellationToken cancellationToken = default)
    {
        var payload = new LocalCloudSyncState
        {
            DeckIdMap = state.DeckIdMap
                .Where(item => !string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                .ToDictionary(item => item.Key.Trim(), item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase),
            CardIdMap = state.CardIdMap
                .Where(item => !string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                .ToDictionary(item => item.Key.Trim(), item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase),
        };

        await using var stream = File.Create(_syncStateFilePath);
        await JsonSerializer.SerializeAsync(
            stream,
            payload,
            WordsNoteJsonSerializerContext.Default.LocalCloudSyncState,
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

public sealed class LocalCloudSyncState
{
    public Dictionary<string, string> DeckIdMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> CardIdMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
