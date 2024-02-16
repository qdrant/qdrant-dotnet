using System.Runtime.InteropServices;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Qdrant.Client.Grpc;

namespace Qdrant.Client;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

/// <summary>
/// Client for the Qdrant vector database.
/// </summary>
public class QdrantClient : IDisposable
{
	private readonly QdrantGrpcClient _grpcClient;
	private readonly bool _ownsGrpcClient;
	private bool _isDisposed;

	private readonly Collections.CollectionsClient _collectionsClient;
	private readonly Points.PointsClient _pointsClient;
	private readonly Snapshots.SnapshotsClient _snapshotsClient;

	private readonly TimeSpan _grpcTimeout;
	private readonly ILogger _logger;

	/// <summary>Instantiates a new Qdrant client.</summary>
	/// <param name="host">The host to connect to.</param>
	/// <param name="port">The port to connect to. Defaults to 6334.</param>
	/// <param name="https">Whether to encrypt the connection using HTTPS. Defaults to <c>false</c>.</param>
	/// <param name="apiKey">The API key to use.</param>
	/// <param name="grpcTimeout">The timeout for gRPC calls to Qdrant; sets the gRPC deadline for all calls.</param>
	/// <param name="loggerFactory">A logger factory through which to log messages.</param>
	/// <remarks>
	/// This type provides higher-level wrappers over the low-level Qdrant gRPC API. If these wrappers aren't
	/// sufficient, <see cref="QdrantGrpcClient" /> can be used instead for low-level API access.
	/// </remarks>
	public QdrantClient(
		string host,
		int port = 6334,
		bool https = false,
		string? apiKey = null,
		TimeSpan grpcTimeout = default,
		ILoggerFactory? loggerFactory = null)
		: this(new UriBuilder(https ? "https" : "http", host, port).Uri, apiKey, grpcTimeout, loggerFactory)
	{
	}

	/// <summary>Instantiates a new Qdrant client.</summary>
	/// <param name="address">The address to connect to.</param>
	/// <param name="apiKey">The API key to use.</param>
	/// <param name="grpcTimeout">The timeout for gRPC calls to Qdrant; sets the gRPC deadline for all calls.</param>
	/// <param name="loggerFactory">A logger factory through which to log messages.</param>
	/// <remarks>
	/// This type provides higher-level wrappers over the low-level Qdrant gRPC API. If these wrappers aren't
	/// sufficient, <see cref="QdrantGrpcClient" /> can be used instead for low-level API access.
	/// </remarks>
	public QdrantClient(
		System.Uri address,
		string? apiKey = null,
		TimeSpan grpcTimeout = default,
		ILoggerFactory? loggerFactory = null)
		: this(new QdrantGrpcClient(address, apiKey), ownsGrpcClient: true, grpcTimeout, loggerFactory)
	{
	}

	/// <summary>Instantiates a new Qdrant client.</summary>
	/// <param name="grpcClient">The low-level gRPC client to use.</param>
	/// <param name="grpcTimeout">The timeout for gRPC calls to Qdrant; sets the gRPC deadline for all calls.</param>
	/// <param name="loggerFactory">A logger factory through which to log messages.</param>
	/// <remarks>
	/// This type provides higher-level wrappers over the low-level Qdrant gRPC API. If these wrappers aren't
	/// sufficient, <see cref="QdrantGrpcClient" /> can be used instead for low-level API access.
	/// </remarks>
	public QdrantClient(
		QdrantGrpcClient grpcClient,
		TimeSpan grpcTimeout = default,
		ILoggerFactory? loggerFactory = null)
		: this(grpcClient, ownsGrpcClient: false, grpcTimeout, loggerFactory)
	{
	}

	private QdrantClient(
		QdrantGrpcClient grpcClient,
		bool ownsGrpcClient,
		TimeSpan grpcTimeout = default,
		ILoggerFactory? loggerFactory = null)
	{
		_grpcClient = grpcClient;
		_ownsGrpcClient = ownsGrpcClient;

		_collectionsClient = grpcClient.Collections;
		_pointsClient = grpcClient.Points;
		_snapshotsClient = grpcClient.Snapshots;

		_grpcTimeout = grpcTimeout;
		_logger = loggerFactory?.CreateLogger("Qdrant.Client") ?? NullLogger.Instance;
	}

	#region CreateCollection

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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
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
		ShardingMethod? shardingMethod = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> CreateCollectionAsync(
			collectionName, new VectorsConfig { Params = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, shardingMethod, sparseVectorsConfig, timeout, cancellationToken);

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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task CreateCollectionAsync(
		string collectionName,
		VectorParamsMap? vectorsConfig = null,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		ShardingMethod? shardingMethod = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> CreateCollectionAsync(
			collectionName, vectorsConfig == null ? null : new VectorsConfig { ParamsMap = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, shardingMethod, sparseVectorsConfig, timeout, cancellationToken);

	private async Task CreateCollectionAsync(
		string collectionName,
		VectorsConfig? vectorsConfig = null,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		ShardingMethod? shardingMethod = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
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
			QuantizationConfig = quantizationConfig,
			SparseVectorsConfig = sparseVectorsConfig,
		};

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (initFromCollection is not null)
			request.InitFromCollection = initFromCollection;

		if (shardingMethod is not null)
			request.ShardingMethod = (ShardingMethod)shardingMethod;

		_logger.CreateCollection(collectionName);

		try
		{
			var response = await _collectionsClient.CreateAsync(request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
				throw new QdrantException($"Collection '{collectionName}' could not be created");
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.CreateCollection), e);

			throw;
		}
	}

	#endregion CreateCollection

	#region RecreateCollection

	/// <summary>
	/// Deletes a collection if one exists, and creates a new collection with the given parameters.
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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task RecreateCollectionAsync(
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
		ShardingMethod? shardingMethod = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> RecreateCollectionAsync(
			collectionName, new VectorsConfig { Params = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, shardingMethod, sparseVectorsConfig, timeout, cancellationToken);

	/// <summary>
	/// Deletes a collection if one exists, and creates a new collection with the given parameters.
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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task RecreateCollectionAsync(
		string collectionName,
		VectorParamsMap? vectorsConfig = null,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		ShardingMethod? shardingMethod = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> RecreateCollectionAsync(
			collectionName, vectorsConfig == null ? null : new VectorsConfig { ParamsMap = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, shardingMethod, sparseVectorsConfig, timeout, cancellationToken);

	private async Task RecreateCollectionAsync(
		string collectionName,
		VectorsConfig? vectorsConfig = null,
		uint shardNumber = 1,
		uint replicationFactor = 1,
		uint writeConsistencyFactor = 1,
		bool onDiskPayload = false,
		HnswConfigDiff? hnswConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		WalConfigDiff? walConfig = null,
		QuantizationConfig? quantizationConfig = null,
		string? initFromCollection = null,
		ShardingMethod? shardingMethod = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		await DeleteCollectionAsync(collectionName, timeout, cancellationToken).ConfigureAwait(false);

		await CreateCollectionAsync(
				collectionName, vectorsConfig, shardNumber, replicationFactor,
				writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
				initFromCollection, shardingMethod, sparseVectorsConfig, timeout, cancellationToken)
			.ConfigureAwait(false);
	}

	#endregion RecreateCollection

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
		_logger.GetCollectionInfo(collectionName);

		try
		{
			var response = await _collectionsClient.GetAsync(
					new GetCollectionInfoRequest { CollectionName = collectionName },
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.GetCollectionInfo), e);

			throw;
		}
	}

	/// <summary>
	/// Gets the names of all existing collections.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default)
	{
		_logger.ListCollections();

		try
		{
			var response = await _collectionsClient
				.ListAsync(
					new ListCollectionsRequest(),
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			var names = new string[response.Collections.Count];
			for (var i = 0; i < names.Length; i++)
				names[i] = response.Collections[i].Name;

			return names;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.ListCollections), e);

			throw;
		}
	}

	/// <summary>
	/// Drop a collection and all its associated data.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="timeout">Wait timeout for operation commit in seconds, if not specified - default value will be supplied</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task DeleteCollectionAsync(
		string collectionName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeleteCollection
		{
			CollectionName = collectionName,
		};

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		_logger.DeleteCollection(collectionName);

		try
		{
			var response = await _collectionsClient
				.DeleteAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
				throw new QdrantException($"Collection '{collectionName}' could not be deleted");
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeleteCollection), e);

			throw;
		}
	}

	#region UpdateCollection

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
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
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
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateCollectionCoreAsync(collectionName, new VectorsConfigDiff { Params = vectorsConfig }, optimizersConfig,
			collectionParams, hnswConfig, quantizationConfig, sparseVectorsConfig, timeout, cancellationToken);

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
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
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
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateCollectionCoreAsync(collectionName, new VectorsConfigDiff { ParamsMap = vectorsConfig },
			optimizersConfig, collectionParams, hnswConfig, quantizationConfig, sparseVectorsConfig, timeout, cancellationToken);

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
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task UpdateCollectionAsync(
		string collectionName,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> UpdateCollectionCoreAsync(collectionName, vectorsConfig: null, optimizersConfig, collectionParams,
			hnswConfig, quantizationConfig, sparseVectorsConfig, timeout, cancellationToken);

	private async Task UpdateCollectionCoreAsync(
		string collectionName,
		VectorsConfigDiff? vectorsConfig = null,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		SparseVectorConfig? sparseVectorsConfig = null,
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
			SparseVectorsConfig = sparseVectorsConfig,
		};

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		_logger.UpdateCollection(collectionName);

		try
		{
			var response = await _collectionsClient
				.UpdateAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
				throw new QdrantException($"Collection '{collectionName}' could not be updated");
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.UpdateCollection), e);

			throw;
		}
	}

	#endregion UpdateCollection

	#region Alias management

	/// <summary>
	/// Creates an alias for a given collection.
	/// </summary>
	/// <param name="aliasName">The alias to be created.</param>
	/// <param name="collectionName">The collection for which the alias is to be created.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
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
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
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
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
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
	/// <remarks>
	/// Alias changes are atomic, meaning that no collection modifications can happen between alias operations.
	/// </remarks>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task UpdateAliasesAsync(
		IReadOnlyList<AliasOperations> aliasOperations,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new ChangeAliases { Actions = { aliasOperations } };

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (_logger.IsEnabled(LogLevel.Debug))
		{
			foreach (var operation in aliasOperations)
			{
				// ReSharper disable ConvertTypeCheckPatternToNullCheck
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
				// ReSharper restore ConvertTypeCheckPatternToNullCheck
			}
		}

		try
		{
			var response = await _collectionsClient
				.UpdateAliasesAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
				throw new QdrantException("Alias update operation(s) could not be performed.");
		}
		catch (Exception e)
		{
			_logger.OperationFailed("UpdateAliases", e);

			throw;
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

		_logger.ListCollectionAliases(collectionName);

		try
		{
			var response = await _collectionsClient
				.ListCollectionAliasesAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			var names = new string[response.Aliases.Count];
			for (var i = 0; i < names.Length; i++)
				names[i] = response.Aliases[i].AliasName;

			return names;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.ListCollectionAliases), e);

			throw;
		}
	}

	/// <summary>
	/// Gets a list of all aliases for all existing collections.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<AliasDescription>> ListAliasesAsync(CancellationToken cancellationToken = default)
	{
		_logger.ListAliases();

		try
		{
			var response = await _collectionsClient
				.ListAliasesAsync(
					new ListAliasesRequest(),
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Aliases;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.ListAliases), e);

			throw;
		}
	}

	#endregion Alias management

	#region Point management

	/// <summary>
	/// Perform insert and updates on points. If a point with a given ID already exists, it will be overwritten.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="points">The points to be upserted.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<UpdateResult> UpsertAsync(
		string collectionName,
		IReadOnlyList<PointStruct> points,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new UpsertPoints
		{
			CollectionName = collectionName,
			Points = { points },
			Wait = wait
		};

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Upsert(collectionName, points.Count);

		try
		{
			var response = await _pointsClient.UpsertAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Upsert), e);

			throw;
		}
	}

	/// <summary>
	/// Delete a point.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="id">The IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeleteAsync(collectionName,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Num = id } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Delete points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return DeleteAsync(collectionName, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Delete a point.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="id">The IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeleteAsync(collectionName,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Uuid = id.ToString() } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Delete points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return DeleteAsync(collectionName, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Delete points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">A filter selecting the points to be deleted.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeleteAsync(collectionName, new PointsSelector { Filter = filter }, wait, ordering, shardKeySelector, cancellationToken);

	private async Task<UpdateResult> DeleteAsync(
		string collectionName,
		PointsSelector pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeletePoints
		{
			CollectionName = collectionName,
			Points = pointsSelector,
			Wait = wait
		};

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Delete(collectionName);

		try
		{
			var response = await _pointsClient.DeleteAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Delete), e);

			throw;
		}
	}

	/// <summary>
	/// Retrieve a point.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="id">The ID of a point to retrieve.</param>
	/// <param name="withPayload">Whether to include the payload or not.</param>
	/// <param name="withVectors">Whether to include the vectors or not.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		Guid id,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> RetrieveAsync(
			collectionName,
			new[] { new PointId { Uuid = id.ToString() } },
			new WithPayloadSelector { Enable = withPayload },
			new WithVectorsSelector { Enable = withVectors },
			readConsistency,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Retrieve a point.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="id">The ID of a point to retrieve.</param>
	/// <param name="withPayload">Whether to include the payload or not.</param>
	/// <param name="withVectors">Whether to include the vectors or not.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		ulong id,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> RetrieveAsync(
			collectionName,
			new[] { new PointId { Num = id } },
			new WithPayloadSelector { Enable = withPayload },
			new WithVectorsSelector { Enable = withVectors },
			readConsistency,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Retrieve points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">List of points to retrieve.</param>
	/// <param name="withPayload">Whether to include the payload or not.</param>
	/// <param name="withVectors">Whether to include the vectors or not.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		IReadOnlyList<PointId> ids,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> RetrieveAsync(
			collectionName,
			ids,
			new WithPayloadSelector { Enable = withPayload },
			new WithVectorsSelector { Enable = withVectors },
			readConsistency,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Retrieve points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">List of points to retrieve.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorSelector">Options for specifying which vectors to include into response.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		IReadOnlyList<PointId> ids,
		WithPayloadSelector payloadSelector,
		WithVectorsSelector vectorSelector,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new GetPoints
		{
			CollectionName = collectionName,
			Ids = { ids },
			WithPayload = payloadSelector,
			WithVectors = vectorSelector
		};

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Retrieve(collectionName);

		try
		{
			var response = await _pointsClient.GetAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Retrieve), e);

			throw;
		}
	}

	/// <summary>
	/// Update named vectors for point.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="points">The list of points and vectors to update.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<UpdateResult> UpdateVectorsAsync(
		string collectionName,
		IReadOnlyList<PointVectors> points,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new UpdatePointVectors
		{
			CollectionName = collectionName,
			Points = { points },
			Wait = wait
		};

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.UpdateVectors(collectionName);

		try
		{
			var response = await _pointsClient.UpdateVectorsAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.UpdateVectors), e);

			throw;
		}
	}

	#region DeleteVectors

	/// <summary>
	/// Delete named vectors for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectors">List of vector names to delete.</param>
	/// <param name="ids">The numeric IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	private Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return DeleteVectorsAsync(
			collectionName, vectors, new PointsSelector { Points = idsList }, wait, ordering,
			shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Delete named vectors for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectors">List of vector names to delete.</param>
	/// <param name="ids">The GUID IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	private Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return DeleteVectorsAsync(
			collectionName, vectors, new PointsSelector { Points = idsList }, wait, ordering,
			shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Delete named vectors for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectors">List of vector names to delete.</param>
	/// <param name="filter">A filter selecting the points to be deleted.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	private Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeleteVectorsAsync(
			collectionName, vectors, new PointsSelector { Filter = filter }, wait, ordering, shardKeySelector, cancellationToken);

	private async Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		PointsSelector pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeletePointVectors
		{
			CollectionName = collectionName,
			PointsSelector = pointsSelector,
			Vectors = new VectorsSelector { Names = { vectors } },
			Wait = wait
		};

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.DeleteVectors(collectionName);

		try
		{
			var response = await _pointsClient.DeleteVectorsAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeleteVectors), e);

			throw;
		}
	}

	#endregion DeleteVectors

	#region SetPayload

	/// <summary>
	/// Sets the payload for all points in the collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> SetPayloadAsync(collectionName, payload, pointsSelector: null, wait, ordering, shardKeySelector, cancellationToken);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> SetPayloadAsync(
			collectionName, payload,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Num = id } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return SetPayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> SetPayloadAsync(
			collectionName, payload,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Uuid = id.ToString() } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The GUID IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return SetPayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="filter">A filter selecting the points to be set.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> SetPayloadAsync(
			collectionName, payload, new PointsSelector { Filter = filter }, wait, ordering, shardKeySelector, cancellationToken);

	private async Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SetPayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait,
		};

		foreach (var kvp in payload)
			request.Payload[kvp.Key] = kvp.Value;

		if (pointsSelector is not null)
			request.PointsSelector = pointsSelector;

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.SetPayload(collectionName);

		try
		{
			var response = await _pointsClient.SetPayloadAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.SetPayload), e);

			throw;
		}
	}

	#endregion SetPayload

	#region OverwritePayload

	/// <summary>
	/// Overwrites the payload for all points in the collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> OverwritePayloadAsync(collectionName, payload, pointsSelector: null, wait, ordering, shardKeySelector, cancellationToken);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> OverwritePayloadAsync(
			collectionName, payload,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Num = id } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The IDs for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return OverwritePayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> OverwritePayloadAsync(
			collectionName, payload,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Uuid = id.ToString() } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The IDs for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return OverwritePayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="filter">A filter selecting the points to be overwritten.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> OverwritePayloadAsync(
			collectionName, payload, new PointsSelector { Filter = filter }, wait, ordering, shardKeySelector, cancellationToken);

	private async Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SetPayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait
		};

		foreach (var kvp in payload)
			request.Payload[kvp.Key] = kvp.Value;

		if (pointsSelector is not null)
			request.PointsSelector = pointsSelector;

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.OverwritePayload(collectionName);

		try
		{
			var response = await _pointsClient.OverwritePayloadAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.OverwritePayload), e);

			throw;
		}
	}

	#endregion OverwritePayload

	#region DeletePayload

	/// <summary>
	/// Delete specified key payload for all points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeletePayloadAsync(collectionName, keys, pointsSelector: null, wait, ordering, shardKeySelector, cancellationToken);

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="id">The ID for which to delete the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeletePayloadAsync(
			collectionName, keys,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Num = id } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="ids">The IDs for which to delete the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return DeletePayloadAsync(
			collectionName, keys, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="id">The ID for which to delete the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeletePayloadAsync(
			collectionName, keys,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Uuid = id.ToString() } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="ids">The IDs for which to delete the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return DeletePayloadAsync(
			collectionName, keys, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="filter">A filter selecting the points to be overwritten.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> DeletePayloadAsync(
			collectionName, keys, new PointsSelector { Filter = filter }, wait, ordering, shardKeySelector, cancellationToken);

	private async Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeletePayloadPoints
		{
			CollectionName = collectionName,
			Keys = { keys },
			Wait = wait
		};

		if (pointsSelector is not null)
			request.PointsSelector = pointsSelector;

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.DeletePayload(collectionName);

		try
		{
			var response = await _pointsClient.DeletePayloadAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeletePayload), e);

			throw;
		}
	}

	#endregion DeletePayload

	#region ClearPayload

	/// <summary>
	/// Remove all payload for all points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> ClearPayloadAsync(collectionName, pointsSelector: null, wait, ordering, shardKeySelector, cancellationToken);

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="id">The ID for which to remove the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> ClearPayloadAsync(
			collectionName, new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Num = id } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The IDs for which to remove the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return ClearPayloadAsync(
			collectionName, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="id">The ID for which to remove the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> ClearPayloadAsync(
			collectionName,
			new PointsSelector { Points = new PointsIdsList { Ids = { new PointId { Uuid = id.ToString() } } } },
			wait,
			ordering,
			shardKeySelector,
			cancellationToken);

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The IDs for which to remove the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return ClearPayloadAsync(
			collectionName, new PointsSelector { Points = idsList }, wait, ordering, shardKeySelector, cancellationToken);
	}

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">A filter selecting the points for which to remove the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
		=> ClearPayloadAsync(collectionName, new PointsSelector { Filter = filter }, wait, ordering, shardKeySelector, cancellationToken);

	private async Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new ClearPayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait
		};

		if (pointsSelector is not null)
			request.Points = pointsSelector;

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.ClearPayload(collectionName);

		try
		{
			var response = await _pointsClient.ClearPayloadAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.ClearPayload), e);

			throw;
		}
	}

	#endregion ClearPayload

	/// <summary>
	/// Creates a payload field index in a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="fieldName">Field name to index.</param>
	/// <param name="schemaType">The schema type of the field.</param>
	/// <param name="indexParams">Payload index params.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<UpdateResult> CreatePayloadIndexAsync(
		string collectionName,
		string fieldName,
		PayloadSchemaType schemaType = PayloadSchemaType.Keyword,
		PayloadIndexParams? indexParams = null,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new CreateFieldIndexCollection
		{
			CollectionName = collectionName,
			FieldName = fieldName,
			Wait = wait,
			FieldType = schemaType switch
			{
				PayloadSchemaType.Keyword => FieldType.Keyword,
				PayloadSchemaType.Integer => FieldType.Integer,
				PayloadSchemaType.Float => FieldType.Float,
				PayloadSchemaType.Bool => FieldType.Bool,
				PayloadSchemaType.Geo => FieldType.Geo,
				PayloadSchemaType.Text => FieldType.Text,

				_ => throw new ArgumentException("Invalid PayloadSchemaType: " + schemaType, nameof(schemaType))
			}
		};

		if (indexParams is not null)
			request.FieldIndexParams = indexParams;

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		_logger.CreatePayloadIndex(collectionName);

		try
		{
			var response = await _pointsClient.CreateFieldIndexAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.CreatePayloadIndex), e);

			throw;
		}
	}

	/// <summary>
	/// Deletes a payload field index in a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="fieldName">Field name to index.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<UpdateResult> DeletePayloadIndexAsync(
		string collectionName,
		string fieldName,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeleteFieldIndexCollection
		{
			CollectionName = collectionName,
			FieldName = fieldName,
			Wait = wait,
		};

		if (ordering is not null)
			request.Ordering = new() { Type = ordering.Value };

		_logger.DeletePayloadIndex(collectionName);

		try
		{
			var response = await _pointsClient.DeleteFieldIndexAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeletePayloadIndex), e);

			throw;
		}
	}

	/// <summary>
	/// Retrieves closest points based on vector similarity and the given filtering conditions.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vector">The vector to search for.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="offset">Offset of the result.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="vectorName">Which vector to use for search, if not specified - use default vector.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="sparseIndices">Sparse vector indices. If set, vector argument will be used as sparse values list.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<ScoredPoint>> SearchAsync(
		string collectionName,
		ReadOnlyMemory<float> vector,
		Filter? filter = null,
		SearchParams? searchParams = null,
		ulong limit = 10,
		ulong offset = 0,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? vectorName = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		ReadOnlyMemory<uint>? sparseIndices = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SearchPoints
		{
			CollectionName = collectionName,
			Limit = limit,
			Offset = offset,
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		Populate(request.Vector, vector);

		if (filter is not null)
			request.Filter = filter;

		if (searchParams is not null)
			request.Params = searchParams;

		if (scoreThreshold is not null)
			request.ScoreThreshold = scoreThreshold.Value;

		if (vectorName is not null)
			request.VectorName = vectorName;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		if (sparseIndices is not null)
		{
			var sparseIndicesContainer = new SparseIndices();
			Populate(sparseIndicesContainer.Data, (ReadOnlyMemory<uint>)sparseIndices);
			request.SparseIndices = sparseIndicesContainer;
		}

		_logger.Search(collectionName);

		try
		{
			var response = await _pointsClient.SearchAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Search), e);

			throw;
		}
	}

	/// <summary>
	/// Retrieves closest points based on vector similarity and the given filtering conditions.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="searches">The searches to be performed in the batch.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<BatchResult>> SearchBatchAsync(
		string collectionName,
		IReadOnlyList<SearchPoints> searches,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SearchBatchPoints
		{
			CollectionName = collectionName,
		};

		// TODO: Workaround for https://github.com/qdrant/qdrant/issues/2880
		foreach (var search in searches)
			search.CollectionName = collectionName;

		request.SearchPoints.AddRange(searches);

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		_logger.SearchBatch(collectionName);

		try
		{
			var response = await _pointsClient.SearchBatchAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.SearchBatch), e);

			throw;
		}
	}

	/// <summary>
	/// Retrieves closest points based on vector similarity and the given filtering conditions, grouped by a given field.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vector">The vector to search for.</param>
	/// <param name="groupBy">
	/// Payload field to group by, must be a string or number field. If there are multiple values for the field, all of
	/// them will be used. One point can be in multiple groups.
	/// </param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="groupSize">Maximum amount of points to return per group.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores..</param>
	/// <param name="vectorName">Which vector to use for search, if not specified - use default vector.</param>
	/// <param name="withLookup">
	/// Options for specifying how to use the group id to lookup points in another collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="sparseIndices">Sparse vector indices. If set, vector argument will be used as sparse values list.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<PointGroup>> SearchGroupsAsync(
		string collectionName,
		ReadOnlyMemory<float> vector,
		string groupBy,
		Filter? filter = null,
		SearchParams? searchParams = null,
		uint limit = 10,
		uint groupSize = 1,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? vectorName = null,
		WithLookup? withLookup = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		ReadOnlyMemory<uint>? sparseIndices = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SearchPointGroups
		{
			CollectionName = collectionName,
			GroupBy = groupBy,
			Limit = limit,
			GroupSize = groupSize,
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		Populate(request.Vector, vector);

		if (filter is not null)
			request.Filter = filter;

		if (searchParams is not null)
			request.Params = searchParams;

		if (scoreThreshold is not null)
			request.ScoreThreshold = scoreThreshold.Value;

		if (vectorName is not null)
			request.VectorName = vectorName;

		if (withLookup is not null)
			request.WithLookup = withLookup;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		if (sparseIndices is not null)
		{
			var sparseIndicesContainer = new SparseIndices();
			Populate(sparseIndicesContainer.Data, (ReadOnlyMemory<uint>)sparseIndices);
			request.SparseIndices = sparseIndicesContainer;
		}

		_logger.SearchGroups(collectionName);

		try
		{
			var response = await _pointsClient.SearchGroupsAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result.Groups;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.SearchGroups), e);

			throw;
		}
	}

	/// <summary>
	/// Iterates over all or filtered points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="offset">Start with this ID.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<ScrollResponse> ScrollAsync(
		string collectionName,
		Filter? filter = null,
		uint limit = 10,
		PointId? offset = null,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new ScrollPoints
		{
			CollectionName = collectionName,
			Limit = limit,
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		if (filter is not null)
			request.Filter = filter;

		if (offset is not null)
			request.Offset = offset;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Scroll(collectionName);

		try
		{
			var response = await _pointsClient.ScrollAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Scroll), e);

			throw;
		}
	}

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="offset">Offset of the result.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="lookupFrom">
	/// Name of the collection to use for points lookup, if not specified - use current collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<ScoredPoint>> RecommendAsync(
		string collectionName,
		IReadOnlyList<ulong> positive,
		IReadOnlyList<ulong>? negative = null,
		Filter? filter = null,
		SearchParams? searchParams = null,
		ulong limit = 10,
		ulong offset = 0,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? usingVector = null,
		LookupLocation? lookupFrom = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> RecommendAsync(
			collectionName,
			positive.Select(id => new PointId { Num = id }).ToList(),
			negative?.Select(id => new PointId { Num = id }).ToList(),
			filter,
			searchParams,
			limit,
			offset,
			payloadSelector,
			vectorsSelector,
			scoreThreshold,
			usingVector,
			lookupFrom,
			readConsistency,
			shardKeySelector,
			timeout,
			cancellationToken);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="offset">Offset of the result.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="lookupFrom">
	/// Name of the collection to use for points lookup, if not specified - use current collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<ScoredPoint>> RecommendAsync(
		string collectionName,
		IReadOnlyList<Guid> positive,
		IReadOnlyList<Guid>? negative = null,
		Filter? filter = null,
		SearchParams? searchParams = null,
		ulong limit = 10,
		ulong offset = 0,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? usingVector = null,
		LookupLocation? lookupFrom = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> RecommendAsync(
			collectionName,
			positive.Select(id => new PointId { Uuid = id.ToString() }).ToList(),
			negative?.Select(id => new PointId { Uuid = id.ToString() }).ToList(),
			filter,
			searchParams,
			limit,
			offset,
			payloadSelector,
			vectorsSelector,
			scoreThreshold,
			usingVector,
			lookupFrom,
			readConsistency,
			shardKeySelector,
			timeout,
			cancellationToken);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="offset">Offset of the result.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="lookupFrom">
	/// Name of the collection to use for points lookup, if not specified - use current collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<ScoredPoint>> RecommendAsync(
		string collectionName,
		IReadOnlyList<PointId> positive,
		IReadOnlyList<PointId>? negative = null,
		Filter? filter = null,
		SearchParams? searchParams = null,
		ulong limit = 10,
		ulong offset = 0,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? usingVector = null,
		LookupLocation? lookupFrom = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendPoints
		{
			CollectionName = collectionName,
			Limit = limit,
			Offset = offset,
			Positive = { positive },
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		if (negative is not null)
			request.Negative.AddRange(negative);

		if (filter is not null)
			request.Filter = filter;

		if (searchParams is not null)
			request.Params = searchParams;

		if (scoreThreshold is not null)
			request.ScoreThreshold = scoreThreshold.Value;

		if (usingVector is not null)
			request.Using = usingVector;

		if (lookupFrom is not null)
			request.LookupFrom = lookupFrom;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Recommend(collectionName);

		try
		{
			var response = await _pointsClient.RecommendAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Recommend), e);

			throw;
		}
	}

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="recommendSearches">The recommendation searches to be performed in the batch.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<BatchResult>> RecommendBatchAsync(
		string collectionName,
		IReadOnlyList<RecommendPoints> recommendSearches,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendBatchPoints
		{
			CollectionName = collectionName,
		};

		// TODO: Workaround for https://github.com/qdrant/qdrant/issues/2880
		foreach (var search in recommendSearches)
			search.CollectionName = collectionName;

		request.RecommendPoints.AddRange(recommendSearches);

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		_logger.RecommendBatch(collectionName);

		try
		{
			var response = await _pointsClient.RecommendBatchAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.SearchBatch), e);

			throw;
		}
	}

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples, grouped by a given field
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="groupBy">
	/// Payload field to group by, must be a string or number field. If there are multiple values for the field, all of
	/// them will be used. One point can be in multiple groups.
	/// </param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="groupSize">Maximum amount of points to return per group.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="withLookup">
	/// Options for specifying how to use the group id to lookup points in another collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<PointGroup>> RecommendGroupsAsync(
		string collectionName,
		string groupBy,
		IReadOnlyList<ulong> positive,
		IReadOnlyList<ulong>? negative = null,
		Filter? filter = null,
		SearchParams? searchParams = null,
		uint limit = 10,
		uint groupSize = 1,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? usingVector = null,
		WithLookup? withLookup = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	=> RecommendGroupsAsync(
		collectionName,
		groupBy,
		positive.Select(id => new PointId { Num = id }).ToList(),
		negative?.Select(id => new PointId { Num = id }).ToList(),
		filter,
		searchParams,
		limit,
		groupSize,
		payloadSelector,
		vectorsSelector,
		scoreThreshold,
		usingVector,
		withLookup,
		readConsistency,
		shardKeySelector,
		timeout,
		cancellationToken);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples, grouped by a given field
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="groupBy">
	/// Payload field to group by, must be a string or number field. If there are multiple values for the field, all of
	/// them will be used. One point can be in multiple groups.
	/// </param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="groupSize">Maximum amount of points to return per group.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="withLookup">
	/// Options for specifying how to use the group id to lookup points in another collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<PointGroup>> RecommendGroupsAsync(
		string collectionName,
		string groupBy,
		IReadOnlyList<Guid> positive,
		IReadOnlyList<Guid>? negative = null,
		Filter? filter = null,
		SearchParams? searchParams = null,
		uint limit = 10,
		uint groupSize = 1,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? usingVector = null,
		WithLookup? withLookup = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	=> RecommendGroupsAsync(
		collectionName,
		groupBy,
		positive.Select(id => new PointId { Uuid = id.ToString() }).ToList(),
		negative?.Select(id => new PointId { Uuid = id.ToString() }).ToList(),
		filter,
		searchParams,
		limit,
		groupSize,
		payloadSelector,
		vectorsSelector,
		scoreThreshold,
		usingVector,
		withLookup,
		readConsistency,
		shardKeySelector,
		timeout,
		cancellationToken);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples, grouped by a given field
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="groupBy">
	/// Payload field to group by, must be a string or number field. If there are multiple values for the field, all of
	/// them will be used. One point can be in multiple groups.
	/// </param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="groupSize">Maximum amount of points to return per group.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="scoreThreshold">If provided - cut off results with worse scores.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="withLookup">
	/// Options for specifying how to use the group id to lookup points in another collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<PointGroup>> RecommendGroupsAsync(
		string collectionName,
		string groupBy,
		IReadOnlyList<PointId> positive,
		IReadOnlyList<PointId>? negative = null,
		Filter? filter = null,
		SearchParams? searchParams = null,
		uint limit = 10,
		uint groupSize = 1,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		float? scoreThreshold = null,
		string? usingVector = null,
		WithLookup? withLookup = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendPointGroups
		{
			CollectionName = collectionName,
			GroupBy = groupBy,
			Limit = limit,
			GroupSize = groupSize,
			Positive = { positive },
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		if (negative is not null)
			request.Negative.AddRange(negative);

		if (filter is not null)
			request.Filter = filter;

		if (searchParams is not null)
			request.Params = searchParams;

		if (scoreThreshold is not null)
			request.ScoreThreshold = scoreThreshold.Value;

		if (usingVector is not null)
			request.Using = usingVector;

		if (withLookup is not null)
			request.WithLookup = withLookup;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.RecommendGroups(collectionName);

		try
		{
			var response = await _pointsClient.RecommendGroupsAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result.Groups;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.RecommendGroups), e);

			throw;
		}
	}

	/// <summary>
	/// Count the points in a collection with the given filtering conditions.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="exact">
	/// If <c>true</c>, returns the exact count, if <c>false</c>, returns an approximate count. Defaults to <c>true</c>.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<ulong> CountAsync(
		string collectionName,
		Filter? filter = null,
		bool exact = true,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default)
	{
		var request = new CountPoints
		{
			CollectionName = collectionName,
			Exact = exact
		};

		if (filter is not null)
			request.Filter = filter;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Count(collectionName);

		try
		{
			var response = await _pointsClient.CountAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result.Count;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Retrieve), e);

			throw;
		}
	}

	/// <summary>
	/// Use context and a target to find the most similar points to the target, constrained by the context.
	/// </summary>
	///
	/// <remarks>
	/// When using only the context (without a target), a special search - called context search - is performed where
	/// pairs of points are used to generate a loss that guides the search towards the zone where
	/// most positive examples overlap. This means that the score minimizes the scenario of
	/// finding a point closer to a negative than to a positive part of a pair.
	///
	/// Since the score of a context relates to loss, the maximum score a point can get is 0.0,
	/// and it becomes normal that many points can have a score of 0.0.
	///
	/// When using target (with or without context), the score behaves a little different: The
	/// integer part of the score represents the rank with respect to the context, while the
	/// decimal part of the score relates to the distance to the target. The context part of the score for
	/// each pair is calculated +1 if the point is closer to a positive than to a negative part of a pair,
	/// and -1 otherwise.
	/// </remarks>
	///
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="context">Search will be constrained by these pairs of examples.</param>
	/// <param name="target">Use this as the primary search objective.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include in the response.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="offset">Offset of the result.</param>
	/// <param name="usingVector">
	/// Define which vector to use for recommendation, if not specified - default vector.
	/// </param>
	/// <param name="lookupFrom">
	/// Name of the collection to use for points lookup, if not specified - use current collection.
	/// </param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<ScoredPoint>> DiscoverAsync(
		string collectionName,
		IReadOnlyList<ContextExamplePair> context,
		TargetVector? target = null,
		Filter? filter = null,
		uint limit = 10,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		SearchParams? searchParams = null,
		ulong offset = 0,
		string? usingVector = null,
		LookupLocation? lookupFrom = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DiscoverPoints
		{
			CollectionName = collectionName,
			Context = { context },
			Limit = limit,
			Offset = offset,
		};

		if (target is not null)
			request.Target = target;

		if (filter is not null)
			request.Filter = filter;

		if (payloadSelector is not null)
			request.WithPayload = payloadSelector;

		if (vectorsSelector is not null)
			request.WithVectors = vectorsSelector;

		if (searchParams is not null)
			request.Params = searchParams;

		if (usingVector is not null)
			request.Using = usingVector;

		if (lookupFrom is not null)
			request.LookupFrom = lookupFrom;

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (shardKeySelector is not null)
			request.ShardKeySelector = shardKeySelector;

		_logger.Discover(collectionName);

		try
		{
			var response = await _pointsClient.DiscoverAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.Discover), e);

			throw;
		}
	}

	/// <summary>
	/// Batch request points based on { positive, negative } pairs of examples, and/or a target
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="discoverPoints">Batched request.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<BatchResult>> DiscoverBatchAsync(
		string collectionName,
		IReadOnlyList<DiscoverPoints> discoverPoints,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DiscoverBatchPoints
		{
			CollectionName = collectionName,
			DiscoverPoints = { discoverPoints },
		};

		if (readConsistency is not null)
			request.ReadConsistency = readConsistency;

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		_logger.DiscoverBatch(collectionName);

		try
		{
			var response = await _pointsClient.DiscoverBatchAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.Result;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DiscoverBatch), e);

			throw;
		}
	}

	#endregion Point management

	#region Snapshot management

	/// <summary>
	/// Create snapshot for a given collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<SnapshotDescription> CreateSnapshotAsync(string collectionName, CancellationToken cancellationToken = default)
	{
		_logger.CreateSnapshot(collectionName);

		try
		{
			var response = await _snapshotsClient.CreateAsync(
					new CreateSnapshotRequest { CollectionName = collectionName },
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.SnapshotDescription;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.CreateSnapshot), e);

			throw;
		}
	}

	/// <summary>
	/// Get list of snapshots for a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<SnapshotDescription>> ListSnapshotsAsync(string collectionName, CancellationToken cancellationToken = default)
	{
		_logger.ListSnapshots(collectionName);

		try
		{
			var response = await _snapshotsClient.ListAsync(
					new ListSnapshotsRequest { CollectionName = collectionName },
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.SnapshotDescriptions;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.ListSnapshots), e);

			throw;
		}
	}

	/// <summary>
	/// Delete snapshot for a given collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="snapshotName">The name of the snapshot</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task DeleteSnapshotAsync(string collectionName, string snapshotName, CancellationToken cancellationToken = default)
	{
		_logger.DeleteSnapshot(collectionName, snapshotName);

		try
		{
			await _snapshotsClient.DeleteAsync(
					new DeleteSnapshotRequest { CollectionName = collectionName, SnapshotName = snapshotName },
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeleteSnapshot), e);

			throw;
		}
	}

	/// <summary>
	/// Create snapshot for a whole storage.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<SnapshotDescription> CreateFullSnapshotAsync(CancellationToken cancellationToken = default)
	{
		_logger.CreateFullSnapshot();

		try
		{
			var response = await _snapshotsClient.CreateFullAsync(
					new CreateFullSnapshotRequest(),
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.SnapshotDescription;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.CreateFullSnapshot), e);

			throw;
		}
	}

	/// <summary>
	/// List all snapshots for a whole storage.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<SnapshotDescription>> ListFullSnapshotsAsync(CancellationToken cancellationToken = default)
	{
		_logger.ListFullSnapshots();

		try
		{
			var response = await _snapshotsClient.ListFullAsync(
					new ListFullSnapshotsRequest(),
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response.SnapshotDescriptions;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.ListFullSnapshots), e);

			throw;
		}
	}

	/// <summary>
	/// Delete snapshot for a whole storage.
	/// </summary>
	/// <param name="snapshotName">The name of the snapshot</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task DeleteFullSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default)
	{
		_logger.DeleteFullSnapshot(snapshotName);

		try
		{
			await _snapshotsClient.DeleteFullAsync(
					new DeleteFullSnapshotRequest { SnapshotName = snapshotName },
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeleteFullSnapshot), e);

			throw;
		}
	}

	#endregion

	#region Cluster management

	/// <summary>
	/// Create shard key
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="createShardKey">Request to create shard key</param>
	/// <param name="timeout">Wait timeout for operation commit in seconds, if not specified - default value will be supplied</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<CreateShardKeyResponse> CreateShardKeyAsync(
		string collectionName,
		CreateShardKey createShardKey,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new CreateShardKeyRequest
		{
			CollectionName = collectionName,
			Request = createShardKey,
		};

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (createShardKey.ShardKey.HasKeyword)
		{
			_logger.CreateShardKey(createShardKey.ShardKey.Keyword, collectionName);
		}
		else
		{
			_logger.CreateShardKey(createShardKey.ShardKey.Number.ToString(), collectionName);
		}

		try
		{
			var response = await _collectionsClient
				.CreateShardKeyAsync(
					request: request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.CreateShardKey), e);

			throw;
		}
	}

	/// <summary>
	/// Delete shard key
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="deleteShardKey">Request to delete shard key</param>
	/// <param name="timeout">Wait timeout for operation commit in seconds, if not specified - default value will be supplied</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<DeleteShardKeyResponse> DeleteShardKeyAsync(
		string collectionName,
		DeleteShardKey deleteShardKey,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeleteShardKeyRequest
		{
			CollectionName = collectionName,
			Request = deleteShardKey,
		};

		if (timeout is not null)
			request.Timeout = ConvertTimeout(timeout);

		if (deleteShardKey.ShardKey.HasKeyword)
		{
			_logger.DeleteShardKey(deleteShardKey.ShardKey.Keyword, collectionName);
		}
		else
		{
			_logger.DeleteShardKey(deleteShardKey.ShardKey.Number.ToString(), collectionName);
		}

		try
		{
			var response = await _collectionsClient
				.DeleteShardKeyAsync(
					request: request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			return response;
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeleteShardKey), e);

			throw;
		}
	}

	#endregion Cluster management

	private static void Populate<T>(RepeatedField<T> repeatedField, ReadOnlyMemory<T> memory)
	{
		if (MemoryMarshal.TryGetArray(memory, out var segment) &&
			segment.Offset == 0 &&
			segment.Count == segment.Array!.Length)
			repeatedField.Add(segment.Array);
		else
		{
			foreach (var f in memory.Span)
				repeatedField.Add(f);
		}
	}

	private static ulong ConvertTimeout(TimeSpan? timeout)
		=> timeout switch
		{
			null => 0,

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			{ TotalSeconds: var seconds } => Math.Floor(seconds) == seconds
				? (ulong)seconds
				: throw new ArgumentException("Sub-second components in timeout are not supported")
		};


	/// <inheritdoc />
	public void Dispose()
	{
		if (_isDisposed)
			return;

		if (_ownsGrpcClient)
			_grpcClient.Dispose();

		_isDisposed = true;
	}
}
