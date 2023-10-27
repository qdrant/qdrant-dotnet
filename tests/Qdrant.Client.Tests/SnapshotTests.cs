using FluentAssertions;
using Grpc.Core;
using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

[Collection("Qdrant")]
public class SnapshotTests : IAsyncLifetime
{
	private readonly QdrantClient _client;

	public SnapshotTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateClient();

	[Fact]
	public async Task CreateSnapshot()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateSnapshotAsync("collection_1");
	}

	[Fact]
	public async Task DeleteSnapshot()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		var createSnapshotResponse = await _client.CreateSnapshotAsync("collection_1");
		await _client.DeleteSnapshotAsync("collection_1", createSnapshotResponse.SnapshotDescription.Name);
	}

	[Fact]
	public Task DeleteSnapshot_with_missing_snapshot() =>
		Assert.ThrowsAsync<RpcException>(() =>
			_client.DeleteSnapshotAsync("collection_1", "snapshot_1"));


	[Fact]
	public async Task ListSnapshots()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateSnapshotAsync("collection_1");
		await _client.CreateSnapshotAsync("collection_1");

		var listSnapshotsResponse = await _client.ListSnapshotsAsync("collection_1");
		listSnapshotsResponse.SnapshotDescriptions.Should().HaveCount(2);
	}

	[Fact]
	public async Task CreateFullSnapshot()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateCollectionAsync("collection_2", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateFullSnapshotAsync();
	}

	[Fact]
	public async Task DeleteFullSnapshot()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateCollectionAsync("collection_2", new VectorParams { Size = 4, Distance = Distance.Cosine });
		var createSnapshotResponse = await _client.CreateFullSnapshotAsync();
		await _client.DeleteFullSnapshotAsync(createSnapshotResponse.SnapshotDescription.Name);
	}

	[Fact]
	public Task DeleteFullSnapshot_with_missing_snapshot() =>
		Assert.ThrowsAsync<RpcException>(() => _client.DeleteFullSnapshotAsync("snapshot_1"));

	[Fact]
	public async Task ListFullSnapshots()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateCollectionAsync("collection_2", new VectorParams { Size = 4, Distance = Distance.Cosine });
		await _client.CreateFullSnapshotAsync();
		await _client.CreateFullSnapshotAsync();

		var listSnapshotsResponse = await _client.ListFullSnapshotsAsync();
		listSnapshotsResponse.SnapshotDescriptions.Should().HaveCount(2);
	}

	public async Task InitializeAsync()
	{
		foreach (var collection in await _client.ListCollectionsAsync())
		{
			var listSnapshotsResponse = await _client.ListSnapshotsAsync(collection);
			foreach (var snapshot in listSnapshotsResponse.SnapshotDescriptions.Select(s => s.Name))
			{
				await _client.DeleteSnapshotAsync(collection, snapshot);
			}

			await _client.DeleteCollectionAsync(collection);
		}

		var listFullSnapshotsResponse = await _client.ListFullSnapshotsAsync();
		foreach (var snapshot in listFullSnapshotsResponse.SnapshotDescriptions.Select(s => s.Name))
		{
			await _client.DeleteFullSnapshotAsync(snapshot);
		}
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
