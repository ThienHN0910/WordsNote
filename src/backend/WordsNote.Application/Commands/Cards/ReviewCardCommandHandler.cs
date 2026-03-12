using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Interfaces;
using WordsNote.Domain.ValueObjects;

namespace WordsNote.Application.Commands.Cards;

public class ReviewCardCommandHandler : IRequestHandler<ReviewCardCommand, CardDto?>
{
    private readonly ICardRepository _cardRepository;
    private readonly IMapper _mapper;

    public ReviewCardCommandHandler(ICardRepository cardRepository, IMapper mapper)
    {
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<CardDto?> Handle(ReviewCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(request.CardId);
        if (card == null) return null;

        var result = (CardReviewResult)request.Result;
        bool wasCorrect = result >= CardReviewResult.Good;

        card.UpdateReview(wasCorrect);
        await _cardRepository.UpdateAsync(card);
        return _mapper.Map<CardDto>(card);
    }
}
