namespace Qdrant.Client.Grpc;

/// <summary>
/// Shard key
/// </summary>
public partial class ShardKey
{
	/// <summary>
	/// Implicitly converts a ulong to a new instance of <see cref="ShardKey"/>
	/// </summary>
	/// <param name="key">key</param>
	/// <returns>a new instance of <see cref="ShardKey"/></returns>
	public static implicit operator ShardKey(ulong key) => new() { Number = key };

	/// <summary>
	/// Implicitly converts a ulong to a new instance of <see cref="ShardKey"/>
	/// </summary>
	/// <param name="key">key</param>
	/// <returns>a new instance of <see cref="ShardKey"/></returns>
	public static implicit operator ShardKey(string key) => new() { Keyword = key };
}
