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

	/// <summary>
	/// Implicitly converts an tuple of sparse values array of <see cref="float"/> 
	/// and sparse indices array of <see cref="uint"/> to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="sparse_values">list of values and indices</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector((float[], uint[]) sparse_values) => new()
	{
		Data = { sparse_values.Item1 },
		Indices = new SparseIndices { Data = { sparse_values.Item2 } }
	};

	/// <summary>
	/// Implicitly converts an array of sparse vector value-index pairs to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="sparse_values">the array of value-index pairs</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector((float, uint)[] sparse_values) => new()
	{
		Data = { sparse_values.Select(v => v.Item1) },
		Indices = new SparseIndices { Data = { sparse_values.Select(v => v.Item2) } }
	};
}
