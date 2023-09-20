using Qdrant.Client.Grpc;

namespace Qdrant.Client;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class QdrantClient
{
	private readonly QdrantGrpcClient _grpcClient;
	private readonly Collections.CollectionsClient _collectionsClient;

	public QdrantClient(QdrantGrpcClient grpcClient)
	{
		_grpcClient = grpcClient;
		_collectionsClient = grpcClient.Collections;
	}

	/// <summary>
	/// Creates a new collection with the given parameters.
	/// </summary>
	/// <param name="name">The name of the collection to be created.</param>
	/// <param name="vectorsConfig">The configuration for the vectors.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task CreateCollectionAsync(
		string name,
		VectorsConfig vectorsConfig,
		CancellationToken cancellationToken = default)
	{
		// TODO: Logging
		// TODO: pass timeout configured at the client level

		var request = new CreateCollection
		{
			CollectionName = name,
			VectorsConfig = vectorsConfig
		};

		var response = await _collectionsClient.CreateAsync(request, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		if (!response.Result)
		{
			throw new QdrantException("Collection could not be created");
		}
	}

	/// <summary>
	/// Gets detailed information about an existing collection.
	/// </summary>
	/// <param name="name">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<CollectionInfo> GetCollectionInfoAsync(
		string name,
		CancellationToken cancellationToken = default)
	{
		// TODO: Logging

		var response = await _collectionsClient.GetAsync(
				new GetCollectionInfoRequest { CollectionName = name }, cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		var grpcCollectionInfo = response.Result;

		return new CollectionInfo
		{
			Status = grpcCollectionInfo.Status,
			// TODO: OptimizerStatus
			VectorsCount = grpcCollectionInfo.VectorsCount,
			SegmentsCount = grpcCollectionInfo.SegmentsCount,
			// TODO: PayloadSchema
			PointsCount = grpcCollectionInfo.PointsCount,
			IndexedVectorsCount = grpcCollectionInfo.IndexedVectorsCount
		};
	}
}
