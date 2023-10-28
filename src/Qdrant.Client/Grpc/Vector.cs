namespace Qdrant.Client.Grpc;

/// <summary>
/// A vector
/// </summary>
public partial class Vector
{
	/// <summary>
	/// Implicitly converts an array of <see cref="float"/> to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(float[] values) => new() { Data = { values } };
}
