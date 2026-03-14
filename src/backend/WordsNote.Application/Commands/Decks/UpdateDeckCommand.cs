using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Commands.Decks;

public record UpdateDeckCommand(Guid Id, string Name, string Description, string UserId) : IRequest<DeckDto?>;
