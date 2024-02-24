namespace Qdrant.Client.Grpc;

/// <summary>
/// Selects payload to return
/// </summary>
public partial class WithPayloadSelector
{
	/// <summary>
	/// Implicitly converts <see cref="bool"/> to a new instance of <see cref="WithPayloadSelector"/>
	/// </summary>
	/// <param name="enable">If <code>true</code> return all payload, if <code>false</code> then none</param>
	/// <returns>a new instance of <see cref="WithPayloadSelector"/></returns>
	public static implicit operator WithPayloadSelector(bool enable) => new() { Enable = enable };

	/// <summary>
	/// Implicitly converts <see cref="string"/> to a new instance of <see cref="WithPayloadSelector"/>
	/// </summary>
	/// <param name="fields">List of fields in the payload to return.</param>
	/// <returns>a new instance of <see cref="WithPayloadSelector"/></returns>
	public static implicit operator WithPayloadSelector(string[] fields) =>
		new() { Include = new PayloadIncludeSelector { Fields = { fields } } };
}
