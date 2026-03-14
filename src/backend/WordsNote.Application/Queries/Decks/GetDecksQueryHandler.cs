using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Queries.Decks;

public class GetDecksQueryHandler : IRequestHandler<GetDecksQuery, IEnumerable<DeckDto>>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IMapper _mapper;

    public GetDecksQueryHandler(IDeckRepository deckRepository, ICardRepository cardRepository, IMapper mapper)
    {
        _deckRepository = deckRepository;
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DeckDto>> Handle(GetDecksQuery request, CancellationToken cancellationToken)
    {
        var decks = (await _deckRepository.GetByUserIdAsync(request.UserId)).ToList();
        var deckDtos = _mapper.Map<List<DeckDto>>(decks);

        if (deckDtos.Count == 0)
            return deckDtos;

        var deckIds = deckDtos.Select(d => d.Id).ToList();
        var cardCounts = await _cardRepository.GetCardCountsByDeckIdsAsync(deckIds);
        var dueCardCounts = await _cardRepository.GetDueCardCountsByDeckIdsAsync(deckIds, DateTime.UtcNow);

        foreach (var dto in deckDtos)
        {
            dto.CardCount = cardCounts.TryGetValue(dto.Id, out var cardCount) ? cardCount : 0;
            dto.DueCardCount = dueCardCounts.TryGetValue(dto.Id, out var dueCount) ? dueCount : 0;
        }

        return deckDtos;
    }
}
