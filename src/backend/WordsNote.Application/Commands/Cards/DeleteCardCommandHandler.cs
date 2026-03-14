using MediatR;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Cards;

public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand, bool>
{
    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;

    public DeleteCardCommandHandler(ICardRepository cardRepository, IDeckRepository deckRepository)
    {
        _cardRepository = cardRepository;
        _deckRepository = deckRepository;
    }

    public async Task<bool> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(request.CardId);
        if (card == null) return false;

        var deck = await _deckRepository.GetByIdAsync(card.DeckId);
        if (deck == null || deck.UserId != request.UserId)
            return false;

        await _cardRepository.DeleteAsync(request.CardId);
        return true;
    }
}
