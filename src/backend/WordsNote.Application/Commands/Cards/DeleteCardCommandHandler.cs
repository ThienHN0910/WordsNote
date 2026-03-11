using MediatR;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Cards;

public class DeleteCardCommandHandler : IRequestHandler<DeleteCardCommand, bool>
{
    private readonly ICardRepository _cardRepository;

    public DeleteCardCommandHandler(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<bool> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(request.CardId);
        if (card == null) return false;

        await _cardRepository.DeleteAsync(request.CardId);
        return true;
    }
}
