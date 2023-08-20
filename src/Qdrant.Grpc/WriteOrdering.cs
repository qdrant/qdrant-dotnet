namespace Qdrant.Grpc;

/// <summary>
/// Write ordering can be specified for any write request to serialize it through a single “leader” node, which
/// ensures that all write operations (issued with the same ordering) are performed and observed sequentially.
/// </summary>
public partial class WriteOrdering
{
	/// <summary>
	/// Implicitly converts a <see cref="WriteOrderingType"/> to a new instance of <see cref="WriteOrdering"/>
	/// </summary>
	/// <param name="writeOrderingType">the write ordering type</param>
	/// <returns>a new instance of <see cref="WriteOrdering"/></returns>
	public static implicit operator WriteOrdering(WriteOrderingType writeOrderingType) => new() { Type = writeOrderingType };
}
