namespace Qdrant.Grpc;

/// <summary>
/// The id of a point
/// </summary>
public partial class PointId
{
	/// <summary>
	/// Implicitly converts a ulong to a new instance of <see cref="PointId"/>
	/// </summary>
	/// <param name="id">the id</param>
	/// <returns>a new instance of <see cref="PointId"/></returns>
	public static implicit operator PointId(ulong id) => new() { Num = id };

	/// <summary>
	/// Implicitly converts a <see cref="Guid"/> to a new instance of <see cref="PointId"/>
	/// </summary>
	/// <param name="id">the id</param>
	/// <returns>a new instance of <see cref="PointId"/></returns>
	public static implicit operator PointId(Guid id) => new() { Uuid = id.ToString() };
}
