using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using WordsNote.Application.Interfaces;
using WordsNote.Domain.Entities;
using WordsNote.Domain.Interfaces;
using WordsNote.Infrastructure.Persistence;
using WordsNote.Infrastructure.Repositories;
using WordsNote.Infrastructure.Services;

namespace WordsNote.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureMongoDbSerialization();

        var mongoSettings = configuration.GetSection("MongoDb").Get<MongoDbSettings>()
            ?? new MongoDbSettings { ConnectionString = "mongodb://localhost:27017", DatabaseName = "WordsNote" };

        services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoSettings.ConnectionString));
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.DatabaseName);
        });
        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IDeckRepository, DeckRepository>();
        services.AddScoped<ICardRepository, CardRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    private const string DeckCardsFieldName = "_cards";

    private static void ConfigureMongoDbSerialization()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(b => b.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Card)))
        {
            BsonClassMap.RegisterClassMap<Card>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                // Id is mapped on BaseEntity
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Deck)))
        {
            BsonClassMap.RegisterClassMap<Deck>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                // Id is mapped on BaseEntity
                cm.MapField(DeckCardsFieldName).SetElementName("Cards");
                cm.UnmapMember(c => c.Cards);
            });
        }
    }
}
