using Application.Dtos.WordsNote;

namespace FeatureFusion.Tests;

public class CardRequestResolveCollectionIdTests
{
    [Fact]
    public void UpsertResolveCollectionId_PrefersCollectionId()
    {
        var request = new CardUpsertRequestDTO
        {
            CollectionId = "collection-1",
            DeckId = "deck-1",
            DeskId = "desk-1",
        };

        Assert.Equal("collection-1", request.ResolveCollectionId());
    }

    [Fact]
    public void UpsertResolveCollectionId_FallsBackToDeckThenDesk()
    {
        var deckRequest = new CardUpsertRequestDTO
        {
            DeckId = "deck-1",
            DeskId = "desk-1",
        };

        var deskRequest = new CardUpsertRequestDTO
        {
            DeskId = "desk-1",
        };

        Assert.Equal("deck-1", deckRequest.ResolveCollectionId());
        Assert.Equal("desk-1", deskRequest.ResolveCollectionId());
    }

    [Fact]
    public void ImportResolveCollectionId_PrefersCollectionId()
    {
        var request = new CardImportRequestDTO
        {
            CollectionId = "collection-1",
            DeckId = "deck-1",
            DeskId = "desk-1",
        };

        Assert.Equal("collection-1", request.ResolveCollectionId());
    }
}
