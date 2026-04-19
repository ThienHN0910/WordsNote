using System.Text.Json.Serialization;
using WordsNote.Desktop.Models;

namespace WordsNote.Desktop.Services.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(DesktopAppSettings))]
[JsonSerializable(typeof(LocalManageData))]
[JsonSerializable(typeof(LocalCloudSyncState))]
[JsonSerializable(typeof(StudyDeck))]
[JsonSerializable(typeof(StudyCard))]
[JsonSerializable(typeof(ImportCardsResult))]
[JsonSerializable(typeof(List<StudyDeck>))]
[JsonSerializable(typeof(List<StudyCard>))]
[JsonSerializable(typeof(GoogleAuthTokenRequest))]
[JsonSerializable(typeof(CollectionUpsertRequest))]
[JsonSerializable(typeof(CardUpsertRequest))]
[JsonSerializable(typeof(CardsImportRequest))]
internal sealed partial class WordsNoteJsonSerializerContext : JsonSerializerContext
{
}
