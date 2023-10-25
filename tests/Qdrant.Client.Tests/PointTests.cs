using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

[Collection("Qdrant")]
public class PointTests : IAsyncLifetime
{
	private readonly QdrantClient _client;

	public PointTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateClient();

	[Fact]
	public async Task Retrieve()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.RetrieveAsync("collection_1", new[] { new PointId { Num = 9 } });

		var point = Assert.Single(points);

		Assert.Equal(9ul, point.Id.Num);

		var payloadKeyValue = Assert.Single(point.Payload);
		Assert.Equal("foo", payloadKeyValue.Key);
		Assert.Equal("goodbye", payloadKeyValue.Value.StringValue);

		Assert.Null(point.Vectors);
	}

	[Fact]
	public async Task Retrieve_with_vector_without_payload()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.RetrieveAsync(
			"collection_1",
			new[] { new PointId { Num = 8 } },
			withPayload: false,
			withVectors: true);

		var point = Assert.Single(points);
		Assert.Equal(8ul, point.Id.Num);
		Assert.Empty(point.Payload);
		Assert.NotNull(point.Vectors);
	}

	[Fact]
	public async Task SetPayload()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.SetPayloadAsync(
			"collection_1",
			new Dictionary<string, Value> { ["bar"] = "some bar" },
			new[] { 9ul });

		var points = await _client.RetrieveAsync("collection_1", new[] { new PointId { Num = 9 } });

		var point = Assert.Single(points);
		Assert.Collection(
			point.Payload.OrderBy(kv => kv.Key),
			kv =>
			{
				Assert.Equal("bar", kv.Key);
				Assert.Equal("some bar", kv.Value.StringValue);
			},
			kv =>
			{
				Assert.Equal("foo", kv.Key);
				Assert.Equal("goodbye", kv.Value.StringValue);
			});
	}

	[Fact]
	public async Task OverwritePayload()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.OverwritePayloadAsync(
			"collection_1",
			new Dictionary<string, Value> { ["bar"] = "some bar" },
			new[] { 9ul });

		var points = await _client.RetrieveAsync("collection_1", new[] { new PointId { Num = 9 } });

		var point = Assert.Single(points);
		var payloadKeyValue = Assert.Single(point.Payload);
		Assert.Equal("bar", payloadKeyValue.Key);
		Assert.Equal("some bar", payloadKeyValue.Value.StringValue);
	}

	[Fact]
	public async Task DeletePayload()
	{
		await CreateAndSeedCollection("collection_1");
		await _client.SetPayloadAsync(
			"collection_1",
			new Dictionary<string, Value> { ["bar"] = "some bar" },
			new[] { 9ul });

		await _client.DeletePayloadAsync("collection_1", new[] { "foo" }, new[] { 9ul });

		var points = await _client.RetrieveAsync("collection_1", new[] { new PointId { Num = 9 } });

		var point = Assert.Single(points);
		var payloadKeyValue = Assert.Single(point.Payload);
		Assert.Equal("bar", payloadKeyValue.Key);
		Assert.Equal("some bar", payloadKeyValue.Value.StringValue);
	}

	[Fact]
	public async Task ClearPayload()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.ClearPayloadAsync("collection_1", new[] { 9ul });

		var points = await _client.RetrieveAsync("collection_1", new[] { new PointId { Num = 9 } });

		var point = Assert.Single(points);
		Assert.Empty(point.Payload);
	}

	[Fact]
	public async Task CreateFieldIndex()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.CreatePayloadIndexAsync("collection_1", "foo");
	}

	[Fact]
	public async Task DeleteFieldIndex()
	{
		await CreateAndSeedCollection("collection_1");
		await _client.CreatePayloadIndexAsync("collection_1", "foo");

		await _client.DeletePayloadIndexAsync("collection_1", "foo");
	}

	[Fact]
	public async Task Search()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.SearchAsync(
			"collection_1",
			new[] { 10.4f, 11.4f },
			limit: 1);

		var point = Assert.Single(points);

		Assert.Equal(9ul, point.Id);
		var payloadKeyValue = Assert.Single(point.Payload);
		Assert.Equal("foo", payloadKeyValue.Key);
		Assert.Equal("goodbye", payloadKeyValue.Value.StringValue);
	}

	[Fact]
	public async Task SearchBatch()
	{
		await CreateAndSeedCollection("collection_1");

		var batchResults = await _client.SearchBatchAsync(
			"collection_1",
			new SearchPoints[]
			{
				// TODO: It shouldn't be necessary to specify CollectionName here, see
				// https://github.com/qdrant/qdrant-dotnet/pull/11/files#r1366050585
				new() { CollectionName = "collection_1", Vector = { 10.4f, 11.4f }, Limit = 1 },
				new() { CollectionName = "collection_1", Vector = { 3.4f, 4.4f }, Limit = 1 }
			});

		Assert.Collection(
			batchResults,
			br => Assert.Equal(9ul, Assert.Single(br.Result).Id),
			br => Assert.Equal(8ul, Assert.Single(br.Result).Id));
	}

	[Fact]
	public async Task SearchGroups()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.UpsertAsync("collection_1", new[]
		{
			new PointStruct
			{
				Id = new PointId { Num = 10 },
				Vectors = new() { Vector = new() { Data = { 30f, 31f }} },
				Payload = { ["foo"] = "hello" }
			}
		});

		var groups = await _client.SearchGroupsAsync(
			"collection_1",
			new[] { 10.4f, 11.4f },
			groupBy: "foo",
			groupSize: 2);

		Assert.Equal(2, groups.Count);
		Assert.Single(groups, g => g.Hits.Count == 2);
		Assert.Single(groups, g => g.Hits.Count == 1);
	}

	[Fact]
	public async Task Scroll()
	{
		await CreateAndSeedCollection("collection_1");

		var results = await _client.ScrollAsync(
			"collection_1",
			limit: 1);

		Assert.Single(results.Result);
		Assert.NotNull(results.NextPageOffset);

		results = await _client.ScrollAsync(
			"collection_1",
			limit: 1,
			offset: results.NextPageOffset);

		Assert.Single(results.Result);
		Assert.Null(results.NextPageOffset);
	}

	[Fact]
	public async Task Recommend()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.RecommendAsync("collection_1", positive: new PointId[] { 8 }, limit: 1);

		var point = Assert.Single(points);
		Assert.Equal(9ul, point.Id);
	}

	[Fact]
	public async Task RecommendBatch()
	{
		await CreateAndSeedCollection("collection_1");

		var batchResults = await _client.RecommendBatchAsync(
			"collection_1",
			new RecommendPoints[]
			{
				// TODO: It shouldn't be necessary to specify CollectionName here, see
				// https://github.com/qdrant/qdrant-dotnet/pull/11/files#r1366050585
				new() { CollectionName = "collection_1", Positive = { 8 }, Limit = 1 },
				new() { CollectionName = "collection_1", Positive = { 9 }, Limit = 1 }
			});

		Assert.Collection(
			batchResults,
			br => Assert.Equal(9ul, Assert.Single(br.Result).Id),
			br => Assert.Equal(8ul, Assert.Single(br.Result).Id));
	}

	[Fact]
	public async Task RecommendGroups()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.UpsertAsync("collection_1", new[]
		{
			new PointStruct
			{
				Id = new PointId { Num = 10 },
				Vectors = new() { Vector = new() { Data = { 30f, 31f }} },
				Payload = { ["foo"] = "hello" }
			}
		});

		var groups = await _client.RecommendGroupsAsync(
			"collection_1",
			groupBy: "foo",
			positive: new PointId[] { 9 },
			groupSize: 2);

		Assert.Equal(1, groups.Count);
		Assert.Single(groups, g => g.Hits.Count == 2);
	}

	[Fact]
	public async Task Count()
	{
		await CreateAndSeedCollection("collection_1");

		Assert.Equal(2ul, await _client.CountAsync("collection_1"));
	}

	[Fact]
	public async Task Count_with_filter()
	{
		await CreateAndSeedCollection("collection_1");

		var count = await _client.CountAsync(
			"collection_1",
			new Filter
			{
				Must = { new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 9 } } } } }
			});

		Assert.Equal(1ul, count);
	}

	private async Task CreateAndSeedCollection(string collection)
	{
		await _client.CreateCollectionAsync(collection, new VectorParams { Size = 2, Distance = Distance.Cosine });

		await _client.UpsertAsync(collection, new[]
		{
			new PointStruct
			{
				Id = new PointId { Num = 8 },
				Vectors = new() { Vector = new() { Data = { 3.5f, 4.5f }} },
				Payload = { ["foo"] = "hello" }
			},
			new PointStruct
			{
				Id = new PointId { Num = 9 },
				Vectors = new() { Vector = new() { Data = { 10.5f, 11.5f } } },
				Payload = { ["foo"] = "goodbye" }
			}
		});
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
