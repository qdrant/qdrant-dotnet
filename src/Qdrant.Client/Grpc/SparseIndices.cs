namespace Qdrant.Client.Grpc;

/// <summary>
/// Indices of sparse vector
/// </summary>
public partial class SparseIndices
{
	/// <summary>
	/// Implicitly converts an array of <see cref="uint"/> to a new instance of <see cref="SparseIndices"/>
	/// </summary>
	/// <param name="indices">key</param>
	/// <returns>a new instance of <see cref="SparseIndices"/></returns>
	public static implicit operator SparseIndices(uint[] indices) => new() { Data = { indices } };
}
