namespace Qdrant.Grpc;

/// <summary>
/// A vector
/// </summary>
public partial class Vector
{
	/// <summary>
	/// Implicitly converts an array of floats to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(float[] values) => new() { Data = { values } };

	/// <summary>
	/// Implicitly converts an array of int to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="values">the array of int</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(int[] values) => new() { Data = { Array.ConvertAll(values, x => (float)x) } };
}
