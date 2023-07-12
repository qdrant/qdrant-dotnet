using FluentAssertions;
using Grpc.Net.Client;
using Qdrant;
using Xunit;

namespace Qdrant.Grpc.Tests;

[Collection("Qdrant")]
public class CollectionTests
{
	private readonly QdrantGrpcClient _client;

	public CollectionTests(QdrantFixture qdrantFixture)
	{
		var address = QdrantChannel.ForAddress($"http://{qdrantFixture.Host}:{qdrantFixture.GrpcPort}");
		_client = new QdrantGrpcClient(address);
	}

	[Fact]
	public void CanCreateCollection()
	{
		var createResponse = _client.Collections.Create(new CreateCollection
		{
			CollectionName = "collection_1",
			VectorsConfig = new VectorsConfig
			{
				Params = new VectorParams { Size = 4, Distance = Distance.Cosine }
			}
		});

		createResponse.Result.Should().BeTrue();
		createResponse.Time.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task CanDeleteCollection()
	{
		var createResponse = await _client.Collections.CreateAsync(new CreateCollection
		{
			CollectionName = "collection_2",
			VectorsConfig = new VectorsConfig
			{
				Params = new VectorParams { Size = 4, Distance = Distance.Cosine }
			}
		});

		createResponse.Result.Should().BeTrue();

		var deleteResponse = await _client.Collections.DeleteAsync(new DeleteCollection
		{
			CollectionName = "collection_2",
		});

		createResponse.Result.Should().BeTrue();
	}
}
