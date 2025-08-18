using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace QdrantExample;

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new QdrantClient("localhost", 6334);
                
        var collectionName = "example_collection";
        var vectorParams = new VectorParams 
        { 
            Size = 128,
            Distance = Distance.Cosine
        };

        await client.CreateCollectionAsync(collectionName, vectorParams);
        
        var collections = await client.ListCollectionsAsync();
        Console.WriteLine($"\nAvailable collections:");
        foreach (var collection in collections)
        {
            Console.WriteLine($"  - {collection}");
        }       
    }
}
