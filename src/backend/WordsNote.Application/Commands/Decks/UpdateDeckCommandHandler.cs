using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Decks;

public class UpdateDeckCommandHandler : IRequestHandler<UpdateDeckCommand, DeckDto?>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IMapper _mapper;

    public UpdateDeckCommandHandler(IDeckRepository deckRepository, IMapper mapper)
    {
        _deckRepository = deckRepository;
        _mapper = mapper;
    }

    public async Task<DeckDto?> Handle(UpdateDeckCommand request, CancellationToken cancellationToken)
    {
        var deck = await _deckRepository.GetByIdAsync(request.Id);
        if (deck == null) return null;

        deck.Update(request.Name, request.Description);
        await _deckRepository.UpdateAsync(deck);
        return _mapper.Map<DeckDto>(deck);
    }
}
