namespace Qdrant.Client.Grpc;

/// <summary>
/// Selects vectors to return
/// </summary>
public partial class WithVectorsSelector
{
	/// <summary>
	/// Implicitly converts <see cref="bool"/> to a new instance of <see cref="WithPayloadSelector"/>
	/// </summary>
	/// <param name="enable">If <code>true</code> return all vectors, if <code>false</code> then none</param>
	/// <returns>a new instance of <see cref="WithPayloadSelector"/></returns>
	public static implicit operator WithVectorsSelector(bool enable) => new() { Enable = enable };

	/// <summary>
	/// Implicitly converts an array <see cref="string"/> to a new instance of <see cref="WithPayloadSelector"/>
	/// </summary>
	/// <param name="vectors">List of vector names to return.</param>
	/// <returns>a new instance of <see cref="WithPayloadSelector"/></returns>
	public static implicit operator WithVectorsSelector(string[] vectors) =>
		new() { Include = new() { Names = { vectors } } };
}
