using Grpc.Core;
using Qdrant.Client.Grpc;

namespace Qdrant.Client;

/// <summary>
/// Client for the Qdrant vector database.
/// </summary>
public interface IQdrantClient
{
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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task CreateCollectionAsync(
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
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task CreateCollectionAsync(
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
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task RecreateCollectionAsync(
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
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="shardingMethod">Sharding method.</param>
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task RecreateCollectionAsync(
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
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	#endregion RecreateCollection

	/// <summary>
	/// Gets detailed information about an existing collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<CollectionInfo> GetCollectionInfoAsync(string collectionName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the names of all existing collections.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Drop a collection and all its associated data.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="timeout">Wait timeout for operation commit in seconds, if not specified - default value will be supplied</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task DeleteCollectionAsync(
		string collectionName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Check if a collection exists.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default);

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
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task UpdateCollectionAsync(
		string collectionName,
		VectorParamsDiff vectorsConfig,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task UpdateCollectionAsync(
		string collectionName,
		VectorParamsDiffMap vectorsConfig,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="sparseVectorsConfig">Configuration for sparse vectors.</param>
	/// <param name="strictModeConfig">Configuration for strict mode.</param>
	/// <param name="timeout">
	/// Wait for operation commit timeout. If timeout is reached, the request will return with a service error.
	/// </param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task UpdateCollectionAsync(
		string collectionName,
		OptimizersConfigDiff? optimizersConfig = null,
		CollectionParamsDiff? collectionParams = null,
		HnswConfigDiff? hnswConfig = null,
		QuantizationConfigDiff? quantizationConfig = null,
		SparseVectorConfig? sparseVectorsConfig = null,
		StrictModeConfig? strictModeConfig = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	Task CreateAliasAsync(
		string aliasName,
		string collectionName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	Task RenameAliasAsync(
		string oldAliasName,
		string newAliasName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	Task DeleteAliasAsync(
		string aliasName,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	Task UpdateAliasesAsync(
		IReadOnlyList<AliasOperations> aliasOperations,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a list of all aliases for a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<string>> ListCollectionAliasesAsync(
		string collectionName,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a list of all aliases for all existing collections.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<AliasDescription>> ListAliasesAsync(CancellationToken cancellationToken = default);

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
	Task<UpdateResult> UpsertAsync(
		string collectionName,
		IReadOnlyList<PointStruct> points,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		PointId id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		IReadOnlyList<PointId> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteAsync(
		string collectionName,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		PointId id,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		Guid id,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		ulong id,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		IReadOnlyList<PointId> ids,
		bool withPayload = true,
		bool withVectors = false,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		string collectionName,
		IReadOnlyList<PointId> ids,
		WithPayloadSelector payloadSelector,
		WithVectorsSelector vectorSelector,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> UpdateVectorsAsync(
		string collectionName,
		IReadOnlyList<PointVectors> points,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeleteVectorsAsync(
		string collectionName,
		IReadOnlyList<string> vectors,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="key">Optional key for which to set the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to set the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to set the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to set the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The GUID IDs for which to set the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to set the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="filter">A filter selecting the points to be set.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to set the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> SetPayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="key">Optional key for which to overwrite the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to overwrite the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The IDs for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to overwrite the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="id">The ID for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to overwrite the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="ids">The IDs for which to overwrite the payload.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to overwrite the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Overwrites the payload for the given points.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="payload">New payload values.</param>
	/// <param name="filter">A filter selecting the points to be overwritten.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="shardKeySelector">Option for custom sharding to specify used shard keys.</param>
	/// <param name="key">Optional key for which to overwrite the payload if nested.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<UpdateResult> OverwritePayloadAsync(
		string collectionName,
		IReadOnlyDictionary<string, Value> payload,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		string? key = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadAsync(
		string collectionName,
		IReadOnlyList<string> keys,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		ulong id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		IReadOnlyList<ulong> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		Guid id,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		IReadOnlyList<Guid> ids,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> ClearPayloadAsync(
		string collectionName,
		Filter filter,
		bool wait = true,
		WriteOrderingType? ordering = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> CreatePayloadIndexAsync(
		string collectionName,
		string fieldName,
		PayloadSchemaType schemaType = PayloadSchemaType.Keyword,
		PayloadIndexParams? indexParams = null,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default);

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
	Task<UpdateResult> DeletePayloadIndexAsync(
		string collectionName,
		string fieldName,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<ScoredPoint>> SearchAsync(
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
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<BatchResult>> SearchBatchAsync(
		string collectionName,
		IReadOnlyList<SearchPoints> searches,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<PointGroup>> SearchGroupsAsync(
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
		CancellationToken cancellationToken = default);

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
	/// <param name="orderBy">Order the records by a payload field.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<ScrollResponse> ScrollAsync(
		string collectionName,
		Filter? filter = null,
		uint limit = 10,
		PointId? offset = null,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		OrderBy? orderBy = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="positiveVectors">Look for vectors closest to these.</param>
	/// <param name="negativeVectors">Try to avoid vectors like these.</param>
	/// <param name="strategy">
	/// Strategy to use for recommendation. Strategy defines how to combine multiple examples into a recommendation query.
	/// </param>
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
	Task<IReadOnlyList<ScoredPoint>> RecommendAsync(
		string collectionName,
		IReadOnlyList<ulong> positive,
		IReadOnlyList<ulong>? negative = null,
		ReadOnlyMemory<Vector>? positiveVectors = null,
		ReadOnlyMemory<Vector>? negativeVectors = null,
		RecommendStrategy? strategy = null,
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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="positiveVectors">Look for vectors closest to these.</param>
	/// <param name="negativeVectors">Try to avoid vectors like these.</param>
	/// <param name="strategy">
	/// Strategy to use for recommendation. Strategy defines how to combine multiple examples into a recommendation query.
	/// </param>
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
	Task<IReadOnlyList<ScoredPoint>> RecommendAsync(
		string collectionName,
		IReadOnlyList<Guid> positive,
		IReadOnlyList<Guid>? negative = null,
		ReadOnlyMemory<Vector>? positiveVectors = null,
		ReadOnlyMemory<Vector>? negativeVectors = null,
		RecommendStrategy? strategy = null,
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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Look for the points which are closer to stored positive examples and at the same time further to negative
	/// examples.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="positive">Look for vectors closest to the vectors from these points.</param>
	/// <param name="negative">Try to avoid vectors like the vector from these points.</param>
	/// <param name="positiveVectors">Look for vectors closest to these.</param>
	/// <param name="negativeVectors">Try to avoid vectors like these.</param>
	/// <param name="strategy">
	/// Strategy to use for recommendation. Strategy defines how to combine multiple examples into a recommendation query.
	/// </param>
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
	Task<IReadOnlyList<ScoredPoint>> RecommendAsync(
		string collectionName,
		IReadOnlyList<PointId> positive,
		IReadOnlyList<PointId>? negative = null,
		ReadOnlyMemory<Vector>? positiveVectors = null,
		ReadOnlyMemory<Vector>? negativeVectors = null,
		RecommendStrategy? strategy = null,
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
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<BatchResult>> RecommendBatchAsync(
		string collectionName,
		IReadOnlyList<RecommendPoints> recommendSearches,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="positiveVectors">Look for vectors closest to these.</param>
	/// <param name="negativeVectors">Try to avoid vectors like these.</param>
	/// <param name="strategy">
	/// Strategy to use for recommendation. Strategy defines how to combine multiple examples into a recommendation query.
	/// </param>
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
	Task<IReadOnlyList<PointGroup>> RecommendGroupsAsync(
		string collectionName,
		string groupBy,
		IReadOnlyList<ulong> positive,
		IReadOnlyList<ulong>? negative = null,
		ReadOnlyMemory<Vector>? positiveVectors = null,
		ReadOnlyMemory<Vector>? negativeVectors = null,
		RecommendStrategy? strategy = null,
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
		CancellationToken cancellationToken = default);

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
	/// <param name="positiveVectors">Look for vectors closest to these.</param>
	/// <param name="negativeVectors">Try to avoid vectors like these.</param>
	/// <param name="strategy">
	/// Strategy to use for recommendation. Strategy defines how to combine multiple examples into a recommendation query.
	/// </param>
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
	Task<IReadOnlyList<PointGroup>> RecommendGroupsAsync(
		string collectionName,
		string groupBy,
		IReadOnlyList<Guid> positive,
		IReadOnlyList<Guid>? negative = null,
		ReadOnlyMemory<Vector>? positiveVectors = null,
		ReadOnlyMemory<Vector>? negativeVectors = null,
		RecommendStrategy? strategy = null,
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
		CancellationToken cancellationToken = default);

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
	/// <param name="positiveVectors">Look for vectors closest to these.</param>
	/// <param name="negativeVectors">Try to avoid vectors like these.</param>
	/// <param name="strategy">
	/// Strategy to use for recommendation. Strategy defines how to combine multiple examples into a recommendation query.
	/// </param>
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
	Task<IReadOnlyList<PointGroup>> RecommendGroupsAsync(
		string collectionName,
		string groupBy,
		IReadOnlyList<PointId> positive,
		IReadOnlyList<PointId>? negative = null,
		ReadOnlyMemory<Vector>? positiveVectors = null,
		ReadOnlyMemory<Vector>? negativeVectors = null,
		RecommendStrategy? strategy = null,
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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Performs a batch of operations on a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="operations">The operations to be performed in the batch.</param>
	/// <param name="wait">Whether to wait until the changes have been applied. Defaults to <c>true</c>.</param>
	/// <param name="ordering">Write ordering guarantees.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<UpdateResult>> UpdateBatchAsync(
		string collectionName,
		IReadOnlyList<PointsUpdateOperation> operations,
		bool wait = true,
		WriteOrderingType? ordering = null,
		CancellationToken cancellationToken = default);

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
	Task<ulong> CountAsync(
		string collectionName,
		Filter? filter = null,
		bool exact = true,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		CancellationToken cancellationToken = default);

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
	/// <param name="target">Use this as the primary search objective.</param>
	/// <param name="context">Search will be constrained by these pairs of examples.</param>
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
	Task<IReadOnlyList<ScoredPoint>> DiscoverAsync(
		string collectionName,
		TargetVector target,
		IReadOnlyList<ContextExamplePair> context,
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
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Use context to find points constrained by the context.
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
	Task<IReadOnlyList<ScoredPoint>> DiscoverAsync(
		string collectionName,
		IReadOnlyList<ContextExamplePair> context,
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
		CancellationToken cancellationToken = default);

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
	Task<IReadOnlyList<BatchResult>> DiscoverBatchAsync(
		string collectionName,
		IReadOnlyList<DiscoverPoints> discoverPoints,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Perform facet counts. For each value in the field, count the number of points that have this value and match the conditions.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="key">Payload key of the facet.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="limit">Max number of facets.</param>
	/// <param name="exact">If true, return exact counts, slower but useful for debugging purposes.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">TSpecify in which shards to look for the points, if not specified - look in all shards.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request. Unit is seconds.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<FacetResponse> FacetAsync(
		string collectionName,
		string key,
		Filter? filter = null,
		ulong limit = 10,
		bool exact = false, // If true, return exact counts, slower but useful for debugging purposes. Default is false.
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Compute distance matrix for sampled points with a pair based output format.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="sample">How many points to select and search within.</param>
	/// <param name="limit">How many neighbours per sample to find.</param>
	/// <param name="usingVector">Define which vector to use for querying. If missing, the default vector is is used.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">TSpecify in which shards to look for the points, if not specified - look in all shards.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request. Unit is seconds.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<SearchMatrixPairs> SearchMatrixPairsAsync(
		string collectionName,
		Filter? filter = null,
		ulong sample = 10,
		ulong limit = 3,
		string? usingVector = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Compute distance matrix for sampled points with an offset based output format.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="sample">How many points to select and search within.</param>
	/// <param name="limit">How many neighbours per sample to find.</param>
	/// <param name="usingVector">Define which vector to use for querying. If missing, the default vector is is used.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">TSpecify in which shards to look for the points, if not specified - look in all shards.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request. Unit is seconds.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<SearchMatrixOffsets> SearchMatrixOffsetsAsync(
		string collectionName,
		Filter? filter = null,
		ulong sample = 10,
		ulong limit = 3,
		string? usingVector = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	#endregion Point management

	#region Snapshot management

	/// <summary>
	/// Create snapshot for a given collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<SnapshotDescription> CreateSnapshotAsync(string collectionName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Universally query points.
	/// Covers all capabilities of search, recommend, discover, filters.
	/// Also enables hybrid and multi-stage queries.
	///
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="query">Query to perform. If missing, returns points ordered by their IDs.</param>
	/// <param name="prefetch">Sub-requests to perform first. If present, the query will be performed on the results of the prefetches.</param>
	/// <param name="usingVector">Name of the vector to use for querying. If missing, the default vector is used..</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="scoreThreshold">Return points with scores better than this threshold.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="offset">Offset of the result.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include into the response.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Specify in which shards to look for the points, if not specified - look in all shards.</param>
	/// <param name="lookupFrom">The location to use for IDs lookup, if not specified - use the current collection and the 'usingVector' vector</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<ScoredPoint>> QueryAsync(
		string collectionName,
		Query? query = null,
		IReadOnlyList<PrefetchQuery>? prefetch = null,
		string? usingVector = null,
		Filter? filter = null,
		float? scoreThreshold = null,
		SearchParams? searchParams = null,
		ulong limit = 10,
		ulong offset = 0,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		LookupLocation? lookupFrom = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Universally query points in batch.
	/// Covers all capabilities of search, recommend, discover, filters.
	/// Also enables hybrid and multi-stage queries.
	///
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="queries">The queries to be performed in the batch.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<BatchResult>> QueryBatchAsync(
		string collectionName,
		IReadOnlyList<QueryPoints> queries,
		ReadConsistency? readConsistency = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Universally query points.
	/// Covers all capabilities of search, recommend, discover, filters.
	/// Grouped by a payload field.
	///
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="groupBy">Payload field to group by, must be a string or number field.</param>
	/// <param name="query">Query to perform. If missing, returns points ordered by their IDs.</param>
	/// <param name="prefetch">Sub-requests to perform first. If present, the query will be performed on the results of the prefetches.</param>
	/// <param name="usingVector">Name of the vector to use for querying. If missing, the default vector is used..</param>
	/// <param name="filter">Filter conditions - return only those points that satisfy the specified conditions.</param>
	/// <param name="scoreThreshold">Return points with scores better than this threshold.</param>
	/// <param name="searchParams">Search config.</param>
	/// <param name="limit">Max number of results.</param>
	/// <param name="groupSize">Maximum amount of points to return per group.</param>
	/// <param name="payloadSelector">Options for specifying which payload to include or not.</param>
	/// <param name="vectorsSelector">Options for specifying which vectors to include into the response.</param>
	/// <param name="readConsistency">Options for specifying read consistency guarantees.</param>
	/// <param name="shardKeySelector">Specify in which shards to look for the points, if not specified - look in all shards.</param>
	/// <param name="withLookup">Options for specifying how to use the group id to lookup points in another collection.</param>
	/// <param name="lookupFrom">The location to use for IDs lookup, if not specified - use the current collection and the 'usingVector' vector</param>
	/// <param name="timeout">If set, overrides global timeout setting for this request.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
	Task<IReadOnlyList<PointGroup>> QueryGroupsAsync(
		string collectionName,
		string groupBy,
		Query? query = null,
		IReadOnlyList<PrefetchQuery>? prefetch = null,
		string? usingVector = null,
		Filter? filter = null,
		float? scoreThreshold = null,
		SearchParams? searchParams = null,
		ulong limit = 10,
		ulong groupSize = 1,
		WithPayloadSelector? payloadSelector = null,
		WithVectorsSelector? vectorsSelector = null,
		ReadConsistency? readConsistency = null,
		ShardKeySelector? shardKeySelector = null,
		WithLookup? withLookup = null,
		LookupLocation? lookupFrom = null,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Get list of snapshots for a collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<SnapshotDescription>> ListSnapshotsAsync(string collectionName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete snapshot for a given collection.
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="snapshotName">The name of the snapshot</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task DeleteSnapshotAsync(string collectionName, string snapshotName, CancellationToken cancellationToken = default);

	/// <summary>
	/// Create snapshot for a whole storage.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<SnapshotDescription> CreateFullSnapshotAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// List all snapshots for a whole storage.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<IReadOnlyList<SnapshotDescription>> ListFullSnapshotsAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete snapshot for a whole storage.
	/// </summary>
	/// <param name="snapshotName">The name of the snapshot</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task DeleteFullSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default);

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
	Task<CreateShardKeyResponse> CreateShardKeyAsync(
		string collectionName,
		CreateShardKey createShardKey,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Delete shard key
	/// </summary>
	/// <param name="collectionName">The name of the collection</param>
	/// <param name="deleteShardKey">Request to delete shard key</param>
	/// <param name="timeout">Wait timeout for operation commit in seconds, if not specified - default value will be supplied</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	Task<DeleteShardKeyResponse> DeleteShardKeyAsync(
		string collectionName,
		DeleteShardKey deleteShardKey,
		TimeSpan? timeout = null,
		CancellationToken cancellationToken = default);

	#endregion Cluster management

	/// <summary>
	/// Asynchronously checks the health of the Qdrant service.
	/// </summary>
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// <returns>
	/// A task that represents the asynchronous operation. The task result contains
	/// a <see cref="HealthCheckReply"/> indicating the health status of the Qdrant service.
	/// </returns>
	/// <exception cref="RpcException">occurs when server is unavailable</exception>
	Task<HealthCheckReply> HealthAsync(CancellationToken cancellationToken = default);
}
