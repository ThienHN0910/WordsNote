using AutoMapper;
using MediatR;
using WordsNote.Application.DTOs;
using WordsNote.Domain.Entities;
using WordsNote.Domain.Interfaces;

namespace WordsNote.Application.Commands.Cards;

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, CardDto>
{
    private readonly ICardRepository _cardRepository;
    private readonly IMapper _mapper;

    public CreateCardCommandHandler(ICardRepository cardRepository, IMapper mapper)
    {
        _cardRepository = cardRepository;
        _mapper = mapper;
    }

    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var card = new Card(request.DeckId, request.Front, request.Back, request.Notes);
        await _cardRepository.AddAsync(card);
        return _mapper.Map<CardDto>(card);
    }
}
