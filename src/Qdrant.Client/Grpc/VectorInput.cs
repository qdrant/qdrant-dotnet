namespace Qdrant.Client.Grpc;

/// <summary>
/// A vector input for queries
/// </summary>
public partial class VectorInput
{
	/// <summary>
	/// Implicitly converts an array of <see cref="float"/> to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput(float[] values) =>
		new() { Dense = new() { Data = { values } } };

	/// <summary>
	/// Implicitly converts a tuple of sparse values array of <see cref="float"/>
	/// and sparse indices array of <see cref="uint"/> to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="sparseValues">a tuple of arrays of values and indices</param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput((float[], uint[]) sparseValues) =>
		new()
		{
			Sparse = new() { Values = { sparseValues.Item1 }, Indices = { sparseValues.Item2 } },
		};

	/// <summary>
	/// Implicitly converts an array of sparse value index tuples to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="sparseValues">the array of value-index pairs</param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput((float, uint)[] sparseValues) =>
			new()
			{
				Sparse = new() { Values = { sparseValues.Select(v => v.Item1) }, Indices = { sparseValues.Select(v => v.Item2) } },
			};

	/// <summary>
	/// Implicitly converts a nested array of <see cref="float"/> representing a multi-vector
	/// to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput(float[][] values)
	{
		var denseVectors = values.Select(v => new DenseVector { Data = { v } });

		return new()
		{
			MultiDense = new()
			{
				Vectors = { denseVectors }
			}
		};
	}

	/// <summary>
	/// Implicitly converts a <see cref="PointId"/> to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="id">The id of the point</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator VectorInput(PointId id) => new()
	{
		Id = id
	};

	/// <summary>
	/// Implicitly converts a <see cref="ulong"/> ID to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="id">The id of the point</param>
	/// <returns>a new instance of <see cref="Vector"/></returns>
	public static implicit operator VectorInput(ulong id) => new()
	{
		Id = id
	};

	/// <summary>
	/// Implicitly converts a <see cref="Guid"/> IDto a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="id">The id of the point</param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput(Guid id) => new()
	{
		Id = id
	};

	/// <summary>
	/// Implicitly converts a <see cref="Document"/> to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="document">an instance of <see cref="Document"/></param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput(Document document) => new()
	{
		Document = document
	};

	/// <summary>
	/// Implicitly converts an <see cref="Image"/> to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="image">an instance of <see cref="Image"/></param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput(Image image) => new()
	{
		Image = image
	};

	/// <summary>
	/// Implicitly converts an <see cref="InferenceObject"/> to a new instance of <see cref="VectorInput"/>
	/// </summary>
	/// <param name="obj">an instance of <see cref="InferenceObject"/></param>
	/// <returns>a new instance of <see cref="VectorInput"/></returns>
	public static implicit operator VectorInput(InferenceObject obj) => new()
	{
		Object = obj
	};
}
