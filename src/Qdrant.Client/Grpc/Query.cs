namespace Qdrant.Client.Grpc;

/// <summary>
/// A query to perform.
/// </summary>
public partial class Query
{
	/// <summary>
	/// Implicitly converts a <see cref="RecommendInput"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="input">An instance of <see cref="RecommendInput"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(RecommendInput input) => new() { Recommend = input };

	/// <summary>
	/// Implicitly converts a <see cref="DiscoverInput"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="input">An instance of <see cref="DiscoverInput"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(DiscoverInput input) => new() { Discover = input };

	/// <summary>
	/// Implicitly converts a <see cref="ContextInput"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="input">An instance of <see cref="ContextInput"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(ContextInput input) => new() { Context = input };

	/// <summary>
	/// Implicitly converts a <see cref="Fusion"/>  to a new instance of <see cref="Query"/> to combine pre-fetch results.
	/// </summary>
	/// <param name="input">A <see cref="Fusion"/> value</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(Fusion input) => new() { Fusion = input };

	/// <summary>
	/// Implicitly converts a <see cref="OrderBy"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="orderBy">An instance of <see cref="OrderBy"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(OrderBy orderBy) => new() { OrderBy = orderBy };

	/// <summary>
	/// Implicitly converts a <see cref="Sample"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="sample">An instance of <see cref="Sample"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(Sample sample) => new() { Sample = sample };

	/// <summary>
	/// Implicitly converts a <see cref="Formula"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="formula">An instance of <see cref="Formula"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(Formula formula) => new() { Formula = formula };

	/// <summary>
	/// Explicitly creates a <see cref="Query"/> to order points by a payload field.
	/// </summary>
	/// <param name="key">Name of the payload field to order by</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static explicit operator Query(string key) => new() { OrderBy = new() { Key = key } };

	#region Nearest

	/// <summary>
	/// Implicitly converts a <see cref="VectorInput"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="input">An instance of <see cref="VectorInput"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(VectorInput input) => new() { Nearest = input };

	/// <summary>
	/// Implicitly converts a <see cref="PointId"/>  to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="id">An instance of <see cref="VectorInput"/> </param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(PointId id) => new() { Nearest = (VectorInput)id };

	/// <summary>
	/// Implicitly converts a ulong to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="id">the id</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(ulong id) => new() { Nearest = (VectorInput)id };

	/// <summary>
	/// Implicitly converts a <see cref="Guid"/> to a new instance of <see cref="PointId"/>
	/// </summary>
	/// <param name="id">the id</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(Guid id) => new() { Nearest = (VectorInput)id };

	/// <summary>
	/// Implicitly converts an array of <see cref="float"/> to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(float[] values) => new() { Nearest = (VectorInput)values };

	/// <summary>
	/// Implicitly converts a tuple of sparse values array of <see cref="float"/>
	/// and sparse indices array of <see cref="uint"/> to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="sparseValues">a tuple of arrays of values and indices</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query((float[], uint[]) sparseValues) => new() { Nearest = (VectorInput)sparseValues };

	/// <summary>
	/// Implicitly converts an array of sparse value index tuples to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="sparseValues">the array of value-index pairs</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query((float, uint)[] sparseValues) => new() { Nearest = (VectorInput)sparseValues };

	/// <summary>
	/// Implicitly converts a nested array of <see cref="float"/> representing a multi-vector
	/// to a new instance of <see cref="Query"/>
	/// </summary>
	/// <param name="values">the array of floats</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(float[][] values) => new() { Nearest = (VectorInput)values };

	/// <summary>
	/// Implicitly converts an instance of <see cref="Document"/> to a new instance of <see cref="Query"/> for cloud inference.
	/// </summary>
	/// <param name="document">An instance of <see cref="Document"/> to query against</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(Document document) => new() { Nearest = (VectorInput)document };

	/// <summary>
	/// Implicitly converts an instance of <see cref="Image"/> to a new instance of <see cref="Query"/> for cloud inference.
	/// </summary>
	/// <param name="image">An instance of <see cref="Image"/> to query against</param>
	public static implicit operator Query(Image image) => new() { Nearest = (VectorInput)image };

	/// <summary>
	/// Implicitly converts an instance of <see cref="InferenceObject"/> to a new instance of <see cref="Query"/> for cloud inference.
	/// </summary>
	/// <param name="inferenceObject">An instance of <see cref="InferenceObject"/> to query against</param>
	/// <returns>a new instance of <see cref="Query"/></returns>
	public static implicit operator Query(InferenceObject inferenceObject) => new() { Nearest = (VectorInput)inferenceObject };

	#endregion
}
