namespace Qdrant.Client.Grpc;

/// <summary>
/// Selects the points to be affected.
/// </summary>
public partial class PointsSelector
{
	/// <summary>
	/// Implicitly converts a <see cref="Guid" /> ID to a new instance of <see cref="PointsSelector"/>
	/// </summary>
	public static implicit operator PointsSelector(Guid id) =>
		new() { Points = new PointsIdsList { Ids = { new PointId { Uuid = id.ToString() } } } };

	/// <summary>
	/// Implicitly converts a <see cref="ulong" /> ID to a new instance of <see cref="PointsSelector"/>
	/// </summary>
	public static implicit operator PointsSelector(ulong id) =>
		new() { Points = new PointsIdsList { Ids = { new PointId { Num = id } } } };

	/// <summary>
	/// Implicitly converts an array of <see cref="ulong" /> ID to a new instance of <see cref="PointsSelector"/>
	/// </summary>
	public static implicit operator PointsSelector(ulong[] ids) =>
		new() { Points = new PointsIdsList { Ids = { ids.Select(id => (PointId)id) } } };

	/// <summary>
	/// Implicitly converts an array of <see cref="Guid" /> ID to a new instance of <see cref="PointsSelector"/>
	/// </summary>
	public static implicit operator PointsSelector(Guid[] ids) =>
		new() { Points = new PointsIdsList { Ids = { ids.Select(id => (PointId)id) } } };
}
