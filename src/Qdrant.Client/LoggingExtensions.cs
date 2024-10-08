using Microsoft.Extensions.Logging;

namespace Qdrant.Client;

internal static partial class LoggingExtensions
{
	#region Collection management

	[LoggerMessage(1000, LogLevel.Debug, "Create collection '{collection}'")]
	public static partial void CreateCollection(this ILogger logger, string collection);

	[LoggerMessage(1001, LogLevel.Debug, "Get collection info for '{collection}'")]
	public static partial void GetCollectionInfo(this ILogger logger, string collection);

	[LoggerMessage(1003, LogLevel.Debug, "List collections")]
	public static partial void ListCollections(this ILogger logger);

	[LoggerMessage(1004, LogLevel.Debug, "Delete collection '{collection}'")]
	public static partial void DeleteCollection(this ILogger logger, string collection);

	[LoggerMessage(1005, LogLevel.Debug, "Update collection '{collection}'")]
	public static partial void UpdateCollection(this ILogger logger, string collection);

	[LoggerMessage(1006, LogLevel.Debug, "Collection exists '{collection}'")]
	public static partial void CollectionExists(this ILogger logger, string collection);

	#endregion Collection management

	#region Alias management

	[LoggerMessage(2000, LogLevel.Debug, "Create alias '{alias}' for collection '{collection}'")]
	public static partial void CreateAlias(this ILogger logger, string alias, string collection);

	[LoggerMessage(2001, LogLevel.Debug, "Delete alias '{alias}'")]
	public static partial void DeleteAlias(this ILogger logger, string alias);

	[LoggerMessage(2002, LogLevel.Debug, "Rename alias '{oldAlias}' to '{newAlias}'")]
	public static partial void RenameAlias(this ILogger logger, string oldAlias, string newAlias);

	[LoggerMessage(2003, LogLevel.Debug, "List aliases for collection '{collection}'")]
	public static partial void ListCollectionAliases(this ILogger logger, string collection);

	[LoggerMessage(2004, LogLevel.Debug, "List all aliases")]
	public static partial void ListAliases(this ILogger logger);

	#endregion Alias management

	#region Point management

	[LoggerMessage(3004, LogLevel.Debug, "Upsert {count} points into '{collection}'")]
	public static partial void Upsert(this ILogger logger, string collection, int count);

	[LoggerMessage(3005, LogLevel.Debug, "Delete from '{collection}'")]
	public static partial void Delete(this ILogger logger, string collection);

	[LoggerMessage(3006, LogLevel.Debug, "Retrieve points from '{collection}'")]
	public static partial void Retrieve(this ILogger logger, string collection);

	[LoggerMessage(3007, LogLevel.Debug, "Update vectors in '{collection}'")]
	public static partial void UpdateVectors(this ILogger logger, string collection);

	[LoggerMessage(3008, LogLevel.Debug, "Delete vectors in '{collection}'")]
	public static partial void DeleteVectors(this ILogger logger, string collection);

	[LoggerMessage(3009, LogLevel.Debug, "Set payload in '{collection}'")]
	public static partial void SetPayload(this ILogger logger, string collection);

	[LoggerMessage(3010, LogLevel.Debug, "Overwrite payload in '{collection}'")]
	public static partial void OverwritePayload(this ILogger logger, string collection);

	[LoggerMessage(3011, LogLevel.Debug, "Delete payload in '{collection}'")]
	public static partial void DeletePayload(this ILogger logger, string collection);

	[LoggerMessage(3012, LogLevel.Debug, "Clear payload in '{collection}'")]
	public static partial void ClearPayload(this ILogger logger, string collection);

	[LoggerMessage(3013, LogLevel.Debug, "Create payload field index in '{collection}'")]
	public static partial void CreatePayloadIndex(this ILogger logger, string collection);

	[LoggerMessage(3014, LogLevel.Debug, "Delete payload field index in '{collection}'")]
	public static partial void DeletePayloadIndex(this ILogger logger, string collection);

	[LoggerMessage(3015, LogLevel.Debug, "Search on '{collection}'")]
	public static partial void Search(this ILogger logger, string collection);

	[LoggerMessage(3016, LogLevel.Debug, "Search batch on '{collection}'")]
	public static partial void SearchBatch(this ILogger logger, string collection);

	[LoggerMessage(3017, LogLevel.Debug, "Search groups on '{collection}'")]
	public static partial void SearchGroups(this ILogger logger, string collection);

	[LoggerMessage(3018, LogLevel.Debug, "Scroll on '{collection}'")]
	public static partial void Scroll(this ILogger logger, string collection);

	[LoggerMessage(3019, LogLevel.Debug, "Recommend on '{collection}'")]
	public static partial void Recommend(this ILogger logger, string collection);

	[LoggerMessage(3020, LogLevel.Debug, "Recommend batch on '{collection}'")]
	public static partial void RecommendBatch(this ILogger logger, string collection);

	[LoggerMessage(3021, LogLevel.Debug, "Recommend groups on '{collection}'")]
	public static partial void RecommendGroups(this ILogger logger, string collection);

	[LoggerMessage(3022, LogLevel.Debug, "Update batch on '{collection}'")]
	public static partial void UpdateBatch(this ILogger logger, string collection);

	[LoggerMessage(3023, LogLevel.Debug, "Count points in '{collection}'")]
	public static partial void Count(this ILogger logger, string collection);

	[LoggerMessage(3024, LogLevel.Debug, "Discover on '{collection}'")]
	public static partial void Discover(this ILogger logger, string collection);

	[LoggerMessage(3025, LogLevel.Debug, "Discover batch on '{collection}'")]
	public static partial void DiscoverBatch(this ILogger logger, string collection);

	[LoggerMessage(3026, LogLevel.Debug, "Query on '{collection}'")]
	public static partial void Query(this ILogger logger, string collection);

	[LoggerMessage(3027, LogLevel.Debug, "Query batch on '{collection}'")]
	public static partial void QueryBatch(this ILogger logger, string collection);

	[LoggerMessage(3028, LogLevel.Debug, "Query groups on '{collection}'")]
	public static partial void QueryGroups(this ILogger logger, string collection);

	[LoggerMessage(3029, LogLevel.Debug, "Facet on '{collection}'")]
	public static partial void Facet(this ILogger logger, string collection);

	[LoggerMessage(3030, LogLevel.Debug, "Search matrix pairs on '{collection}'")]
	public static partial void SearchMatrixPairs(this ILogger logger, string collection);

	[LoggerMessage(3031, LogLevel.Debug, "Search matrix offsets on '{collection}'")]
	public static partial void SearchMatrixOffsets(this ILogger logger, string collection);

	#endregion Point management

	#region Snapshot management

	[LoggerMessage(4000, LogLevel.Debug, "Create snapshot of '{collection}'")]
	public static partial void CreateSnapshot(this ILogger logger, string collection);

	[LoggerMessage(4001, LogLevel.Debug, "List snapshots for '{collection}'")]
	public static partial void ListSnapshots(this ILogger logger, string collection);

	[LoggerMessage(4002, LogLevel.Debug, "Delete snapshot '{snapshot}' for '{collection}'")]
	public static partial void DeleteSnapshot(this ILogger logger, string collection, string snapshot);

	[LoggerMessage(4003, LogLevel.Debug, "Create snapshot for a whole storage")]
	public static partial void CreateFullSnapshot(this ILogger logger);

	[LoggerMessage(4004, LogLevel.Debug, "List snapshots for a whole storage")]
	public static partial void ListFullSnapshots(this ILogger logger);

	[LoggerMessage(4005, LogLevel.Debug, "Delete snapshot '{snapshot}' for a whole storage")]
	public static partial void DeleteFullSnapshot(this ILogger logger, string snapshot);

	#endregion

	#region Cluster management

	[LoggerMessage(5000, LogLevel.Debug, "Create shard key '{key}' for collection '{collection}'")]
	public static partial void CreateShardKey(this ILogger logger, string key, string collection);

	[LoggerMessage(5001, LogLevel.Debug, "Delete shard key '{key}' for collection '{collection}'")]
	public static partial void DeleteShardKey(this ILogger logger, string key, string collection);

	#endregion

	[LoggerMessage(99999, LogLevel.Error, "Operation failed: {operation}")]
	public static partial void OperationFailed(this ILogger logger, string operation, Exception exception);
}
