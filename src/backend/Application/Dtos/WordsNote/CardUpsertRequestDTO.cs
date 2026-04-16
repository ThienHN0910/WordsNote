namespace Application.Dtos.WordsNote;

public class CardUpsertRequestDTO
{
    public string CollectionId { get; set; } = string.Empty;

    public string DeckId { get; set; } = string.Empty;

    public string DeskId { get; set; } = string.Empty;

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string Hint { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public string ResolveCollectionId()
    {
        if (!string.IsNullOrWhiteSpace(CollectionId))
        {
            return CollectionId;
        }

        if (!string.IsNullOrWhiteSpace(DeckId))
        {
            return DeckId;
        }

        return DeskId;
    }
}