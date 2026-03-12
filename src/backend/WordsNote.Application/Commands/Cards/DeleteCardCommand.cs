using MediatR;

namespace WordsNote.Application.Commands.Cards;

public record DeleteCardCommand(Guid CardId) : IRequest<bool>;
