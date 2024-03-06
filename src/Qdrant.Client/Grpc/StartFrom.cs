using Google.Protobuf.WellKnownTypes;

namespace Qdrant.Client.Grpc;

/// <summary>
/// Start ordered records from the specified value
/// </summary>
public partial class StartFrom
{
	/// <summary>
	/// Implicitly converts a datetime string to a new instance of <see cref="StartFrom"/>
	/// </summary>
	/// <param name="value">Datetime string value</param>
	/// <returns>a new instance of <see cref="StartFrom"/></returns>
	public static implicit operator StartFrom(string value) => new() { Datetime = value };

	/// <summary>
	/// Implicitly converts a double to a new instance of <see cref="StartFrom"/>
	/// </summary>
	/// <param name="value">Double value</param>
	/// <returns>a new instance of <see cref="StartFrom"/></returns>
	public static implicit operator StartFrom(double value) => new() { Float = value };

	/// <summary>
	/// Implicitly converts an integer to a new instance of <see cref="StartFrom"/>
	/// </summary>
	/// <param name="value">Integer value</param>
	/// <returns>a new instance of <see cref="StartFrom"/></returns>
	public static implicit operator StartFrom(int value) => new() { Integer = value };

	/// <summary>
	/// Implicitly converts <see cref="Datetime"/> to a new instance of <see cref="StartFrom"/>
	/// </summary>
	/// <param name="value">Integer value</param>
	/// <returns>a new instance of <see cref="StartFrom"/></returns>
	public static implicit operator StartFrom(DateTime value) => new() { Timestamp = Timestamp.FromDateTime(value) };

	/// <summary>
	/// Implicitly converts <see cref="Timestamp"/> to a new instance of <see cref="StartFrom"/>
	/// </summary>
	/// <param name="value">Integer value</param>
	/// <returns>a new instance of <see cref="StartFrom"/></returns>
	public static implicit operator StartFrom(Timestamp value) => new() { Timestamp = value };
}
