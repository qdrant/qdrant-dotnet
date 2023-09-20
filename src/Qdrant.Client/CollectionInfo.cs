using Qdrant.Client.Grpc;

namespace Qdrant.Client;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class CollectionInfo
{
	/// <summary>
	/// The operating condition of the collection.
	/// </summary>
	public CollectionStatus Status { get; init; }

	// /// <summary>
	// /// The status of the collection's optimizers.
	// /// </summary>
	// public OptimizerStatus OptimizerStatus { get; init; }

	/// <summary>
	/// The number of vectors in the collection.
	/// </summary>
	public ulong VectorsCount { get; init; }

	/// <summary>
	/// The number of independent segments in the collection.
	/// </summary>
	public ulong SegmentsCount { get; init; }

	// /// <summary>
	// /// The collection's data types.
	// /// </summary>
	// public IReadOnlyList<PayloadSchemaEntry> PayloadSchema { get; init; }

	/// <summary>
	/// The number of points (vectors + payloads) in the collection.
	/// </summary>
	public ulong PointsCount { get; init; }

	/// <summary>
	/// The number of indexed vectors in the collection.
	/// Indexed vectors in large segments are faster to query, as they are stored in the vector index (HNSW).
	/// </summary>
	public ulong? IndexedVectorsCount { get; set; }
}
