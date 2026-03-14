using MediatR;

namespace WordsNote.Application.Commands.Decks;

public record ResetDeckProgressCommand(Guid DeckId, string UserId) : IRequest<bool>;
