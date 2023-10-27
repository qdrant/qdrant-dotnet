using System.Runtime.InteropServices;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Qdrant.Client.Grpc;

namespace Qdrant.Client;

// ReSharper disable UnusedMethodReturnValue.Global

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

	private TimeSpan _grpcTimeout;
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
		=> CreateCollectionAsync(
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
		=> CreateCollectionAsync(
			collectionName, new VectorsConfig { ParamsMap = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, timeout, cancellationToken);

	private async Task CreateCollectionAsync(
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

		_logger.CreateCollection(collectionName);

		try
		{
			var response = await _collectionsClient.CreateAsync(request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
			{
				throw new QdrantException($"Collection '{collectionName}' could not be created");
			}
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
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default)
		=> RecreateCollectionAsync(
			collectionName, new VectorsConfig { Params = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, timeout, cancellationToken);

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task RecreateCollectionAsync(
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
		=> RecreateCollectionAsync(
			collectionName, new VectorsConfig { ParamsMap = vectorsConfig }, shardNumber, replicationFactor,
			writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
			initFromCollection, timeout, cancellationToken);

	private async Task RecreateCollectionAsync(
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
		await DeleteCollectionAsync(collectionName, cancellationToken).ConfigureAwait(false);

		await CreateCollectionAsync(
				collectionName, vectorsConfig, shardNumber, replicationFactor,
				writeConsistencyFactor, onDiskPayload, hnswConfig, optimizersConfig, walConfig, quantizationConfig,
				initFromCollection, timeout, cancellationToken)
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
			{
				names[i] = response.Collections[i].Name;
			}
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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
	{
		_logger.DeleteCollection(collectionName);

		try
		{
			var response = await _collectionsClient
				.DeleteAsync(
					new DeleteCollection { CollectionName = collectionName },
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
			{
				throw new QdrantException($"Collection '{collectionName}' could not be deleted");
			}
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.DeleteCollection), e);

			throw;
		}
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
			{
				throw new QdrantException($"Collection '{collectionName}' could not be updated");
			}
		}
		catch (Exception e)
		{
			_logger.OperationFailed(nameof(LoggingExtensions.UpdateCollection), e);

			throw;
		}
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

		try
		{
			var response = await _collectionsClient
				.UpdateAliasesAsync(
					request,
					deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (!response.Result)
			{
				throw new QdrantException("Alias update operation(s) could not be performed.");
			}
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
			{
				names[i] = response.Aliases[i].AliasName;
			}
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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<UpdateResult> UpsertAsync(
		string collectionName,
		IReadOnlyList<PointStruct> points,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new UpsertPoints
		{
			CollectionName = collectionName,
			Wait = wait
		};

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

		request.Points.AddRange(points);

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
	/// Delete points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The numeric IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return DeleteAsync(collectionName, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Delete points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The GUID IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return DeleteAsync(collectionName, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Delete points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">A filter selecting the points to be deleted.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees. Defaults to <c>Weak</c>.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		string collectionName,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> DeleteAsync(collectionName, new PointsSelector { Filter = filter }, wait, ordering, cancellationToken);

	private async Task<UpdateResult> DeleteAsync(
		string collectionName,
		PointsSelector pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeletePoints
		{
			CollectionName = collectionName,
			Points = pointsSelector,
			Wait = wait
		};

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
	/// Retrieve points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="pointIds">List of points to retrieve.</param>
	/// <param name="withPayload">Whether to include the payload or not.</param>
	/// <param name="withVectors">Whether to include the vectors or not.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		IReadOnlyList<PointId> pointIds,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		CancellationToken cancellationToken = default)
		=> RetrieveAsync(
			collectionName,
			pointIds,
			new WithPayloadSelector { Enable = withPayload },
			new WithVectorsSelector { Enable = withVectors },
			readConsistency,
			cancellationToken);

	/// <summary>
	/// Retrieve points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="pointIds">List of points to retrieve.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorSelector">Options for specifying which vectors to include into response.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		IReadOnlyList<PointId> pointIds,
		WithPayloadSelector payloadSelector,
		WithVectorsSelector vectorSelector,
		ReadConsistency? readConsistency = null,
		CancellationToken cancellationToken = default)
	{
		var request = new GetPoints
		{
			CollectionName = collectionName,
			WithPayload = payloadSelector,
			WithVectors = vectorSelector
		};

		request.Ids.AddRange(pointIds);

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
		}

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<UpdateResult> UpdateVectorsAsync(
		string collectionName,
		IReadOnlyList<PointVectors> points,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new UpdatePointVectors
		{
			CollectionName = collectionName,
			Wait = wait
		};

		request.Points.AddRange(points);

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	private Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return DeleteVectorsAsync(
			collectionName, vectors, new PointsSelector { Points = idsList }, wait, ordering,
			cancellationToken);
	}

	/// <summary>
	/// Delete named vectors for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectors">List of vector names to delete.</param>
	/// <param name="ids">The GUID IDs to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	private Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return DeleteVectorsAsync(
			collectionName, vectors, new PointsSelector { Points = idsList }, wait, ordering,
			cancellationToken);
	}

	/// <summary>
	/// Delete named vectors for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="vectors">List of vector names to delete.</param>
	/// <param name="filter">A filter selecting the points to be deleted.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	private Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> DeleteVectorsAsync(
			collectionName, vectors, new PointsSelector { Filter = filter }, wait, ordering, cancellationToken);

	private async Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		PointsSelector pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeletePointVectors
		{
			CollectionName = collectionName,
			PointsSelector = pointsSelector,
			Wait = wait
		};

		request.Vectors.Names.AddRange(vectors);

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> SetPayloadAsync(collectionName, payload, pointsSelector: null, wait, ordering, cancellationToken);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The numeric IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return SetPayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The GUID IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return SetPayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="filter">A filter selecting the points to be set.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> SetPayloadAsync(
			collectionName, payload, new PointsSelector { Filter = filter }, wait, ordering, cancellationToken);

	private async Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SetPayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait,
		};

		foreach (var kvp in payload)
		{
			request.Payload[kvp.Key] = kvp.Value;
		}

		if (pointsSelector is not null)
		{
			request.PointsSelector = pointsSelector;
		}

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

		_logger.Count(collectionName);

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> OverwritePayloadAsync(collectionName, payload, pointsSelector: null, wait, ordering, cancellationToken);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The numeric IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return OverwritePayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The GUID IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return OverwritePayloadAsync(
			collectionName, payload, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="filter">A filter selecting the points to be overwritten.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> OverwritePayloadAsync(
			collectionName, payload, new PointsSelector { Filter = filter }, wait, ordering, cancellationToken);

	private async Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SetPayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait
		};

		foreach (var kvp in payload)
		{
			request.Payload[kvp.Key] = kvp.Value;
		}

		if (pointsSelector is not null)
		{
			request.PointsSelector = pointsSelector;
		}

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> DeletePayloadAsync(collectionName, keys, pointsSelector: null, wait, ordering, cancellationToken);

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="ids">The numeric IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return DeletePayloadAsync(
			collectionName, keys, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="ids">The GUID IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return DeletePayloadAsync(
			collectionName, keys, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Delete specified key payload for points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="keys">List of keys to delete.</param>
	/// <param name="filter">A filter selecting the points to be overwritten.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> DeletePayloadAsync(
			collectionName, keys, new PointsSelector { Filter = filter }, wait, ordering, cancellationToken);

	private async Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new DeletePayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait
		};

		request.Keys.AddRange(keys);

		if (pointsSelector is not null)
		{
			request.PointsSelector = pointsSelector;
		}

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> ClearPayloadAsync(collectionName, pointsSelector: null, wait, ordering, cancellationToken);

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The numeric IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Num = id }));

		return ClearPayloadAsync(
			collectionName, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="ids">The GUID IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var idsList = new PointsIdsList();
		idsList.Ids.AddRange(ids.Select(id => new PointId { Uuid = id.ToString() }));

		return ClearPayloadAsync(
			collectionName, new PointsSelector { Points = idsList }, wait, ordering, cancellationToken);
	}

	/// <summary>
	/// Remove all payload for specified points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">A filter selecting the points to be overwritten.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
		=> ClearPayloadAsync(collectionName, new PointsSelector { Filter = filter }, wait, ordering, cancellationToken);

	private async Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		PointsSelector? pointsSelector,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default)
	{
		var request = new ClearPayloadPoints
		{
			CollectionName = collectionName,
			Wait = wait
		};

		if (pointsSelector is not null)
		{
			request.Points = pointsSelector;
		}

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
	/// <param name="schemaType">Field name to index.</param>
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

		request.FieldType = FieldType.Keyword;

		if (indexParams is not null)
		{
			request.FieldIndexParams = indexParams;
		}

		if (ordering is not null)
		{
			request.Ordering = new() { Type = ordering.Value };
		}

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
		{
			request.Ordering = new() { Type = ordering.Value };
		}

		_logger.CreatePayloadIndex(collectionName);

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
			_logger.OperationFailed(nameof(LoggingExtensions.CreatePayloadIndex), e);

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
		{
			request.Filter = filter;
		}

		if (filter is not null)
		{
			request.Filter = filter;
		}

		if (searchParams is not null)
		{
			request.Params = searchParams;
		}

		if (scoreThreshold is not null)
		{
			request.ScoreThreshold = scoreThreshold.Value;
		}

		if (vectorName is not null)
		{
			request.VectorName = vectorName;
		}

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<BatchResult>> SearchBatchAsync(
		string collectionName,
		IReadOnlyList<SearchPoints> searches,
		ReadConsistency? readConsistency = null,
		CancellationToken cancellationToken = default)
	{
		var request = new SearchBatchPoints
		{
			CollectionName = collectionName,
		};

		// TODO: Workaround for https://github.com/qdrant/qdrant/issues/2880
		foreach (var search in searches)
		{
			search.CollectionName = collectionName;
		}

		request.SearchPoints.AddRange(searches);

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
		}

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
		{
			request.Filter = filter;
		}

		if (searchParams is not null)
		{
			request.Params = searchParams;
		}

		if (scoreThreshold is not null)
		{
			request.ScoreThreshold = scoreThreshold.Value;
		}

		if (vectorName is not null)
		{
			request.VectorName = vectorName;
		}

		if (withLookup is not null)
		{
			request.WithLookup = withLookup;
		}

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
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
		{
			request.Filter = filter;
		}

		if (offset is not null)
		{
			request.Offset = offset;
		}

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
		}

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
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendPoints
		{
			CollectionName = collectionName,
			Limit = limit,
			Offset = offset,
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		request.Positive.AddRange(positive);

		if (negative is not null)
		{
			request.Negative.AddRange(negative);
		}

		if (filter is not null)
		{
			request.Filter = filter;
		}

		if (searchParams is not null)
		{
			request.Params = searchParams;
		}

		if (scoreThreshold is not null)
		{
			request.ScoreThreshold = scoreThreshold.Value;
		}

		if (usingVector is not null)
		{
			request.Using = usingVector;
		}

		if (lookupFrom is not null)
		{
			request.LookupFrom = lookupFrom;
		}

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
		}

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<BatchResult>> RecommendBatchAsync(
		string collectionName,
		IReadOnlyList<RecommendPoints> recommendSearches,
		ReadConsistency? readConsistency = null,
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendBatchPoints
		{
			CollectionName = collectionName,
		};

		// TODO: Workaround for https://github.com/qdrant/qdrant/issues/2880
		foreach (var search in recommendSearches)
		{
			search.CollectionName = collectionName;
		}

		request.RecommendPoints.AddRange(recommendSearches);

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
		}

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
		CancellationToken cancellationToken = default)
	{
		var request = new RecommendPointGroups
		{
			CollectionName = collectionName,
			GroupBy = groupBy,
			Limit = limit,
			GroupSize = groupSize,
			WithPayload = payloadSelector ?? new WithPayloadSelector { Enable = true },
			WithVectors = vectorsSelector ?? new WithVectorsSelector { Enable = false }
		};

		request.Positive.AddRange(positive);

		if (negative is not null)
		{
			request.Negative.AddRange(negative);
		}

		if (filter is not null)
		{
			request.Filter = filter;
		}

		if (searchParams is not null)
		{
			request.Params = searchParams;
		}

		if (scoreThreshold is not null)
		{
			request.ScoreThreshold = scoreThreshold.Value;
		}

		if (usingVector is not null)
		{
			request.Using = usingVector;
		}

		if (withLookup is not null)
		{
			request.WithLookup = withLookup;
		}

		if (readConsistency is not null)
		{
			request.ReadConsistency = readConsistency;
		}

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
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<ulong> CountAsync(
		string collectionName,
		Filter? filter = null,
		bool exact = true,
		CancellationToken cancellationToken = default)
	{
		var request = new CountPoints
		{
			CollectionName = collectionName,
			Exact = exact
		};

		if (filter is not null)
		{
			request.Filter = filter;
		}

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

	#endregion Point management

	private static void Populate(RepeatedField<float> repeatedField, ReadOnlyMemory<float> memory)
	{
		if (MemoryMarshal.TryGetArray(memory, out var segment) &&
			segment.Offset == 0 &&
			segment.Count == segment.Array!.Length)
		{
			repeatedField.Add(segment.Array);
		}
		else
		{
			foreach (var f in memory.Span)
			{
				repeatedField.Add(f);
			}
		}
	}

	private static ulong ConvertTimeout(TimeSpan? timeout)
		=> timeout switch
		{
			null => 0,

			{ TotalSeconds: var seconds } => Math.Floor(seconds) == seconds
				? (ulong)seconds
				: throw new ArgumentException("Sub-second components in timeout are not supported")
		};


	/// <inheritdoc />
	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		if (_ownsGrpcClient)
		{
			_grpcClient.Dispose();
		}

		_isDisposed = true;
	}
}
