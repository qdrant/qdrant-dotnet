using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Qdrant.Grpc;

/// <summary>
/// A gRPC channel to Qdrant service.
/// Channels are an abstraction of long-lived connections to remote servers.
/// </summary>
public class QdrantChannel : ChannelBase, IDisposable
{
	private readonly GrpcChannel _channel;
	private readonly ClientConfiguration _configuration;

	/// <summary>
	/// Initializes a new instance of <see cref="QdrantChannel"/>
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="configuration"></param>
	private QdrantChannel(GrpcChannel channel, ClientConfiguration configuration) : base(channel.Target)
	{
		_channel = channel;
		_configuration = configuration;
	}

	/// <inheritdoc />
	public override CallInvoker CreateCallInvoker()
	{
		if (Disposed)
			throw new ObjectDisposedException(nameof(QdrantChannel));

		return _configuration.ApiKey is null
			? _channel.CreateCallInvoker()
			: _channel.Intercept(metadata =>
			{
				metadata.Add("api-key", _configuration.ApiKey);
				return metadata;
			});
	}

	/// <summary>
	/// Creates a <see cref="QdrantChannel"/> for the specified address.
	/// </summary>
	/// <param name="address">The address the channel will use.</param>
	/// <returns>A new instance of <see cref="QdrantChannel"/>.</returns>
	public static QdrantChannel ForAddress(string address) =>
		ForAddress(new System.Uri(address), new ClientConfiguration());

	/// <summary>
	/// Creates a <see cref="QdrantChannel"/> for the specified address.
	/// </summary>
	/// <param name="address">The address the channel will use.</param>
	/// <returns>A new instance of <see cref="QdrantChannel"/>.</returns>
	public static QdrantChannel ForAddress(System.Uri address) =>
		ForAddress(address, new ClientConfiguration());

	/// <summary>
	/// Creates a <see cref="QdrantChannel"/> for the specified address.
	/// </summary>
	/// <param name="address">The address the channel will use.</param>
	/// <param name="configuration">The client configuration</param>
	/// <returns>A new instance of <see cref="QdrantChannel"/>.</returns>
	public static QdrantChannel ForAddress(string address, ClientConfiguration configuration) =>
		ForAddress(new System.Uri(address), configuration);

	/// <summary>
	/// Creates a <see cref="QdrantChannel"/> for the specified address.
	/// </summary>
	/// <param name="address">The address the channel will use.</param>
	/// <param name="configuration">The client configuration</param>
	/// <returns>A new instance of <see cref="QdrantChannel"/>.</returns>
	public static QdrantChannel ForAddress(System.Uri address, ClientConfiguration configuration)
	{
		var channelOptions = new GrpcChannelOptions();
		if (configuration.CertificateThumbprint is not null)
		{
			channelOptions.HttpHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback =
					CertificateValidation.Thumbprint(configuration.CertificateThumbprint)
			};
		}
		var channel = GrpcChannel.ForAddress(address, channelOptions);
		return new QdrantChannel(channel, configuration);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (Disposed)
			return;

		Disposed = true;
		_channel.Dispose();
	}

	internal bool Disposed { get; set; }
}
