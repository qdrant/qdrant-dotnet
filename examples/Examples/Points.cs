using Grpc.Net.Client;
using Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Examples;

public class Points
{
	public async Task Upsert()
	{
		var address = GrpcChannel.ForAddress("http://localhost:6334");
		var client = new QdrantGrpcClient(address);

		#region Upsert
		var pointsOperationResponse = await client.Points.UpsertAsync(new UpsertPoints
		{
			CollectionName = "my_collection",
			Ordering = new() { Type = WriteOrderingType.Medium },
			Wait = true,
			Points =
			{
				new PointStruct
				{
					Id = 1,
					Vectors = new [] { 1f, 2f, 3f, 4f },
					Payload =
					{
						["color"] = "blue",
						["count"] = 7,
						["precision"] = 0.866
					}
				},
				new PointStruct
				{
					Id = 2,
					Vectors = new[] { 2f, 3f, 4f, 5f },
					Payload =
					{
						["color"] = "red",
						["count"] = 5,
						["precision"] = 0.9992
					}
				}
			}
		});
		#endregion
	}

	public async Task Search()
	{
		var address = GrpcChannel.ForAddress("http://localhost:6334");
		var client = new QdrantGrpcClient(address);

		#region Search
		var searchResponse = await client.Points.SearchAsync(new SearchPoints
		{
			Vector = { 1, 3, 4, 5 },
			Params = new SearchParams
			{
				HnswEf = 10,
				Exact = false
			},
			Limit = 10
		});
		#endregion
	}
}
