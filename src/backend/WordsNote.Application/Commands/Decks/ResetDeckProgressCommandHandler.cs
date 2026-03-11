using MediatR;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Decks;

public class ResetDeckProgressCommandHandler : IRequestHandler<ResetDeckProgressCommand, bool>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICardRepository _cardRepository;

    public ResetDeckProgressCommandHandler(IDeckRepository deckRepository, ICardRepository cardRepository)
    {
        _deckRepository = deckRepository;
        _cardRepository = cardRepository;
    }

    public async Task<bool> Handle(ResetDeckProgressCommand request, CancellationToken cancellationToken)
    {
        var deck = await _deckRepository.GetByIdAsync(request.DeckId);
        if (deck == null) return false;

        var cards = (await _cardRepository.GetByDeckIdAsync(request.DeckId)).ToList();
        foreach (var card in cards)
            card.ResetProgress();

        await _cardRepository.UpdateRangeAsync(cards);
        return true;
    }
}
