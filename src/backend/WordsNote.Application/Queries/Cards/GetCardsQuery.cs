using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Queries.Cards;

public record GetCardsQuery(Guid DeckId, string UserId) : IRequest<IEnumerable<CardDto>>;
