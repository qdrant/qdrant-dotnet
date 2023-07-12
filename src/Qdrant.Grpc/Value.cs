namespace Qdrant.Grpc;

public partial class Value
{
	/// <summary>
	/// Implicitly converts a string to a new instance of <see cref="Value"/>
	/// </summary>
	/// <param name="value">the string</param>
	/// <returns>a new instance of <see cref="Value"/></returns>
	public static implicit operator Value(string value) => new() { StringValue = value };

	/// <summary>
	/// Implicitly converts a long to a new instance of <see cref="Value"/>
	/// </summary>
	/// <param name="value">the long</param>
	/// <returns>a new instance of <see cref="Value"/></returns>
	public static implicit operator Value(long value) => new() { IntegerValue = value };

	/// <summary>
	/// Implicitly converts a bool to a new instance of <see cref="Value"/>
	/// </summary>
	/// <param name="value">the bool</param>
	/// <returns>a new instance of <see cref="Value"/></returns>
	public static implicit operator Value(bool value) => new() { BoolValue = value };

	/// <summary>
	/// Implicitly converts a double to a new instance of <see cref="Value"/>
	/// </summary>
	/// <param name="value">the double</param>
	/// <returns>a new instance of <see cref="Value"/></returns>
	public static implicit operator Value(double value) => new() { DoubleValue = value };

	/// <summary>
	/// Implicitly converts an array of strings to a new instance of <see cref="Value"/>
	/// </summary>
	/// <param name="values">the array of strings</param>
	/// <returns>a new instance of <see cref="Value"/></returns>
	public static implicit operator Value(string[] values)
	{
		var listValues = values.Select(s => new Value { StringValue = s });
		return new Value { ListValue = new ListValue { Values = { listValues } } };
	}
}
