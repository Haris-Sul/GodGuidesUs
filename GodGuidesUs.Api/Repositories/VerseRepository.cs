using GodGuidesUs.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GodGuidesUs.Api.Repositories;

public class VerseRepository : IVerseRepository
{
    private readonly IMongoCollection<VerseModel> _versesCollection;

    public VerseRepository(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var settings = mongoDbSettings.Value;

        var mongoClient = new MongoClient(settings.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
        _versesCollection = mongoDatabase.GetCollection<VerseModel>(settings.VersesCollectionName);
    }

    public async Task<VerseModel> InsertAsync(VerseModel verse)
    {
        if (string.IsNullOrWhiteSpace(verse.Id))
        {
            verse.Id = ObjectId.GenerateNewId().ToString();
        }

        await _versesCollection.InsertOneAsync(verse);
        return verse;
    }

    public async Task<IReadOnlyList<VerseModel>> SearchVersesAsync(float[] queryVector)
    {
        if (queryVector.Length != 768)
        {
            throw new ArgumentException("queryVector must have 768 dimensions", nameof(queryVector));
        }

        var vectorSearchStage = new BsonDocument("$vectorSearch", new BsonDocument
        {
            { "index", "vector_index" },
            { "path", "Vector" },
            { "queryVector", new BsonArray(queryVector.Select(value => (double)value)) },
            { "numCandidates", 150 },
            { "limit", 3 }
        });

        var verses = await _versesCollection
            .Aggregate()
            .AppendStage<VerseModel>(vectorSearchStage)
            .ToListAsync();

        return verses;
    }
}