namespace Application.Dtos.WordsNote;

public class CardImportRequestDTO
{
    public string CollectionId { get; set; } = string.Empty;

    public string DeckId { get; set; } = string.Empty;

    public string DeskId { get; set; } = string.Empty;

    public string RawText { get; set; } = string.Empty;

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