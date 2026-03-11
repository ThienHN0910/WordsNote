using MediatR;

namespace WordsNote.Application.Commands.Cards;

public record ImportFromCsvCommand(Guid DeckId, string CsvContent) : IRequest<int>;
