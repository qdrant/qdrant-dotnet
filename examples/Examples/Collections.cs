using Grpc.Net.Client;
using Qdrant;
using Qdrant.Grpc;

namespace Examples;

public class Collections
{
	public async Task Create()
	{
		var address = GrpcChannel.ForAddress("http://localhost:6334");
		var client = new QdrantGrpcClient(address);

		#region CreateCollection
		var collectionOperationResponse = await client.Collections.CreateAsync(new CreateCollection
		{
			CollectionName = "my_collection",
			VectorsConfig = new VectorsConfig
			{
				Params = new VectorParams
				{
					Size = 1536,
					Distance = Distance.Cosine
				}
			}
		});
		#endregion
	}

	public async Task Delete()
	{
		var address = GrpcChannel.ForAddress("http://localhost:6334");
		var client = new QdrantGrpcClient(address);

		#region DeleteCollection
		var collectionOperationResponse = await client.Collections.DeleteAsync(new DeleteCollection
		{
			CollectionName = "my_collection",
			Timeout = 10
		});
		#endregion
	}
}
