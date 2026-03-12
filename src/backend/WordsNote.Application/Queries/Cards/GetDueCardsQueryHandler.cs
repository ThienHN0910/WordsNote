using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Queries.Cards;

public class GetDueCardsQueryHandler : IRequestHandler<GetDueCardsQuery, IEnumerable<CardDto>>
{
    private readonly ICardRepository _cardRepository;
    private readonly IMapper _mapper;

    public GetDueCardsQueryHandler(ICardRepository cardRepository, IMapper mapper)
    {
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CardDto>> Handle(GetDueCardsQuery request, CancellationToken cancellationToken)
    {
        var cards = await _cardRepository.GetDueCardsAsync(request.UserId, DateTime.UtcNow);
        return _mapper.Map<IEnumerable<CardDto>>(cards);
    }
}
