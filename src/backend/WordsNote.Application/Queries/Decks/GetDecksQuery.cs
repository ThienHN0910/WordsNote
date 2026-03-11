using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Queries.Decks;

public record GetDecksQuery(string UserId) : IRequest<IEnumerable<DeckDto>>;
