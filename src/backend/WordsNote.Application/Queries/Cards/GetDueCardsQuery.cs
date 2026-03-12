using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Queries.Cards;

public record GetDueCardsQuery(string UserId) : IRequest<IEnumerable<CardDto>>;
