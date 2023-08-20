#if NETFRAMEWORK
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
#endif
using Qdrant.Grpc.Tests.Container;
using Xunit;

namespace Qdrant.Grpc.Tests;

[CollectionDefinition("Qdrant")]
public sealed class QdrantCollection : ICollectionFixture<QdrantFixture> { }

public sealed class QdrantFixture : IAsyncLifetime
{
	private readonly QdrantContainer _container;

	public QdrantFixture()
	{
#if NETFRAMEWORK
		// .NET Framework must use TLS with HTTPS
		_container = new QdrantBuilder()
			.WithConfigFile(Path.Combine(SolutionPaths.Root, "tests/config.yaml"))
			.WithCertificate(
				Path.Combine(SolutionPaths.Root, "tests/cert.pem"),
				Path.Combine(SolutionPaths.Root, "tests/key.pem"))
			.WithCommand("./entrypoint.sh", "--config-path", "config/custom_config.yaml")
			.Build();
#else
	    _container = new QdrantBuilder().Build();
#endif
	}

	public string Host => _container.Hostname;

	public ushort HttpPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantHttpPort);

	public ushort GrpcPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantGrpcPort);

	public Task InitializeAsync() => _container.StartAsync();

	public Task DisposeAsync() => _container.DisposeAsync().AsTask();

	public QdrantGrpcClient CreateGrpcClient()
	{
#if NETFRAMEWORK
		// .NET Framework must use TLS with HTTPS, and GrpcWebHandler with WinHttpHandler
		var channel = GrpcChannel.ForAddress($"https://{Host}:{GrpcPort}", new GrpcChannelOptions
		{
			HttpHandler = new GrpcWebHandler(new WinHttpHandler
			{
				ServerCertificateValidationCallback =
					CertificateValidation.Thumbprint("cf81bede843ebe18d43b98987e19dcba4b34244897df74d03976dae79d3b3a26")
			})
		});
		var callInvoker = channel.Intercept(metadata =>
		{
			metadata.Add("api-key", "password!");
			return metadata;
		});
		return new QdrantGrpcClient(callInvoker);
#else
			return new QdrantGrpcClient(QdrantChannel.ForAddress($"http://{Host}:{GrpcPort}"));
#endif
	}
}
