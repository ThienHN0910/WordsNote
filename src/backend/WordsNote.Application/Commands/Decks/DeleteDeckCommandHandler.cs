using MediatR;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Decks;

public class DeleteDeckCommandHandler : IRequestHandler<DeleteDeckCommand, bool>
{
    private readonly IDeckRepository _deckRepository;

    public DeleteDeckCommandHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository;
    }

    public async Task<bool> Handle(DeleteDeckCommand request, CancellationToken cancellationToken)
    {
        var deck = await _deckRepository.GetByIdAsync(request.Id);
        if (deck == null || deck.UserId != request.UserId) return false;

        await _deckRepository.DeleteAsync(request.Id);
        return true;
    }
}
