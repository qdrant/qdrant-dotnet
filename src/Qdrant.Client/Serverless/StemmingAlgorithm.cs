namespace Qdrant.Client.Grpc.Serverless;

/// <summary>
/// Stemming algorithm for a full-text index.
/// </summary>
public partial class StemmingAlgorithm
{
	/// <summary>
	/// Implicitly converts <see cref="SnowballParams"/> to a new instance of <see cref="StemmingAlgorithm"/>
	/// </summary>
	/// <param name="snowball">the snowball stemmer parameters</param>
	/// <returns>a new instance of <see cref="StemmingAlgorithm"/></returns>
	public static implicit operator StemmingAlgorithm(SnowballParams snowball) => new() { Snowball = snowball };

	/// <summary>
	/// Implicitly converts <see cref="DisabledStemmer"/> to a new instance of <see cref="StemmingAlgorithm"/>
	/// </summary>
	/// <param name="disabled">disables stemming</param>
	/// <returns>a new instance of <see cref="StemmingAlgorithm"/></returns>
	public static implicit operator StemmingAlgorithm(DisabledStemmer disabled) => new() { Disabled = disabled };
}
