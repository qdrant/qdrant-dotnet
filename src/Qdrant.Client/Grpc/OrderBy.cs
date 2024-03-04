namespace Qdrant.Client.Grpc;

/// <summary>
/// Order the records by a payload field
/// </summary>
public partial class OrderBy
{
	/// <summary>
	/// Implicitly converts a string to a new instance of <see cref="OrderBy"/>
	/// </summary>
	/// <param name="key">key</param>
	/// <returns>a new instance of <see cref="OrderBy"/></returns>
	public static implicit operator OrderBy(string key) => new() { Key = key };
}
