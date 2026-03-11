using MediatR;

namespace WordsNote.Application.Commands.Decks;

public record DeleteDeckCommand(Guid Id) : IRequest<bool>;
