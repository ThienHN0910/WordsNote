using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Commands.Cards;

public record CreateCardCommand(string Front, string Back, string Notes, Guid DeckId, string UserId) : IRequest<CardDto?>;
