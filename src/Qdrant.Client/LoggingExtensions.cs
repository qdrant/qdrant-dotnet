using Microsoft.Extensions.Logging;

namespace Qdrant.Client;

internal static partial class LoggingExtensions
{
	[LoggerMessage(1, LogLevel.Debug, "Create collection '{collection}'")]
	public static partial void CreateCollection(this ILogger logger, string collection);

	[LoggerMessage(2, LogLevel.Debug, "Get collection info for '{collection}'")]
	public static partial void GetCollectionInfo(this ILogger logger, string collection);

	[LoggerMessage(3, LogLevel.Debug, "List collections")]
	public static partial void ListCollections(this ILogger logger);

	[LoggerMessage(4, LogLevel.Debug, "Delete collection '{collection}'")]
	public static partial void DeleteCollection(this ILogger logger, string collection);

	[LoggerMessage(5, LogLevel.Debug, "Update collection '{collection}'")]
	public static partial void UpdateCollection(this ILogger logger, string collection);

	#region Alias management

	[LoggerMessage(1000, LogLevel.Debug, "Create alias '{alias}' for collection '{collection}'")]
	public static partial void CreateAlias(this ILogger logger, string alias, string collection);

	[LoggerMessage(1001, LogLevel.Debug, "Delete alias '{alias}'")]
	public static partial void DeleteAlias(this ILogger logger, string alias);

	[LoggerMessage(1002, LogLevel.Debug, "Rename alias '{oldAlias}' to '{newAlias}'")]
	public static partial void RenameAlias(this ILogger logger, string oldAlias, string newAlias);

	[LoggerMessage(1003, LogLevel.Debug, "List aliases for collection '{collection}'")]
	public static partial void ListCollectionAliases(this ILogger logger, string collection);

	[LoggerMessage(1004, LogLevel.Debug, "List all aliases")]
	public static partial void ListAliases(this ILogger logger);

	#endregion Alias management
}
