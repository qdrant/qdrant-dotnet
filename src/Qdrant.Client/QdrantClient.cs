using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Qdrant.Client.Grpc;

namespace Qdrant.Client;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class QdrantClient
{
	private readonly Collections.CollectionsClient _collectionsClient;
	private readonly ILogger _logger;

	public QdrantClient(QdrantGrpcClient grpcClient, ILoggerFactory? loggerFactory = null)
	{
		_collectionsClient = grpcClient.Collections;

		_logger = loggerFactory?.CreateLogger("Qdrant.Client") ?? NullLogger.Instance;
	}

	/// <summary>
	/// Creates a new collection with the given parameters.
	/// </summary>
	/// <param name="collectionName">The name of the collection to be created.</param>
	/// <param name="vectorsConfig">
	/// Configuration of the vector storage. Vector params contains size and distance for the vector storage.
	/// This overload creates a single anonymous vector storage.
	/// </param>
	/// <param name="shardNumber">Number of shards in collection. Default is 1, minimum is 1.</param>
	/// <param name="replicationFactor">
	/// Replication factor for collection. Default is 1, minimum is 1.
	/// Defines how many copies of each shard will be created. Has an effect only in distributed mode.
	/// </param>
	/// <param name="writeConsistencyFactor">
	/// Write consistency factor for collection. Default is 1, minimum is 1.
	/// Defines how many replicas should apply the operation for us to consider it successful.
	/// Increasing this number will make the collection more resilient to inconsistencies, but will also make it fail if
	/// not enough replicas are available. Does not have any performance impact. Has an effect only in distributed mode.
	/// </param>
	/// <param name="onDiskPayload">
	/// If true - point`s payload will not be stored in memory. It will be read from the disk every time it is
	/// requested. This setting saves RAM by (slightly) increasing the response time.
	/// Note: those payload values that are involved in filtering and are indexed - remain in RAM.
	/// </param>
	/// <param name="hnswConfig">Params for HNSW index.</param>
	/// <param name="optimizersConfig">Params for optimizer.</param>
	/// <param name="walConfig">Params for Write-Ahead-Log.</param>
	/// <param name="quantizationConfig">
	/// Params for quantization, if <c>null</c> - quantization will be disabled.
	/// </param>
	/// <param name="initFromCollection">Use data stored in another collection to initialize this collection.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task CreateCollectionAsync(
		string collectionName,
		VectorParams vectorsConfig,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> CreateCollectionCoreAsync(
			collectionName, new VectorsConfig { Params = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, timeout, cancellationToken);

	/// <summary>
	/// Creates a new collection with the given parameters.
	/// </summary>
	/// <param name="collectionName">The name of the collection to be created.</param>
	/// <param name="vectorsConfig">
	/// Configuration of the vector storage. Vector params contains size and distance for the vector storage.
	/// This overload creates a vector storage for each key in the provided map.
	/// </param>
	/// <param name="shardNumber">Number of shards in collection. Default is 1, minimum is 1.</param>
	/// <param name="replicationFactor">
	/// Replication factor for collection. Default is 1, minimum is 1.
	/// Defines how many copies of each shard will be created. Has an effect only in distributed mode.
	/// </param>
	/// <param name="writeConsistencyFactor">
	/// Write consistency factor for collection. Default is 1, minimum is 1.
	/// Defines how many replicas should apply the operation for us to consider it successful.
	/// Increasing this number will make the collection more resilient to inconsistencies, but will also make it fail if
	/// not enough replicas are available. Does not have any performance impact. Has an effect only in distributed mode.
	/// </param>
	/// <param name="onDiskPayload">
	/// If true - point`s payload will not be stored in memory. It will be read from the disk every time it is
	/// requested. This setting saves RAM by (slightly) increasing the response time.
	/// Note: those payload values that are involved in filtering and are indexed - remain in RAM.
	/// </param>
	/// <param name="hnswConfig">Params for HNSW index.</param>
	/// <param name="optimizersConfig">Params for optimizer.</param>
	/// <param name="walConfig">Params for Write-Ahead-Log.</param>
	/// <param name="quantizationConfig">
	/// Params for quantization, if <c>null</c> - quantization will be disabled.
	/// </param>
	/// <param name="initFromCollection">Use data stored in another collection to initialize this collection.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task CreateCollectionAsync(
		string collectionName,
		VectorParamsMap vectorsConfig,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> CreateCollectionCoreAsync(
			collectionName, new VectorsConfig { ParamsMap = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, timeout, cancellationToken);

	// TODO: Several of the below are proto-generated objects (so no docs, NRT annotations...)
	private async Task CreateCollectionCoreAsync(
		string collectionName,
		VectorsConfig vectorsConfig,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		// TODO: pass timeout configured at the client level

		var request = new CreateCollection
		{
			CollectionName = collectionName,
			VectorsConfig = vectorsConfig,
			ShardNumber = shardNumber,
			ReplicationFactor = replicationFactor,
			WriteConsistencyFactor = writeConsistencyFactor,
			OnDiskPayload = onDiskPayload,
			HnswConfig = hnswConfig,
			OptimizersConfig = optimizersConfig,
			WalConfig = walConfig,
			QuantizationConfig = quantizationConfig
		};

		if (timeout is not null)
		{
			request.Timeout = ConvertTimeout(timeout);
		}

		if (initFromCollection is not null)
		{
			request.InitFromCollection = initFromCollection;
		}

		var response = await _collectionsClient.CreateAsync(request, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (!response.Result)
		{
			throw new QdrantException($"Collection '{collectionName}' could not be created");
		}

		_logger.CreateCollection(collectionName);
	}

	// TODO: RecreateCollectionAsync

	/// <summary>
	/// Gets detailed information about an existing collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<CollectionInfo> GetCollectionInfoAsync(
		string collectionName,
		CancellationToken cancellationToken = default)
	{
		var response = await _collectionsClient.GetAsync(
				new GetCollectionInfoRequest { CollectionName = collectionName }, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		_logger.GetCollectionInfo(collectionName);

		var grpcCollectionInfo = response.Result;

		return new CollectionInfo
		{
			Status = grpcCollectionInfo.Status,
			// TODO: OptimizerStatus
			VectorsCount = grpcCollectionInfo.VectorsCount,
			SegmentsCount = grpcCollectionInfo.SegmentsCount,
			// TODO: PayloadSchema
			PointsCount = grpcCollectionInfo.PointsCount,
			IndexedVectorsCount = grpcCollectionInfo.IndexedVectorsCount
		};
	}

	/// <summary>
	/// Gets the names of all existing collections.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default)
	{
		var response = await _collectionsClient
			.ListAsync(new ListCollectionsRequest(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		_logger.ListCollections();

		var names = new string[response.Collections.Count];
		for (var i = 0; i < names.Length; i++)
		{
			names[i] = response.Collections[i].Name;
		}
		return names;
	}

	/// <summary>
	/// Drop a collection and all its associated data.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
	{
		var response = await _collectionsClient
			.DeleteAsync(new DeleteCollection { CollectionName = collectionName }, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (!response.Result)
		{
			throw new QdrantException($"Collection '{collectionName}' could not be deleted");
		}

		_logger.DeleteCollection(collectionName);
	}

	/// <summary>
	/// Update parameters of the collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectorsConfig">
	/// Configuration of the vector storage. Vector params contains size and distance for the vector storage.
	/// This overload creates a single anonymous vector storage.
	/// </param>
	/// <param name="optimizersConfig">Params for optimizer.</param>
	/// <param name="collectionParams">The collection parameters.</param>
	/// <param name="hnswConfig">Params for HNSW index.</param>
	/// <param name="quantizationConfig">
	/// Params for quantization, if <c>null</c> - quantization will be disabled.
	/// </param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task UpdateCollectionAsync(
		string collectionName,
		VectorParamsDiff vectorsConfig,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateCollectionCoreAsync(collectionName, new VectorsConfigDiff { Params = vectorsConfig }, optimizersConfig,
			collectionParams, hnswConfig, quantizationConfig, timeout, cancellationToken);

	/// <summary>
	/// Update parameters of the collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectorsConfig">
	/// Configuration of the vector storage. Vector params contains size and distance for the vector storage.
	/// This overload creates a vector storage for each key in the provided map.
	/// </param>
	/// <param name="optimizersConfig">Params for optimizer.</param>
	/// <param name="collectionParams">The collection parameters.</param>
	/// <param name="hnswConfig">Params for HNSW index.</param>
	/// <param name="quantizationConfig">
	/// Params for quantization, if <c>null</c> - quantization will be disabled.
	/// </param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task UpdateCollectionAsync(
		string collectionName,
		VectorParamsDiffMap vectorsConfig,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateCollectionCoreAsync(collectionName, new VectorsConfigDiff { ParamsMap = vectorsConfig },
			optimizersConfig, collectionParams, hnswConfig, quantizationConfig, timeout, cancellationToken);

	/// <summary>
	/// Update parameters of the collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="optimizersConfig">Params for optimizer.</param>
	/// <param name="collectionParams">The collection parameters.</param>
	/// <param name="hnswConfig">Params for HNSW index.</param>
	/// <param name="quantizationConfig">
	/// Params for quantization, if <c>null</c> - quantization will be disabled.
	/// </param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task UpdateCollectionAsync(
		string collectionName,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateCollectionCoreAsync(collectionName, vectorsConfig: null, optimizersConfig, collectionParams,
			hnswConfig, quantizationConfig, timeout, cancellationToken);

	private async Task UpdateCollectionCoreAsync(
		string collectionName,
		VectorsConfigDiff? vectorsConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new UpdateCollection
		{
			CollectionName = collectionName,
			VectorsConfig = vectorsConfig,
			OptimizersConfig = optimizersConfig,
			Params = collectionParams,
			HnswConfig = hnswConfig,
			QuantizationConfig = quantizationConfig,
		};

		if (timeout is not null)
		{
			request.Timeout = ConvertTimeout(timeout);
		}

		var response = await _collectionsClient
			.UpdateAsync(request, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (!response.Result)
		{
			throw new QdrantException($"Collection '{collectionName}' could not be updated");
		}

		_logger.UpdateCollection(collectionName);
	}

	#region Alias management

	/// <summary>
	/// Creates an alias for a given collection.
	/// </summary>
	/// <param name="aliasName">The alias to be created.</param>
	/// <param name="collectionName">The collection for which the alias is to be created.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	public Task CreateAliasAsync(
		string aliasName,
		string collectionName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateAliasesAsync(
			new[]
			{
				new AliasOperations
				{
					CreateAlias = new CreateAlias { AliasName = aliasName, CollectionName = collectionName }
				}
			}, timeout, cancellationToken);

	/// <summary>
	/// Renames an existing alias.
	/// </summary>
	/// <param name="oldAliasName">The old alias name.</param>
	/// <param name="newAliasName">The new alias name.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	public Task RenameAliasAsync(
		string oldAliasName,
		string newAliasName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateAliasesAsync(
			new[]
			{
				new AliasOperations
				{
					RenameAlias = new RenameAlias { OldAliasName = oldAliasName, NewAliasName = newAliasName }
				}
			}, timeout, cancellationToken);

	/// <summary>
	/// Deletes an alias.
	/// </summary>
	/// <param name="aliasName">The alias to be deleted.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	public Task DeleteAliasAsync(
		string aliasName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateAliasesAsync(
			new[]
			{
				new AliasOperations
				{
					DeleteAlias = new DeleteAlias { AliasName = aliasName }
				}
			}, timeout, cancellationToken);

	/// <summary>
	/// Update the aliases of existing collections.
	/// </summary>
	/// <param name="aliasOperations">The list of operations to perform.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	public async Task UpdateAliasesAsync(
		IReadOnlyList<AliasOperations> aliasOperations,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new ChangeAliases();

		if (timeout is not null)
		{
			request.Timeout = ConvertTimeout(timeout);
		}

		request.Actions.AddRange(aliasOperations);

		var response = await _collectionsClient
			.UpdateAliasesAsync(request, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (!response.Result)
		{
			throw new QdrantException("Alias update operation(s) could not be performed.");
		}

		if (_logger.IsEnabled(LogLevel.Debug))
		{
			foreach (var operation in aliasOperations)
			{
				switch (operation)
				{
					case { CreateAlias: CreateAlias createAlias }:
						_logger.CreateAlias(createAlias.AliasName, createAlias.CollectionName);
						break;

					case { DeleteAlias: DeleteAlias deleteAlias }:
						_logger.DeleteAlias(deleteAlias.AliasName);
						break;

					case { RenameAlias: RenameAlias renameAlias }:
						_logger.RenameAlias(renameAlias.OldAliasName, renameAlias.NewAliasName);
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	/// <summary>
	/// Gets a list of all aliases for a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<string>> ListCollectionAliasesAsync(
		string collectionName,
		CancellationToken cancellationToken = default)
	{
		var request = new ListCollectionAliasesRequest { CollectionName = collectionName };

		var response = await _collectionsClient
			.ListCollectionAliasesAsync(request, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		_logger.ListCollectionAliases(collectionName);

		var names = new string[response.Aliases.Count];
		for (var i = 0; i < names.Length; i++)
		{
			names[i] = response.Aliases[i].AliasName;
		}
		return names;
	}

	/// <summary>
	/// Gets a list of all aliases for all existing collections.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<AliasDescription>> ListAliasesAsync(CancellationToken cancellationToken = default)
	{
		var response = await _collectionsClient
			.ListAliasesAsync(new ListAliasesRequest(), cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		_logger.ListAliases();

		return response.Aliases;
	}

	#endregion Alias management

	private static ulong ConvertTimeout(TimeSpan? timeout)
		=> timeout switch
		{
			null => 0,

			{ TotalSeconds: var seconds } => Math.Floor(seconds) == seconds
				? (ulong)seconds
				: throw new ArgumentException("Sub-second components in timeout are not supported")
		};
}
