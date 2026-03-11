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

    private static void ConfigureMongoDbSerialization()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        if (!BsonClassMap.IsClassMapRegistered(typeof(Card)))
        {
            BsonClassMap.RegisterClassMap<Card>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Deck)))
        {
            BsonClassMap.RegisterClassMap<Deck>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdMember(c => c.Id).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                cm.MapField("_cards").SetElementName("Cards");
                cm.UnmapMember(c => c.Cards);
            });
        }
    }
}
