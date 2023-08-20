#if NETFRAMEWORK
using Grpc.Net.Client.Web;
#endif

using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Qdrant.Grpc;

namespace Examples;

public class Client
{
	public void Create()
	{
		#region CreateClient
		var channel = QdrantChannel.ForAddress("http://localhost:6334");
		var client = new QdrantGrpcClient(channel);
		#endregion
	}

#if NETFRAMEWORK
	public void CreateClientNetFramework()
	{
		#region CreateClientNetFramework
		var channel = GrpcChannel.ForAddress("https://localhost:6334", new GrpcChannelOptions
		{
			HttpHandler = new GrpcWebHandler(new WinHttpHandler
			{
				ServerCertificateValidationCallback =
					CertificateValidation.Thumbprint("<certificate thumbprint>")
			})
		});
		var client = new QdrantGrpcClient(channel);
		#endregion
	}
#endif

	public void CreateWithApiKey()
	{
		#region CreateClientWithApiKey
		var channel = QdrantChannel.ForAddress("http://localhost:6334", new ClientConfiguration
		{
			ApiKey = "<api key>"
		});
		var client = new QdrantGrpcClient(channel);
		#endregion
	}

	public void CreateWithApiKeyAndSelfSignedCert()
	{
		#region CreateClientWithApiKeyAndSelfSignedCert
		var channel = QdrantChannel.ForAddress("https://localhost:6334", new ClientConfiguration
		{
			ApiKey = "<api key>",
			CertificateThumbprint = "<certificate thumbprint>"
		});
		var client = new QdrantGrpcClient(channel);
		#endregion
	}

#if NETFRAMEWORK
	public void CreateWithGrpcChannelNetFramework()
	{
		#region CreateWithGrpcChannelNetFramework
		var channel = GrpcChannel.ForAddress("https://localhost:6334", new GrpcChannelOptions
		{
			MaxRetryAttempts = 2,
			MaxReceiveMessageSize = 8_388_608, // 8MB
			HttpHandler = new GrpcWebHandler(new WinHttpHandler
			{
				ServerCertificateValidationCallback =
					CertificateValidation.Thumbprint("<certificate thumbprint>")
			})
		});
		var callInvoker = channel.Intercept(metadata =>
		{
			metadata.Add("api-key", "<api key>");
			return metadata;
		});
		var client = new QdrantGrpcClient(callInvoker);
		#endregion
	}
#endif

	public void CreateWithGrpcChannel()
	{
		#region CreateClientWithGrpcChannel
		var channel = GrpcChannel.ForAddress("https://localhost:6334", new GrpcChannelOptions
		{
			MaxRetryAttempts = 2,
			MaxReceiveMessageSize = 8_388_608, // 8MB
			HttpHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback =
					CertificateValidation.Thumbprint("<certificate thumbprint>")
			}
		});
		var callInvoker = channel.Intercept(metadata =>
		{
			metadata.Add("api-key", "<api key>");
			return metadata;
		});
		var client = new QdrantGrpcClient(callInvoker);
		#endregion
	}
}
