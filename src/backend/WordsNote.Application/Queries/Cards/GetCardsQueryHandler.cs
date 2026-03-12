using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Queries.Cards;

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, IEnumerable<CardDto>>
{
    private readonly ICardRepository _cardRepository;
    private readonly IMapper _mapper;

    public GetCardsQueryHandler(ICardRepository cardRepository, IMapper mapper)
    {
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CardDto>> Handle(GetCardsQuery request, CancellationToken cancellationToken)
    {
        var cards = await _cardRepository.GetByDeckIdAsync(request.DeckId);
        return _mapper.Map<IEnumerable<CardDto>>(cards);
    }
}
