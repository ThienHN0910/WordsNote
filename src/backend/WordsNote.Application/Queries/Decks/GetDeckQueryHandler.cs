using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Queries.Decks;

public class GetDeckQueryHandler : IRequestHandler<GetDeckQuery, DeckDto?>
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IMapper _mapper;

    public GetDeckQueryHandler(IDeckRepository deckRepository, ICardRepository cardRepository, IMapper mapper)
    {
        _deckRepository = deckRepository;
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<DeckDto?> Handle(GetDeckQuery request, CancellationToken cancellationToken)
    {
        var deck = await _deckRepository.GetByIdAsync(request.Id);
        if (deck == null || deck.UserId != request.UserId)
            return null;

        var dto = _mapper.Map<DeckDto>(deck);
        var cardCounts = await _cardRepository.GetCardCountsByDeckIdsAsync(new[] { dto.Id });
        var dueCardCounts = await _cardRepository.GetDueCardCountsByDeckIdsAsync(new[] { dto.Id }, DateTime.UtcNow);

        dto.CardCount = cardCounts.TryGetValue(dto.Id, out var cardCount) ? cardCount : 0;
        dto.DueCardCount = dueCardCounts.TryGetValue(dto.Id, out var dueCount) ? dueCount : 0;

        return dto;
    }
}
