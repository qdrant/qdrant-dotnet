namespace Qdrant.Client.Grpc;

/// <summary>
/// Filtering to apply
/// </summary>
public partial class Filter
{
	/// <summary>
	/// Implicitly converts a <see cref="Condition"/> to a new instance of <see cref="Filter"/>
	/// </summary>
	/// <param name="condition">the condition</param>
	/// <returns>a new instance of <see cref="Filter"/></returns>
	public static implicit operator Filter(Condition condition) =>
		new() { Must = { condition } };
}
