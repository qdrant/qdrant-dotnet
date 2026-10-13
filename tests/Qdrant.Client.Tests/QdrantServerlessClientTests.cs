using FluentAssertions;
using Grpc.Core;
using Qdrant.Client.Grpc;
using Xunit;
using static Qdrant.Client.Grpc.Conditions;
using Serverless = Qdrant.Client.Grpc.Serverless;

namespace Qdrant.Client;

/// <summary>
/// Runs against the serverless space at QDRANT_SERVERLESS_URL. Skipped if it is not configured.
/// </summary>
public sealed class QdrantServerlessClientTests : IAsyncLifetime
{
	private static readonly string? Url = Environment.GetEnvironmentVariable("QDRANT_SERVERLESS_URL");
	private static readonly string? ApiKey = Environment.GetEnvironmentVariable("QDRANT_SERVERLESS_API_KEY");

	// CI sets unavailable secrets to empty strings, e.g. on pull requests from forks.
	private static readonly bool Configured = !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(ApiKey);

	private readonly string _collectionName = $"dotnet-client-test-{Guid.NewGuid():N}";
	private QdrantServerlessClient? _client;

	private QdrantServerlessClient Client => _client!;

	public Task InitializeAsync()
	{
		if (Configured)
			_client = new QdrantServerlessClient(new System.Uri(Url!).Host, ApiKey, TimeSpan.FromSeconds(30));
		return Task.CompletedTask;
	}

	public async Task DisposeAsync()
	{
		if (_client is null)
			return;
		try
		{
			await _client.DeleteCollectionAsync(_collectionName);
		}
		finally
		{
			_client.Dispose();
		}
	}

	[ServerlessFact]
	public async Task Collections()
	{
		var config = new Serverless.CollectionConfig
		{
			DenseVectors =
			{
				[""] = new Serverless.DenseVectorConfig
				{
					Size = 4, Distance = Serverless.Distance.Cosine, PrecisionTier = Serverless.PrecisionTier.Full
				}
			},
			SparseVectors = { ["bm25"] = new Serverless.SparseVectorConfig { UseIdf = true } },
			PayloadIndexes =
			{
				["color"] = new Serverless.KeywordIndex(),
				["age"] = new Serverless.IntegerIndex { Range = false },
				["price"] = new Serverless.FloatIndex(),
				["user_id"] = new Serverless.UuidIndex(),
				["created"] = new Serverless.DatetimeIndex(),
				["geo"] = new Serverless.GeoIndex(),
				["active"] = new Serverless.BoolIndex(),
				["title"] = new Serverless.TextIndex
				{
					Tokenizer = Serverless.Tokenizer.Word,
					Stopwords = new Serverless.StopwordsSet { Languages = { "english" } },
					Stemmer = new Serverless.SnowballParams { Language = "english" }
				},
				["body"] = new Serverless.TextIndex { Stemmer = new Serverless.DisabledStemmer() },
			}
		};

		var created = await Client.CreateCollectionAsync(_collectionName, config);
		created.Result.Should().Be("created");

		// The API may report an existing collection as an error or through the result.
		try
		{
			var again = await Client.CreateCollectionAsync(_collectionName, config);
			again.Result.Should().NotBe("created");
		}
		catch (RpcException e)
		{
			e.StatusCode.Should().Be(StatusCode.AlreadyExists);
		}

		var info = await Client.GetCollectionAsync(_collectionName);
		info.Exists.Should().BeTrue();
		info.Config.Should().Be(config);
		(await Client.CollectionExistsAsync(_collectionName)).Should().BeTrue();
		(await Client.GetCollectionAsync(_collectionName + "-missing")).Exists.Should().BeFalse();

		var names = new List<string>();
		var request = new Serverless.ListCollectionsRequest { Limit = 100 };
		Serverless.ListCollectionsResponse page;
		do
		{
			page = await Client.ListCollectionsAsync(request);
			names.AddRange(page.Collections.Select(c => c.CollectionName));
			request.OffsetToken = page.NextOffsetToken;
		} while (page.HasNextOffsetToken);
		names.Should().Contain(_collectionName);
	}

	[ServerlessFact]
	public async Task Points()
	{
		await Client.CreateCollectionAsync(_collectionName, new Serverless.CollectionConfig
		{
			DenseVectors = { [""] = new Serverless.DenseVectorConfig { Size = 4, Distance = Serverless.Distance.Dot } },
			PayloadIndexes = { ["color"] = new Serverless.KeywordIndex() },
		});

		await Client.UpsertAsync(new UpsertPoints
		{
			CollectionName = _collectionName,
			Wait = true,
			Points =
			{
				new PointStruct { Id = 1, Vectors = new[] { 1f, 0, 0, 0 }, Payload = { ["color"] = "red" } },
				new PointStruct { Id = 2, Vectors = new[] { 0f, 1, 0, 0 }, Payload = { ["color"] = "blue" } },
			}
		});

		// Reads can lag a few seconds behind writes.
		await Eventually(async () =>
			await Client.CountAsync(new CountPoints
			{
				CollectionName = _collectionName,
				Exact = true,
				Filter = MatchKeyword("color", "red")
			}) == 1);

		await Eventually(async () =>
		{
			var hits = await Client.QueryAsync(new QueryPoints
			{
				CollectionName = _collectionName,
				Query = new[] { 1f, 0, 0, 0 }
			});
			return hits.Count == 2 && hits[0].Id.Num == 1;
		});

		var setPayload = await Client.SetPayloadAsync(new SetPayloadPoints
		{
			CollectionName = _collectionName,
			Wait = true,
			PointsSelector = 1,
			Payload = { ["size"] = 10 }
		});
		setPayload.Status.Should().Be(UpdateStatus.Completed);

		var points = await Client.RetrieveAsync(new GetPoints
		{
			CollectionName = _collectionName,
			Ids = { 1 },
			WithPayload = true
		});
		points.Should().HaveCount(1);

		var delete = await Client.DeleteAsync(new DeletePoints { CollectionName = _collectionName, Wait = true, Points = 2 });
		delete.Status.Should().Be(UpdateStatus.Completed);
	}

	// Retries on false and on errors, e.g. while a new collection is not readable yet.
	private static async Task Eventually(Func<Task<bool>> condition)
	{
		var deadline = DateTime.UtcNow.AddMinutes(1);
		while (true)
		{
			try
			{
				if (await condition())
					return;
			}
			catch (RpcException) when (DateTime.UtcNow < deadline)
			{
			}

			DateTime.UtcNow.Should().BeBefore(deadline, "the condition should be met within 1 minute");
			await Task.Delay(250);
		}
	}

	private sealed class ServerlessFactAttribute : FactAttribute
	{
		public ServerlessFactAttribute()
		{
			if (!Configured)
				Skip = "QDRANT_SERVERLESS_URL and QDRANT_SERVERLESS_API_KEY are not set";
		}
	}
}
