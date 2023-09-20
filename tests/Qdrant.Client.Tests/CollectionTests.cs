using FluentAssertions;
using Grpc.Core;
using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

[Collection("Qdrant")]
public class CollectionTests
{
	private readonly QdrantClient _client;

	public CollectionTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateClient();

	[Fact]
	public Task Create()
		=> _client.CreateCollectionAsync(
			"collection_1",
			new() { Params = new VectorParams { Size = 4, Distance = Distance.Cosine } });

	[Fact]
	public async Task GetInfo()
	{
		await _client.CreateCollectionAsync(
			"collection_1",
			new() { Params = new VectorParams { Size = 4, Distance = Distance.Cosine } });

		var info = await _client.GetCollectionInfoAsync("collection_1");

		info.Status.Should().Be(CollectionStatus.Green);
	}

	[Fact]
	public async Task GetInfo_with_missing_collection()
	{
		var exception = await Assert.ThrowsAsync<RpcException>(() => _client.GetCollectionInfoAsync("collection_1"));
		exception.StatusCode.Should().Be(StatusCode.NotFound);
	}
}
