using System.Text;
using Qdrant.Client.Grpc;
using Testcontainers.Qdrant;
using Xunit;

namespace Qdrant.Client;

[CollectionDefinition("QdrantSecured")]
public sealed class QdrantSecuredCollection : ICollectionFixture<QdrantSecuredFixture> { }

public sealed class QdrantSecuredFixture : IAsyncLifetime
{
	private readonly QdrantContainer _container = new QdrantBuilder()
		.WithImage("qdrant/qdrant:" + QdrantGrpcClient.QdrantVersion)
		.WithBindMount(
			Path.Combine(SolutionPaths.Root, "tests/config.yaml"),
			"/qdrant/config/custom_config.yaml")
		.WithCertificate(
			File.ReadAllText(Path.Combine(SolutionPaths.Root, "tests/cert.pem"), Encoding.UTF8),
			File.ReadAllText(Path.Combine(SolutionPaths.Root, "tests/key.pem"), Encoding.UTF8))
		.WithCommand("./entrypoint.sh", "--config-path", "config/custom_config.yaml")
		.Build();

	public string Host => _container.Hostname;

	public ushort HttpPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantHttpPort);

	public ushort GrpcPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantGrpcPort);

	public Task InitializeAsync() => _container.StartAsync();

	public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
