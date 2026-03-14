using MediatR;
using WordsNote.Domain.Entities;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Cards;

public class ImportFromCsvCommandHandler : IRequestHandler<ImportFromCsvCommand, int?>
{
    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;

    public ImportFromCsvCommandHandler(ICardRepository cardRepository, IDeckRepository deckRepository)
    {
        _cardRepository = cardRepository;
        _deckRepository = deckRepository;
    }

    public async Task<int?> Handle(ImportFromCsvCommand request, CancellationToken cancellationToken)
    {
        var deck = await _deckRepository.GetByIdAsync(request.DeckId);
        if (deck == null || deck.UserId != request.UserId) return null;

        var lines = request.CsvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var cards = new List<Card>();

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 2) continue;

            var front = parts[0].Trim();
            var back = parts[1].Trim();
            var notes = parts.Length > 2 ? parts[2].Trim() : string.Empty;

            if (!string.IsNullOrWhiteSpace(front) && !string.IsNullOrWhiteSpace(back))
                cards.Add(new Card(request.DeckId, front, back, notes));
        }

        if (cards.Count > 0)
            await _cardRepository.AddRangeAsync(cards);

        return cards.Count;
    }
}
