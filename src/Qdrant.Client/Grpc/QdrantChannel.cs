using System.Net.Http;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Qdrant.Client.Grpc;

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

		return _channel.Intercept(metadata =>
		{
			if (_configuration.ApiKey is not null)
				metadata.Add("api-key", _configuration.ApiKey);

			foreach (var header in _configuration.Headers)
				metadata.Add(header.Key, header.Value);

			var requestHeaders = RequestHeaders.Current;
			if (requestHeaders is not null)
				foreach (var header in requestHeaders)
					metadata.Add(header.Key, header.Value);

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

#if NETFRAMEWORK
		// .NET Framework has finicky HTTP/2 support, so preserve the original
		// behavior: only set an HttpClientHandler when certificate validation is
		// required and otherwise let Grpc.Net.Client pick its default handler.
		if (configuration.CertificateThumbprint is not null)
		{
			channelOptions.HttpHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback =
					CertificateValidation.Thumbprint(configuration.CertificateThumbprint)
			};
		}
#else
		var primaryHandler = new HttpClientHandler();
		if (configuration.CertificateThumbprint is not null)
		{
			primaryHandler.ServerCertificateCustomValidationCallback =
				CertificateValidation.Thumbprint(configuration.CertificateThumbprint);
		}

		// Advertise a Qdrant-branded "qdrant-dotnet/<version>" token in the
		// User-Agent. Grpc.Net.Client does not let us set the User-Agent through
		// gRPC metadata, so we add it at the HTTP layer via a delegating handler
		// that wraps the primary handler.
		channelOptions.HttpHandler = new UserAgentHandler { InnerHandler = primaryHandler };
#endif

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
