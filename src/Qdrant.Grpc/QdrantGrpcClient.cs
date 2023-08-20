using Grpc.Core;

namespace Qdrant.Grpc;

/// <summary>
/// gRPC client for qdrant vector database
/// </summary>
public partial class QdrantGrpcClient
{
	private readonly CallInvoker _callInvoker;

	/// <summary>Creates a new client for Qdrant</summary>
	/// <param name="channel">The channel to use to make remote calls.</param>
	public QdrantGrpcClient(ChannelBase channel) : this(channel.CreateCallInvoker())
	{
	}

	/// <summary>Creates a new client for Qdrant that uses a custom <c>CallInvoker</c>.</summary>
	/// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
	public QdrantGrpcClient(CallInvoker callInvoker) => _callInvoker = callInvoker;

	/// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
	protected QdrantGrpcClient() : this(new UnimplementedCallInvoker())
	{
	}

	/// <summary>
	/// Gets the client for Qdrant services
	/// </summary>
	public virtual Qdrant.QdrantClient Qdrant => new(_callInvoker);

	/// <summary>
	/// Gets the client for Points
	/// </summary>
	public virtual Points.PointsClient Points => new(_callInvoker);

	/// <summary>
	/// Gets the client for Collections
	/// </summary>
	public virtual Collections.CollectionsClient Collections => new(_callInvoker);

	/// <summary>
	/// Gets the client for Snapshots
	/// </summary>
	public virtual Snapshots.SnapshotsClient Snapshots => new(_callInvoker);
}
