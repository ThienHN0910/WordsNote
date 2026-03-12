using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Queries.Decks;

public class GetDecksQueryHandler : IRequestHandler<GetDecksQuery, IEnumerable<DeckDto>>
{
    private readonly IDeckRepository _deckRepository;
    private readonly IMapper _mapper;

    public GetDecksQueryHandler(IDeckRepository deckRepository, IMapper mapper)
    {
        _deckRepository = deckRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DeckDto>> Handle(GetDecksQuery request, CancellationToken cancellationToken)
    {
        var decks = await _deckRepository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<IEnumerable<DeckDto>>(decks);
    }
}
