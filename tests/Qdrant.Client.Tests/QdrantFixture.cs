#if NETFRAMEWORK
using System.Net.Http;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
#endif

using System.Text;
using Qdrant.Client.Grpc;
using Testcontainers.Qdrant;
using Xunit;

namespace Qdrant.Client;

[CollectionDefinition("Qdrant")]
public sealed class QdrantCollection : ICollectionFixture<QdrantFixture> { }

public sealed class QdrantFixture : IAsyncLifetime
{
	private readonly QdrantContainer _container;

	public QdrantFixture() =>
#if NETFRAMEWORK
		// .NET Framework must use TLS with HTTPS
		_container = new QdrantBuilder()
			.WithImage("qdrant/qdrant:" + QdrantGrpcClient.QdrantVersion)
			.WithBindMount(
				Path.Combine(SolutionPaths.Root, "tests/config.yaml"),
				"/qdrant/config/custom_config.yaml")
			.WithCertificate(
				File.ReadAllText(Path.Combine(SolutionPaths.Root, "tests/cert.pem"), Encoding.UTF8),
				File.ReadAllText(Path.Combine(SolutionPaths.Root, "tests/key.pem"), Encoding.UTF8))
			.WithCommand("./entrypoint.sh", "--config-path", "config/custom_config.yaml")
			.Build();
#else
		_container = new QdrantBuilder()
			.WithImage("qdrant/qdrant:" + QdrantGrpcClient.QdrantVersion)
			.Build();
#endif


	public string Host => _container.Hostname;

	public ushort HttpPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantHttpPort);

	public ushort GrpcPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantGrpcPort);

	public Task InitializeAsync() => _container.StartAsync();

	public Task DisposeAsync() => _container.DisposeAsync().AsTask();

	public QdrantClient CreateClient()
		=> new(CreateGrpcClient());

	public QdrantGrpcClient CreateGrpcClient()
	{
#if NETFRAMEWORK
		// .NET Framework must use TLS with HTTPS with WinHttpHandler
		var channel = GrpcChannel.ForAddress($"https://{Host}:{GrpcPort}", new GrpcChannelOptions
		{
			HttpHandler = new WinHttpHandler
			{
				ServerCertificateValidationCallback =
					CertificateValidation.Thumbprint("cf81bede843ebe18d43b98987e19dcba4b34244897df74d03976dae79d3b3a26")
			}
		});
		var callInvoker = channel.Intercept(metadata =>
		{
			metadata.Add("api-key", "password!");
			return metadata;
		});
		return new QdrantGrpcClient(callInvoker);
#else
		return new QdrantGrpcClient(Host, GrpcPort);
#endif
	}
}
