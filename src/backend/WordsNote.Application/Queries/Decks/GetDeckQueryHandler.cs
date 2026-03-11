using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Queries.Decks;

public class GetDeckQueryHandler : IRequestHandler<GetDeckQuery, DeckDto?>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IMapper _mapper;

    public GetDeckQueryHandler(IDeckRepository deckRepository, IMapper mapper)
    {
        _deckRepository = deckRepository;
        _mapper = mapper;
    }

    public async Task<DeckDto?> Handle(GetDeckQuery request, CancellationToken cancellationToken)
    {
        var deck = await _deckRepository.GetByIdAsync(request.Id);
        return deck == null ? null : _mapper.Map<DeckDto>(deck);
    }
}
