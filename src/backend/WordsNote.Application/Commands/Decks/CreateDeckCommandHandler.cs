using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Entities;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Decks;

public class CreateDeckCommandHandler : IRequestHandler<CreateDeckCommand, DeckDto>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IMapper _mapper;

    public CreateDeckCommandHandler(IDeckRepository deckRepository, IMapper mapper)
    {
        _deckRepository = deckRepository;
        _mapper = mapper;
    }

    public async Task<DeckDto> Handle(CreateDeckCommand request, CancellationToken cancellationToken)
    {
        var deck = new Deck(request.Name, request.Description, request.UserId);
        await _deckRepository.AddAsync(deck);
        return _mapper.Map<DeckDto>(deck);
    }
}
