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
	/// Implicitly converts a list of pairs with <see cref="string"/> and <see cref="SparseVectorParams"/> to a new instance
	/// </summary>
	/// <param name="configs">key</param>
	/// <returns>a new instance of <see cref="SparseVectorConfig"/></returns>
	public static implicit operator SparseVectorConfig((string, SparseVectorParams)[] configs) => new()
	{
		Map = { configs.ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2) }
	};
}
