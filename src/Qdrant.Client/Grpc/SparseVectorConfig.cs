namespace Qdrant.Client.Grpc;

/// <summary>
/// Sparse vector config
/// </summary>
public partial class SparseVectorConfig
{
	/// <summary>
	/// Implicitly converts a dictionary of <see cref="string"/> and <see cref="SparseVectorParams"/> to a new instance
	/// </summary>
	/// <param name="configs">key</param>
	/// <returns>a new instance of <see cref="SparseVectorConfig"/></returns>
	public static implicit operator SparseVectorConfig(Dictionary<string, SparseVectorParams> configs) => new()
	{
		Map = { configs }
	};

	/// <summary>
	/// Implicitly converts a tuple of <see cref="string"/> and <see cref="SparseVectorParams"/> to a new instance
	/// </summary>
	/// <param name="config">tuple of key and config</param>
	/// <returns>a new instance of <see cref="SparseVectorConfig"/></returns>
	public static implicit operator SparseVectorConfig((string, SparseVectorParams) config) => new()
	{
		Map = { [config.Item1] = config.Item2 }
	};
}
