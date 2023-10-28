using Grpc.Core;
using Grpc.Net.Client;

namespace Qdrant.Client.Grpc;

/// <summary>
/// Low-level gRPC client for qdrant vector database. Consider using <see cref="QdrantClient" /> instead.
/// </summary>
public partial class QdrantGrpcClient : IDisposable
{
	private readonly CallInvoker _callInvoker;
	private readonly QdrantChannel? _ownedChannel;
	private bool _isDisposed;

	/// <summary>
	/// Creates a new low-level gRPC client for Qdrant. Note that <see cref="QdrantClient" /> provides a higher-level,
	/// easier to use alternative.
	/// </summary>
	/// <param name="host">The host to connect to.</param>
	/// <param name="port">The port to connect to. Defaults to 6334.</param>
	/// <param name="apiKey">The API key to use.</param>
	public QdrantGrpcClient(string host, int port = 6334, string? apiKey = null)
		: this(new UriBuilder("http", host, port).Uri, apiKey)
	{
	}

	/// <summary>
	/// Creates a new low-level gRPC client for Qdrant. Note that <see cref="QdrantClient" /> provides a higher-level,
	/// easier to use alternative.
	/// </summary>
	/// <param name="address">The address to connect to.</param>
	/// <param name="apiKey">The API key to use.</param>
	public QdrantGrpcClient(System.Uri address, string? apiKey = null)
	{
		_ownedChannel = QdrantChannel.ForAddress(address, new() { ApiKey = apiKey });
		_callInvoker = _ownedChannel.CreateCallInvoker();
	}

	/// <summary>
	/// Creates a new low-level gRPC client for Qdrant. Note that <see cref="QdrantClient" /> provides a higher-level,
	/// easier to use alternative.
	/// </summary>
	/// <param name="channel">The channel to use to make remote calls.</param>
	/// <remarks>It is the responsibility of the caller to dispose of <paramref name="channel" />.</remarks>
	public QdrantGrpcClient(ChannelBase channel) : this(channel.CreateCallInvoker())
	{
	}

	/// <summary>
	/// Creates a new low-level gRPC client for Qdrant, which uses a custom <c>CallInvoker</c>.
	/// Note that <see cref="QdrantClient" /> provides a higher-level, easier to use alternative.
	/// </summary>
	/// <summary>Creates a new client for Qdrant that uses a custom <c>CallInvoker</c>.</summary>
	/// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
	/// <remarks>It is the responsibility of the caller to dispose of any resources associated with the
	/// <paramref name="callInvoker" /> (e.g. the underlying <see cref="GrpcChannel" />.</remarks>
	public QdrantGrpcClient(CallInvoker callInvoker)
		=> _callInvoker = callInvoker;

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

	/// <inheritdoc />
	public void Dispose()
	{
		if (_isDisposed)
			return;

		_ownedChannel?.Dispose();
		_isDisposed = true;
	}
}
