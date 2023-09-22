using FluentAssertions;
using Grpc.Core;
using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

[Collection("Qdrant")]
public class CollectionTests : IAsyncLifetime
{
	private readonly QdrantClient _client;

	public CollectionTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateClient();

	[Fact]
	public Task CreateCollection()
		=> _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });

	[Fact]
	public async Task ListCollections()
	{
		Assert.Empty(await _client.ListCollectionsAsync());

		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateCollectionAsync("collection_2", new VectorParams { Size = 4, Distance = Distance.Cosine });

		var collections = await _client.ListCollectionsAsync();

		Assert.Collection(
			collections.OrderBy(c => c),
			c => c.Should().Be("collection_1"),
			c => c.Should().Be("collection_2"));
	}

	[Fact]
	public async Task DeleteCollection()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.DeleteCollectionAsync("collection_1");

		Assert.Empty(await _client.ListCollectionsAsync());
	}

	[Fact]
	public Task Delete_with_missing_collection()
		=> Assert.ThrowsAsync<QdrantException>(() => _client.DeleteCollectionAsync("collection_1"));

	[Fact]
	public async Task UpdateCollection()
	{
		await _client.CreateCollectionAsync("collection_1",
			new VectorParams { Size = 4, Distance = Distance.Cosine, OnDisk = true });

		await _client.UpdateCollectionAsync("collection_1", new VectorParamsDiff { OnDisk = false });
	}

	[Fact]
	public async Task UpdateCollection_with_no_changes()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.UpdateCollectionAsync("collection_1");
	}

	[Fact]
	public async Task GetInfo()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });

		var info = await _client.GetCollectionInfoAsync("collection_1");

		info.Status.Should().Be(CollectionStatus.Green);
		// TODO: Complete
	}

	[Fact]
	public async Task GetInfo_with_missing_collection()
	{
		// TODO: Possibly translate this RpcException to a QdrantException (as e.g. Delete throws)
		var exception = await Assert.ThrowsAsync<RpcException>(() => _client.GetCollectionInfoAsync("collection_1"));
		exception.StatusCode.Should().Be(StatusCode.NotFound);
	}

	[Fact]
	public async Task CreateAlias()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateAliasAsync("alias_1", "collection_1");
	}

	[Fact]
	public async Task ListAliases()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateAliasAsync("alias_1", "collection_1");
		await _client.CreateAliasAsync("alias_2", "collection_1");

		var aliasDescriptions = await _client.ListAliasesAsync();

		Assert.Collection(
			aliasDescriptions.OrderBy(d => d.AliasName),
			d =>
			{
				d.AliasName.Should().Be("alias_1");
				d.CollectionName.Should().Be("collection_1");
			},
			d =>
			{
				d.AliasName.Should().Be("alias_2");
				d.CollectionName.Should().Be("collection_1");
			});
	}

	[Fact]
	public async Task ListCollectionAliases()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateAliasAsync("alias_1", "collection_1");
		await _client.CreateAliasAsync("alias_2", "collection_1");

		var aliases = await _client.ListCollectionAliasesAsync("collection_1");

		Assert.Collection(
			aliases.OrderBy(a => a),
			a => a.Should().Be("alias_1"),
			a => a.Should().Be("alias_2"));
	}

	[Fact]
	public async Task RenameAlias()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateAliasAsync("alias_1", "collection_1");

		await _client.RenameAliasAsync("alias_1", "alias_2");

		var aliases = await _client.ListCollectionAliasesAsync("collection_1");

		Assert.Single(aliases).Should().Be("alias_2");
	}

	[Fact]
	public async Task DeleteAlias()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateAliasAsync("alias_1", "collection_1");

		await _client.DeleteAliasAsync("alias_1");

		Assert.Empty(await _client.ListCollectionAliasesAsync("collection_1"));
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
