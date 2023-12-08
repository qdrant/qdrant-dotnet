namespace Qdrant.Client.Grpc;

/// <summary>
/// A single vector or map of named vectors.
/// </summary>
public partial class Vectors
{
	/// <summary>
	/// Implicitly converts an array of <see cref="float"/> to a new instance of <see cref="Vectors"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="Vectors"/></returns>
	public static implicit operator Vectors(float[] values) =>
		new() { Vector = new() { Data = { values } } };

	/// <summary>
	/// Implicitly converts a dictionary of <see cref="string"/> and array of <see cref="float"/> to a new instance
	/// of <see cref="Vectors"/>
	/// </summary>
	/// <param name="values">a dictionary of string and array of floats</param>
	/// <returns>a new instance of <see cref="Vectors"/></returns>
	public static implicit operator Vectors(Dictionary<string, float[]> values)
	{
		var namedVectors = new NamedVectors();
		foreach (var value in values)
			namedVectors.Vectors.Add(value.Key, value.Value);

		return new Vectors { Vectors_ = namedVectors };
	}

	/// <summary>
	/// Implicitly converts a dictionary of <see cref="string"/> and <see cref="Vector"/> to a new instance
	/// of <see cref="Vectors"/>
	/// </summary>
	/// <param name="values">a dictionary of string and vectors</param>
	/// <returns>a new instance of <see cref="Vectors"/></returns>
	public static implicit operator Vectors(Dictionary<string, Vector> values)
	{
		var namedVectors = new NamedVectors();
		foreach (var value in values)
			namedVectors.Vectors.Add(value.Key, value.Value);

		return new Vectors { Vectors_ = namedVectors };
	}

    /// <summary>
	/// Implicitly converts a tuple of <see cref="string"/> and <see cref="Vector"/> to a new instance
	/// of <see cref="Vectors"/>
	/// </summary>
	/// <param name="value">a tuple of string and vectors</param>
	/// <returns>a new instance of <see cref="Vectors"/></returns>
	public static implicit operator Vectors((string, Vector) value)
	{
		var namedVectors = new NamedVectors { Vectors = { [value.Item1] = value.Item2 }};
		return new Vectors { Vectors_ = namedVectors };
	}
}
