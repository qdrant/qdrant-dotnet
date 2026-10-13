namespace Qdrant.Client.Grpc.Serverless;

/// <summary>
/// The kind of filter a payload field supports.
/// </summary>
public partial class PayloadIndexConfig
{
	/// <summary>
	/// Implicitly converts a <see cref="KeywordIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the keyword index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(KeywordIndex index) => new() { Keyword = index };

	/// <summary>
	/// Implicitly converts an <see cref="IntegerIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the integer index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(IntegerIndex index) => new() { Integer = index };

	/// <summary>
	/// Implicitly converts a <see cref="FloatIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the float index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(FloatIndex index) => new() { Float = index };

	/// <summary>
	/// Implicitly converts a <see cref="UuidIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the UUID index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(UuidIndex index) => new() { Uuid = index };

	/// <summary>
	/// Implicitly converts a <see cref="DatetimeIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the datetime index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(DatetimeIndex index) => new() { Datetime = index };

	/// <summary>
	/// Implicitly converts a <see cref="TextIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the full-text index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(TextIndex index) => new() { Text = index };

	/// <summary>
	/// Implicitly converts a <see cref="GeoIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the geo index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(GeoIndex index) => new() { Geo = index };

	/// <summary>
	/// Implicitly converts a <see cref="BoolIndex"/> to a new instance of <see cref="PayloadIndexConfig"/>
	/// </summary>
	/// <param name="index">the bool index</param>
	/// <returns>a new instance of <see cref="PayloadIndexConfig"/></returns>
	public static implicit operator PayloadIndexConfig(BoolIndex index) => new() { Bool = index };
}
