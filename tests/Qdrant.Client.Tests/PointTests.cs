using FluentAssertions;
using Qdrant.Client.Grpc;
using Xunit;
using static Qdrant.Client.Grpc.Conditions;

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

		var points = await _client.RetrieveAsync("collection_1", 9);
		points.Should().HaveCount(1);
		var point = points.Single();
		point.Id.Should().Be((PointId)9ul);
		point.Payload.Should().ContainKeys("foo", "bar");
		point.Payload["foo"].Should().Be((Value)"goodbye");
		point.Payload["bar"].Should().Be((Value)2);
		point.Vectors.Should().BeNull();
	}

	[Fact]
	public async Task Retrieve_with_vector_without_payload()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.RetrieveAsync("collection_1", 8, withPayload: false, withVectors: true);

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

		var points = await _client.RetrieveAsync("collection_1", 9);

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

		var points = await _client.RetrieveAsync("collection_1", 9);

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

		await _client.DeletePayloadAsync("collection_1", new[] { "foo" }, 9);

		var points = await _client.RetrieveAsync("collection_1", 9);

		var point = Assert.Single(points);
		var payloadKeyValue = Assert.Single(point.Payload);
		Assert.Equal("bar", payloadKeyValue.Key);
		Assert.Equal("some bar", payloadKeyValue.Value.StringValue);
	}

	[Fact]
	public async Task ClearPayload()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.ClearPayloadAsync("collection_1", 9);

		var points = await _client.RetrieveAsync("collection_1", 9);

		var point = Assert.Single(points);
		Assert.Empty(point.Payload);
	}

	[Fact]
	public async Task CreateFieldIndex()
	{
		await CreateAndSeedCollection("collection_1");

		var result = await _client.CreatePayloadIndexAsync("collection_1", "foo");
		result.Status.Should().Be(UpdateStatus.Completed);

		result = await _client.CreatePayloadIndexAsync("collection_1", "bar", PayloadSchemaType.Integer);
		result.Status.Should().Be(UpdateStatus.Completed);

		var collectionInfo = await _client.GetCollectionInfoAsync("collection_1");
		collectionInfo.PayloadSchema.Should().ContainKeys("foo", "bar");
		collectionInfo.PayloadSchema["foo"].DataType.Should().Be(PayloadSchemaType.Keyword);
		collectionInfo.PayloadSchema["bar"].DataType.Should().Be(PayloadSchemaType.Integer);
	}

	[Fact]
	public async Task DeleteFieldIndex()
	{
		await CreateAndSeedCollection("collection_1");

		var result = await _client.CreatePayloadIndexAsync("collection_1", "foo");
		result.Status.Should().Be(UpdateStatus.Completed);

		result = await _client.DeletePayloadIndexAsync("collection_1", "foo");
		result.Status.Should().Be(UpdateStatus.Completed);
	}

	[Fact]
	public async Task QueryNearest()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: new float[] { 10.4f, 11.4f },
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
	public async Task QueryBatch()
	{
		await CreateAndSeedCollection("collection_1");

		var batchResults = await _client.QueryBatchAsync(
			"collection_1",
			new QueryPoints[]
			{
				new() { Query = new float[] { 10.4f, 11.4f }, Limit = 1 },
				new() { Query = new float[] { 3.4f, 4.4f }, Limit = 1 }
			});

		Assert.Collection(
			batchResults,
			br => Assert.Equal(9ul, Assert.Single(br.Result).Id),
			br => Assert.Equal(8ul, Assert.Single(br.Result).Id));
	}

	[Fact]
	public async Task QueryNearestGroups()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.UpsertAsync("collection_1", new[]
		{
			new PointStruct
			{
				Id = 10,
				Vectors = new[] { 30f, 31f },
				Payload = { ["foo"] = "hello" }
			}
		});

		var groups = await _client.QueryGroupsAsync(
			"collection_1",
			groupBy: "foo",
			query: new float[] { 10.4f, 11.4f },
			groupSize: 2);

		Assert.Equal(2, groups.Count);
		Assert.Single(groups, g => g.Hits.Count == 2);
		Assert.Single(groups, g => g.Hits.Count == 1);
	}

	[Fact]
	public async Task Scroll()
	{
		await CreateAndSeedCollection("collection_1");

		var results = await _client.ScrollAsync("collection_1", limit: 1);

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
	public async Task QueryRecommend()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: new RecommendInput { Positive = { (VectorInput)8ul } },
			limit: 1);

		var point = Assert.Single(points);
		Assert.Equal(9ul, point.Id);
	}

	[Fact]
	public async Task QueryRecommendBatch()
	{
		await CreateAndSeedCollection("collection_1");

		var batchResults = await _client.QueryBatchAsync(
			"collection_1",
			new QueryPoints[]
			{
				new() { Query = new RecommendInput { Positive = { (VectorInput)8ul } }, Limit = 1 },
				new() { Query = new RecommendInput { Positive = { (VectorInput)9ul } }, Limit = 1 }
			});

		Assert.Collection(
			batchResults,
			br => Assert.Equal(9ul, Assert.Single(br.Result).Id),
			br => Assert.Equal(8ul, Assert.Single(br.Result).Id));
	}

	[Fact]
	public async Task QueryRecommendGroups()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.UpsertAsync("collection_1", new[]
		{
			new PointStruct
			{
				Id = 10,
				Vectors = new[] { 30f, 31f },
				Payload = { ["foo"] = "hello" }
			}
		});

		var groups = await _client.QueryGroupsAsync(
			"collection_1",
			groupBy: "foo",
			query: new RecommendInput { Positive = { (VectorInput)9ul } },
			groupSize: 2);

		Assert.Single(groups);
		Assert.Single(groups, g => g.Hits.Count == 2);
	}

	[Fact]
	public async Task QueryRecommend_with_vector()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: new RecommendInput
			{
				Positive =
				{
					(VectorInput)8ul,
					(VectorInput)new float[] { 3.5f, 4.5f }
				}
			},
			limit: 1
		);

		var point = Assert.Single(points);
		Assert.Equal(9ul, point.Id);
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
			HasId(9) & MatchKeyword("foo", "goodbye")
		);

		Assert.Equal(1ul, count);
	}

	[Fact]
	public async Task UpdateBatch()
	{
		await CreateAndSeedCollection("collection_1");

		var operations = new PointsUpdateOperation[]
		{
			new()
			{
				Upsert = new()
				{
					Points =
					{
						new PointStruct[]
						{
							new() { Id = 1, Vectors = new float[] { 0.3f, 1.5f } }
						}
					}
				}
			},
			new()
			{
				UpdateVectors = new()
				{
					Points =
					{
						new PointVectors[]
						{
							new() { Id = 1, Vectors = new float[] { 10.5f, 11.5f } }
						}
					}
				}
			}
		};

		var response = await _client.UpdateBatchAsync("collection_1", operations);

		Assert.All(response, r => r.Status.Should().Be(UpdateStatus.Completed));
	}

	private async Task CreateAndSeedCollection(string collection)
	{
		await _client.CreateCollectionAsync(collection, new VectorParams { Size = 2, Distance = Distance.Cosine });

		var updateResult = await _client.UpsertAsync(collection, new[]
		{
			new PointStruct
			{
				Id = 8,
				Vectors = new[] { 3.5f, 4.5f },
				Payload =
				{
					["foo"] = "hello",
					["bar"] = 1
				}
			},
			new PointStruct
			{
				Id = 9,
				Vectors = new[] { 10.5f, 11.5f },
				Payload =
				{
					["foo"] = "goodbye",
					["bar"] = 2
				}
			}
		});

		updateResult.Status.Should().Be(UpdateStatus.Completed);
	}

	private static PointStruct TextPoint(ulong id, string field, string text) => new()
	{
		Id = id,
		Vectors = new[] { 0.1f, 0.2f, 0.3f, 0.4f },
		Payload = { [field] = text }
	};

	public async Task InitializeAsync()
	{
		foreach (var collection in await _client.ListCollectionsAsync())
		{
			await _client.DeleteCollectionAsync(collection);
		}
	}

	[Fact]
	public async Task Query()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync("collection_1");

		Assert.Equal(2, points.Count);
		Assert.Equal(8ul, points[0].Id);
		Assert.Equal(9ul, points[1].Id);

		points = await _client.QueryAsync("collection_1", limit: 1);

		var point = Assert.Single(points);
		Assert.Equal(8ul, point.Id);
	}

	[Fact]
	public async Task QueryWithFilter()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync("collection_1", filter: MatchKeyword("foo", "hello"));

		var point = Assert.Single(points);
		Assert.Equal(8ul, point.Id);
	}

	[Fact]
	public async Task QueryWithNearestId()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync("collection_1", query: 9ul);

		var point = Assert.Single(points);
		Assert.Equal(8ul, point.Id);
	}

	[Fact]
	public async Task QueryWithVector()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync("collection_1");

		Assert.Equal(2, points.Count);
		Assert.Equal(8ul, points[0].Id);
		Assert.Equal(9ul, points[1].Id);

		points = await _client.QueryAsync("collection_1", query: new float[] { 10.5f, 11.5f });

		Assert.Equal(2, points.Count);
		Assert.Equal(9ul, points[0].Id);
	}

	[Fact]
	public async Task QueryOrderBy()
	{
		await CreateAndSeedCollection("collection_1");

		var status = await _client.CreatePayloadIndexAsync(
			collectionName: "collection_1",
			fieldName: "bar",
			schemaType: PayloadSchemaType.Integer,
			indexParams: new PayloadIndexParams
			{
				IntegerIndexParams = new() { Lookup = false, Range = true }
			}
		);
		Assert.Equal(UpdateStatus.Completed, status.Status);

		var points = await _client.QueryAsync("collection_1", query: (OrderBy)"bar", limit: 1);

		var point = Assert.Single(points);
		Assert.Equal(8ul, point.Id);
	}

	[Fact]
	public async Task QueryWithPrefetchLimit()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: new float[] { 10.5f, 11.5f },
			prefetch: new List<PrefetchQuery> { new() { Limit = 1 } }
		);

		Assert.Single(points);
	}

	[Fact]
	public async Task QueryWithPrefetchAndFusion()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: Fusion.Rrf,
			prefetch: new List<PrefetchQuery>
			{
				new() { Query = new float[] { 10.5f, 11.5f } },
				new() { Query = new float[] { 3.5f, 4.5f } }
			}
		);

		Assert.Equal(2, points.Count);
	}

	[Fact]
	public async Task QueryWithSampling()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: Sample.Random,
			limit: 2
		);

		Assert.Equal(2, points.Count);
	}

	[Fact]
	public async Task QueryFormula()
	{
		await CreateAndSeedCollection("collection_1");

		var points = await _client.QueryAsync(
			"collection_1",
			query: new Formula
			{
				Expression = new SumExpression
				{
					Sum =
					{
						new MaxExpression { Max = { "$score", 2f } },
						new MinExpression { Min = { "$score", 0f } },
						Expression.FromAcosh(1f)
					}
				}
			},
			prefetch: new List<PrefetchQuery> { new() { Query = new float[] { 3.5f, 4.5f } } }
		);

		Assert.Equal(2, points.Count);
		Assert.All(points, p => p.Score.Should().BeApproximately(2f, 1e-6f));
	}

	[Fact]
	public async Task QueryText()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Dot });

		var status = await _client.CreatePayloadIndexAsync(
			"collection_1",
			fieldName: "title",
			schemaType: PayloadSchemaType.Text,
			indexParams: new PayloadIndexParams
			{
				TextIndexParams = new()
				{
					Tokenizer = TokenizerType.Word,
					Scoring = new TextScoringParams { Type = TextScoringType.Bm25 }
				}
			}
		);
		Assert.Equal(UpdateStatus.Completed, status.Status);

		await _client.UpsertAsync("collection_1", new[]
		{
			TextPoint(1, "title", "vector search engine"),
			TextPoint(2, "title", "search search search"),
			TextPoint(3, "title", "relational database")
		});

		var points = await _client.QueryAsync(
			"collection_1",
			query: new TextQuery { Query = "vector search" },
			usingVector: "title");

		Assert.Equal(2, points.Count);
		Assert.Equal(1ul, points[0].Id);
		Assert.Equal(2ul, points[1].Id);

		var tuned = await _client.QueryAsync(
			"collection_1",
			query: new TextQuery { Query = "vector search", Bm25 = new Bm25Params { K = 2f, B = 0.5f } },
			usingVector: "title");

		Assert.Equal(2, tuned.Count);
		Assert.NotEqual(points[0].Score, tuned[0].Score);
	}

	[Fact]
	public async Task ScrollWithSubstringMatch()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Dot });
		await _client.CreatePayloadIndexAsync("collection_1", fieldName: "sku", schemaType: PayloadSchemaType.Keyword);

		await _client.UpsertAsync("collection_1", new[]
		{
			TextPoint(1, "sku", "AB-1234-XY"),
			TextPoint(2, "sku", "CD-5678-XY")
		});

		var response = await _client.ScrollAsync("collection_1", filter: MatchSubstring("sku", "1234"));

		var point = Assert.Single(response.Result);
		Assert.Equal(1ul, point.Id);
	}

	[Fact]
	public async Task ScrollWithCodeTokenizer()
	{
		await _client.CreateCollectionAsync("collection_1", new VectorParams { Size = 4, Distance = Distance.Dot });
		await _client.CreatePayloadIndexAsync(
			"collection_1",
			fieldName: "source",
			schemaType: PayloadSchemaType.Text,
			indexParams: new PayloadIndexParams { TextIndexParams = new() { Tokenizer = TokenizerType.Code } }
		);

		await _client.UpsertAsync("collection_1", new[]
		{
			TextPoint(10, "source", "fn getUserName() {}"),
			TextPoint(11, "source", "fn deleteFile() {}")
		});

		var response = await _client.ScrollAsync("collection_1", filter: MatchText("source", "get_user_name"));

		var point = Assert.Single(response.Result);
		Assert.Equal(10ul, point.Id);
	}

	[Fact]
	public async Task CreateNestedFieldIndex()
	{
		await CreateAndSeedCollection("collection_1");

		var status = await _client.CreatePayloadIndexAsync(
			"collection_1",
			fieldName: "offers[].price",
			schemaType: PayloadSchemaType.Float,
			indexParams: new PayloadIndexParams { FloatIndexParams = new() { Nested = "offers" } }
		);

		Assert.Equal(UpdateStatus.Completed, status.Status);
	}

	[Fact]
	public async Task QueryGroups()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.UpsertAsync("collection_1", new[]
		{
			new PointStruct
			{
				Id = 10,
				Vectors = new[] { 30f, 31f },
				Payload = { ["foo"] = "hello" }
			}
		});

		// 3 points in total, 2 with "foo" = "hello" and 1 with "foo" = "goodbye"

		var groups = await _client.QueryGroupsAsync(
			"collection_1",
			"foo",
			new[] { 10.4f, 11.4f },
			groupSize: 2);

		Assert.Equal(2, groups.Count);
		// A group 2 hits because of 2 points with "foo" = "hello"
		Assert.Single(groups, g => g.Hits.Count == 2);
		// A group with 1 hit because of 1 point with "foo" = "goodbye"
		Assert.Single(groups, g => g.Hits.Count == 1);
	}

	[Fact]
	public async Task Facet()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.CreatePayloadIndexAsync(
			"collection_1",
			fieldName: "foo",
			schemaType: PayloadSchemaType.Keyword
		);

		await _client.FacetAsync(
			"collection_1",
			filter: MatchKeyword("bar", "hello"),
			key: "foo",
			limit: 2
		);
	}

	[Fact]
	public async Task SearchMatrix()
	{
		await CreateAndSeedCollection("collection_1");

		await _client.SearchMatrixPairsAsync(
			"collection_1",
			filter: MatchKeyword("bar", "hello"),
			sample: 3,
			limit: 2
		);

		await _client.SearchMatrixOffsetsAsync(
			"collection_1",
			filter: MatchKeyword("bar", "hello"),
			sample: 3,
			limit: 2
		);
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
