using FluentAssertions;
using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

[Collection("Qdrant")]
public class SparseVectorTests : IAsyncLifetime
{
	private readonly QdrantClient _client;

	public SparseVectorTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateClient();

	[Fact]
	public async Task Search()
	{
		await CreateAndSeedSparseCollection("collection_1");

		var points = await _client.SearchAsync(
			"collection_1",
			new[] { 10.4f, 11.4f },
			sparseIndices: new[] { 0u, 1u },
			vectorName: "sparse-name",
			limit: 1);

		points.Should().HaveCount(1);
		var point = points.Single();
		point.Id.Should().Be((PointId)9ul);
		point.Payload.Should().ContainKeys("foo", "bar");
		point.Payload["foo"].Should().Be((Value)"goodbye");
		point.Payload["bar"].Should().Be((Value)2);
		point.Vectors.Should().BeNull();
	}

	[Fact]
	public async Task SearchGroups()
	{
		await CreateAndSeedSparseCollection("collection_1");

		await _client.UpsertAsync("collection_1", new[]
		{
			new PointStruct
			{
				Id = 10,
				Vectors = new[] { ("sparse-name", new Vector(new[]{ (30f, 0u), (31f, 1u) })) },
				Payload = { ["foo"] = "hello" }
			}
		});

		var groups = await _client.SearchGroupsAsync(
			"collection_1",
			new[] { 10.4f, 11.4f },
			groupBy: "foo",
			groupSize: 2,
			vectorName: "sparse-name",
			sparseIndices: new[] { 0u, 1u });

		Assert.Equal(2, groups.Count);
		Assert.Single(groups, g => g.Hits.Count == 2);
		Assert.Single(groups, g => g.Hits.Count == 1);
	}

	private async Task CreateAndSeedSparseCollection(string collection)
	{
		await _client.CreateCollectionAsync(
			collectionName: collection,
			sparseVectorsConfig: new SparseVectorConfig(("sparse-name", new SparseVectorParams { }));

		var updateResult = await _client.UpsertAsync(collection, new[]
		{
			new PointStruct
			{
				Id = 8,
				Vectors = new[] { ("sparse-name", new Vector(new[]{ (3.5f, 0u), (4.5f, 1u) })) },
				Payload =
				{
					["foo"] = "hello",
					["bar"] = 1
				}
			},
			new PointStruct
			{
				Id = 9,
				Vectors = new[] { ("sparse-name", new Vector(new[]{ (10.5f, 0u), (11.5f, 1u) })) },
				Payload =
				{
					["foo"] = "goodbye",
					["bar"] = 2
				}
			}
		});

		updateResult.Status.Should().Be(UpdateStatus.Completed);
	}

	public async Task InitializeAsync()
	{
		foreach (var collection in await _client.ListCollectionsAsync())
		{
			await _client.DeleteCollectionAsync(collection);
		}
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
