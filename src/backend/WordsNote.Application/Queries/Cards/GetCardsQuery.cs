using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Queries.Cards;

public record GetCardsQuery(Guid DeckId) : IRequest<IEnumerable<CardDto>>;
