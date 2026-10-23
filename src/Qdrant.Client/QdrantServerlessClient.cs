using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Qdrant.Client.Grpc;
using Qdrant.Client.Grpc.Serverless;
using CollectionConfig = Qdrant.Client.Grpc.Serverless.CollectionConfig;
using ListCollectionsRequest = Qdrant.Client.Grpc.Serverless.ListCollectionsRequest;
using ListCollectionsResponse = Qdrant.Client.Grpc.Serverless.ListCollectionsResponse;

namespace Qdrant.Client;

/// <summary>
/// Client for Qdrant Serverless.
/// </summary>
/// <remarks>
/// Experimental: the API may change without notice. Serverless types share names with <see cref="Grpc"/>
/// types, e.g. <c>Distance</c>, so import them with an alias: <c>using Serverless = Qdrant.Client.Grpc.Serverless</c>
/// </remarks>
public class QdrantServerlessClient : IDisposable
{
	private readonly QdrantGrpcClient _grpcClient;
	private readonly bool _ownsGrpcClient;
	private bool _isDisposed;

	private readonly CollectionsService.CollectionsServiceClient _collectionsClient;
	private readonly Points.PointsClient _pointsClient;

	private readonly TimeSpan _grpcTimeout;
	private readonly ILogger _logger;

	/// <summary>Instantiates a new Qdrant Serverless client over HTTPS on port 443.</summary>
	/// <param name="host">The host to connect to.</param>
	/// <param name="apiKey">The API key to use.</param>
	/// <param name="grpcTimeout">The timeout for gRPC calls. Sets the gRPC deadline for all calls.</param>
	/// <param name="loggerFactory">A logger factory through which to log messages.</param>
	/// <param name="headers">Optional headers to send with every gRPC request.</param>
	public QdrantServerlessClient(
		string host,
		string? apiKey = null,
		TimeSpan grpcTimeout = default,
		ILoggerFactory? loggerFactory = null,
		IDictionary<string, string>? headers = null)
		: this(
			new QdrantGrpcClient(new UriBuilder("https", host, 443).Uri, apiKey, headers),
			ownsGrpcClient: true,
			grpcTimeout,
			loggerFactory)
	{
	}

	/// <summary>Instantiates a new Qdrant Serverless client.</summary>
	/// <param name="grpcClient">The low-level gRPC client to use.</param>
	/// <param name="grpcTimeout">The timeout for gRPC calls. Sets the gRPC deadline for all calls.</param>
	/// <param name="loggerFactory">A logger factory through which to log messages.</param>
	public QdrantServerlessClient(
		QdrantGrpcClient grpcClient,
		TimeSpan grpcTimeout = default,
		ILoggerFactory? loggerFactory = null)
		: this(grpcClient, ownsGrpcClient: false, grpcTimeout, loggerFactory)
	{
	}

	private QdrantServerlessClient(
		QdrantGrpcClient grpcClient,
		bool ownsGrpcClient,
		TimeSpan grpcTimeout,
		ILoggerFactory? loggerFactory)
	{
		_grpcClient = grpcClient;
		_ownsGrpcClient = ownsGrpcClient;
		_collectionsClient = grpcClient.ServerlessCollections;
		_pointsClient = grpcClient.Points;
		_grpcTimeout = grpcTimeout;
		_logger = loggerFactory?.CreateLogger("Qdrant.Client") ?? NullLogger.Instance;
	}

	#region Collections

	/// <summary>
	/// Creates a collection. At least one dense or sparse vector is required.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="config">The configuration of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	/// <exception cref="RpcException">
	/// With <see cref="StatusCode.AlreadyExists"/> if the collection exists, unless the service reports it in
	/// <see cref="CreateCollectionResponse.Result"/>.
	/// </exception>
	public Task<CreateCollectionResponse> CreateCollectionAsync(
		string collectionName,
		CollectionConfig config,
		CancellationToken cancellationToken = default)
		=> Call(
			nameof(LoggingExtensions.CreateCollection),
			_ => _logger.CreateCollection(collectionName),
			(options, request) => _collectionsClient.CreateCollectionAsync(request, options),
			new CreateCollectionRequest { CollectionName = collectionName, Config = config },
			cancellationToken);

	/// <summary>
	/// Deletes a collection and all of its data.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<DeleteCollectionResponse> DeleteCollectionAsync(
		string collectionName,
		CancellationToken cancellationToken = default)
		=> Call(
			nameof(LoggingExtensions.DeleteCollection),
			_ => _logger.DeleteCollection(collectionName),
			(options, request) => _collectionsClient.DeleteCollectionAsync(request, options),
			new DeleteCollectionRequest { CollectionName = collectionName },
			cancellationToken);

	/// <summary>
	/// Gets a collection's configuration and point count. A missing collection is reported by
	/// <see cref="GetCollectionResponse.Exists"/>, not as an error.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<GetCollectionResponse> GetCollectionAsync(
		string collectionName,
		CancellationToken cancellationToken = default)
		=> Call(
			nameof(LoggingExtensions.GetCollectionInfo),
			_ => _logger.GetCollectionInfo(collectionName),
			(options, request) => _collectionsClient.GetCollectionAsync(request, options),
			new GetCollectionRequest { CollectionName = collectionName },
			cancellationToken);

	/// <summary>
	/// Checks whether a collection exists.
	/// </summary>
	/// <param name="collectionName">The name of the collection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<bool> CollectionExistsAsync(
		string collectionName,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.CollectionExists),
			_ => _logger.CollectionExists(collectionName),
			(options, request) => _collectionsClient.GetCollectionAsync(request, options),
			new GetCollectionRequest { CollectionName = collectionName },
			cancellationToken).ConfigureAwait(false)).Exists;

	/// <summary>
	/// Lists a page of collections. Pass <see cref="ListCollectionsResponse.NextOffsetToken"/> as
	/// <see cref="ListCollectionsRequest.OffsetToken"/> to get the next page.
	/// </summary>
	/// <param name="request">The page to list. If <c>null</c>, lists the first page of the default size (20).</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<ListCollectionsResponse> ListCollectionsAsync(
		ListCollectionsRequest? request = null,
		CancellationToken cancellationToken = default)
		=> Call(
			nameof(LoggingExtensions.ListCollections),
			_ => _logger.ListCollections(),
			(options, r) => _collectionsClient.ListCollectionsAsync(r, options),
			request ?? new ListCollectionsRequest(),
			cancellationToken);

	#endregion Collections

	#region Points

	/// <summary>
	/// Universally queries points: search, recommend, discover, hybrid and multi-stage queries.
	/// </summary>
	/// <param name="request">The query request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<ScoredPoint>> QueryAsync(
		QueryPoints request,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.Query),
			r => _logger.Query(r.CollectionName),
			(options, r) => _pointsClient.QueryAsync(r, options),
			request,
			cancellationToken).ConfigureAwait(false)).Result;

	/// <summary>
	/// Runs several queries in one request.
	/// </summary>
	/// <param name="request">The batch query request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<BatchResult>> QueryBatchAsync(
		QueryBatchPoints request,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.QueryBatch),
			r => _logger.QueryBatch(r.CollectionName),
			(options, r) => _pointsClient.QueryBatchAsync(r, options),
			request,
			cancellationToken).ConfigureAwait(false)).Result;

	/// <summary>
	/// Queries points grouped by a payload field.
	/// </summary>
	/// <param name="request">The grouped query request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<PointGroup>> QueryGroupsAsync(
		QueryPointGroups request,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.QueryGroups),
			r => _logger.QueryGroups(r.CollectionName),
			(options, r) => _pointsClient.QueryGroupsAsync(r, options),
			request,
			cancellationToken).ConfigureAwait(false)).Result.Groups;

	/// <summary>
	/// Retrieves points by IDs.
	/// </summary>
	/// <param name="request">The retrieve request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<RetrievedPoint>> RetrieveAsync(
		GetPoints request,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.Retrieve),
			r => _logger.Retrieve(r.CollectionName),
			(options, r) => _pointsClient.GetAsync(r, options),
			request,
			cancellationToken).ConfigureAwait(false)).Result;

	/// <summary>
	/// Returns a page of points and the offset of the next page.
	/// </summary>
	/// <param name="request">The scroll request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<ScrollResponse> ScrollAsync(
		ScrollPoints request,
		CancellationToken cancellationToken = default)
		=> Call(
			nameof(LoggingExtensions.Scroll),
			r => _logger.Scroll(r.CollectionName),
			(options, r) => _pointsClient.ScrollAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Counts points.
	/// </summary>
	/// <param name="request">The count request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<ulong> CountAsync(
		CountPoints request,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.Count),
			r => _logger.Count(r.CollectionName),
			(options, r) => _pointsClient.CountAsync(r, options),
			request,
			cancellationToken).ConfigureAwait(false)).Result.Count;

	/// <summary>
	/// Inserts or overwrites points.
	/// </summary>
	/// <param name="request">The upsert request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> UpsertAsync(
		UpsertPoints request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.Upsert),
			r => _logger.Upsert(r.CollectionName, r.Points.Count),
			(options, r) => _pointsClient.UpsertAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Deletes points.
	/// </summary>
	/// <param name="request">The delete request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteAsync(
		DeletePoints request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.Delete),
			r => _logger.Delete(r.CollectionName),
			(options, r) => _pointsClient.DeleteAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Updates vectors of points.
	/// </summary>
	/// <param name="request">The update vectors request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> UpdateVectorsAsync(
		UpdatePointVectors request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.UpdateVectors),
			r => _logger.UpdateVectors(r.CollectionName),
			(options, r) => _pointsClient.UpdateVectorsAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Removes vectors from points.
	/// </summary>
	/// <param name="request">The delete vectors request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeleteVectorsAsync(
		DeletePointVectors request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.DeleteVectors),
			r => _logger.DeleteVectors(r.CollectionName),
			(options, r) => _pointsClient.DeleteVectorsAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Sets payload fields of points.
	/// </summary>
	/// <param name="request">The set payload request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> SetPayloadAsync(
		SetPayloadPoints request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.SetPayload),
			r => _logger.SetPayload(r.CollectionName),
			(options, r) => _pointsClient.SetPayloadAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Replaces the entire payload of points.
	/// </summary>
	/// <param name="request">The overwrite payload request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> OverwritePayloadAsync(
		SetPayloadPoints request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.OverwritePayload),
			r => _logger.OverwritePayload(r.CollectionName),
			(options, r) => _pointsClient.OverwritePayloadAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Removes payload fields from points.
	/// </summary>
	/// <param name="request">The delete payload request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> DeletePayloadAsync(
		DeletePayloadPoints request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.DeletePayload),
			r => _logger.DeletePayload(r.CollectionName),
			(options, r) => _pointsClient.DeletePayloadAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Removes the entire payload of points.
	/// </summary>
	/// <param name="request">The clear payload request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public Task<UpdateResult> ClearPayloadAsync(
		ClearPayloadPoints request,
		CancellationToken cancellationToken = default)
		=> Update(
			nameof(LoggingExtensions.ClearPayload),
			r => _logger.ClearPayload(r.CollectionName),
			(options, r) => _pointsClient.ClearPayloadAsync(r, options),
			request,
			cancellationToken);

	/// <summary>
	/// Runs several update operations in one request.
	/// </summary>
	/// <param name="request">The batch update request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
	/// </param>
	public async Task<IReadOnlyList<UpdateResult>> UpdateBatchAsync(
		UpdateBatchPoints request,
		CancellationToken cancellationToken = default)
		=> (await Call(
			nameof(LoggingExtensions.UpdateBatch),
			r => _logger.UpdateBatch(r.CollectionName),
			(options, r) => _pointsClient.UpdateBatchAsync(r, options),
			request,
			cancellationToken).ConfigureAwait(false)).Result;

	#endregion Points

	private async Task<UpdateResult> Update<TRequest>(
		string operation,
		Action<TRequest> log,
		Func<CallOptions, TRequest, AsyncUnaryCall<PointsOperationResponse>> call,
		TRequest request,
		CancellationToken cancellationToken)
		=> (await Call(operation, log, call, request, cancellationToken).ConfigureAwait(false)).Result;

	private async Task<TResponse> Call<TRequest, TResponse>(
		string operation,
		Action<TRequest> log,
		Func<CallOptions, TRequest, AsyncUnaryCall<TResponse>> call,
		TRequest request,
		CancellationToken cancellationToken)
	{
		log(request);

		var options = new CallOptions(
			deadline: _grpcTimeout == default ? null : DateTime.UtcNow.Add(_grpcTimeout),
			cancellationToken: cancellationToken);
		try
		{
			return await call(options, request).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			_logger.OperationFailed(operation, e);

			throw;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases the resources used by the current instance of <see cref="QdrantServerlessClient"/>.
	/// </summary>
	/// <param name="disposing">Indicates whether the method is called from <see cref="Dispose()"/>
	/// or a finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			_isDisposed = true;

			if (disposing && _ownsGrpcClient)
				_grpcClient.Dispose();
		}
	}
}
