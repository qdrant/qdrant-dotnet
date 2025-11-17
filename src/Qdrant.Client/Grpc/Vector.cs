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
	public static implicit operator Vector(float[] values) => new()
	{
		Dense = new DenseVector { Data = { values } }
	};

	/// <summary>
	/// Implicitly converts a tuple of sparse values array of <see cref="float"/>
	/// and sparse indices array of <see cref="uint"/> to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="sparseValues">a tuple of arrays of values and indices</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector((float[], uint[]) sparseValues) => new()
	{
		Sparse = new SparseVector { Values = { sparseValues.Item1 }, Indices = { sparseValues.Item2 } }
	};

	/// <summary>
	/// Implicitly converts an array of sparse value index tuples to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="sparseValues">the array of value-index pairs</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector((float, uint)[] sparseValues) => new()
	{
		Sparse = new SparseVector { Values = { sparseValues.Select(v => v.Item1) }, Indices = { sparseValues.Select(v => v.Item2) } }
	};

	/// <summary>
	/// Implicitly converts a nested array of <see cref="float"/> representing a multi-vector
	/// to a new instance of <see cref="Vector"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(float[][] values)
	{
		var denseVectors = values.Select(v => new DenseVector { Data = { v } }).ToArray();

		return new Vector
		{
			MultiDense = new MultiDenseVector { Vectors = { denseVectors } },
		};

	}

	/// <summary>
	/// Implicitly converts an instance of <see cref="Document"/> to a new instance of <see cref="Vector"/> for cloud inference.
	/// </summary>
	/// <param name="document">An instance of <see cref="Document"/> to vectorize</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(Document document) => new()
	{
		Document = document,
	};


	/// <summary>
	/// Implicitly converts an instance of <see cref="Image"/> to a new instance of <see cref="Vector"/> for cloud inference.
	/// </summary>
	/// <param name="image">An instance of <see cref="Image"/> to vectorize</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(Image image) => new()
	{
		Image = image,
	};

	/// <summary>
	/// Implicitly converts an instance of <see cref="InferenceObject"/> to a new instance of <see cref="Vector"/> for cloud inference.
	/// </summary>
	/// <param name="inferenceObject">An instance of <see cref="InferenceObject"/> to vectorize</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator Vector(InferenceObject inferenceObject) => new()
	{
		Object = inferenceObject,
	};
}
