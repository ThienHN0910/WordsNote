namespace Application.Dtos.WordsNote;

public class CardUpsertRequestDTO
{
    public string DeskId { get; set; } = string.Empty;

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string Hint { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];
}