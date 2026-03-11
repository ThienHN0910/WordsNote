using MediatR;
using WordsNote.Application.DTOs;

namespace WordsNote.Application.Commands.Cards;

public record ReviewCardCommand(Guid CardId, int Result) : IRequest<CardDto?>;
