using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Commands.Decks;

public record CreateDeckCommand(string Name, string Description, string UserId) : IRequest<DeckDto>;
