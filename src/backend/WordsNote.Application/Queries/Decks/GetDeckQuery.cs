using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Queries.Decks;

public record GetDeckQuery(Guid Id) : IRequest<DeckDto?>;
