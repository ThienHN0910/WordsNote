using MediatR;

namespace WordsNote.Application.Commands.Decks;

public record ResetDeckProgressCommand(Guid DeckId) : IRequest<bool>;
