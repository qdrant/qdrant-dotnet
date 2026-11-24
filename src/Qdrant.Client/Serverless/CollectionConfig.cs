namespace Qdrant.Client.Grpc.Serverless;

/// <summary>
/// The configuration of a serverless collection.
/// </summary>
public partial class CollectionConfig
{
	/// <summary>
	/// Implicitly converts a <see cref="DenseVectorConfig"/> to a new instance of <see cref="CollectionConfig"/>
	/// with a single unnamed (default) dense vector.
	/// </summary>
	/// <param name="vector">the configuration of the default vector</param>
	/// <returns>a new instance of <see cref="CollectionConfig"/></returns>
	public static implicit operator CollectionConfig(DenseVectorConfig vector) =>
		new() { DenseVectors = { [""] = vector } };
}
