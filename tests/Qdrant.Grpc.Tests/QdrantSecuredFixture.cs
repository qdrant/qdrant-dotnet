using Qdrant.Grpc.Tests.Container;
using Xunit;

namespace Qdrant.Grpc.Tests;

[CollectionDefinition("QdrantSecured")]
public sealed class QdrantSecuredCollection : ICollectionFixture<QdrantSecuredFixture> { }

public sealed class QdrantSecuredFixture : IAsyncLifetime
{
	private readonly QdrantContainer _container = new QdrantBuilder()
		.WithConfigFile(Path.Combine(SolutionPaths.Root, "tests/config.yaml"))
		.WithCertificate(
			Path.Combine(SolutionPaths.Root, "tests/cert.pem"),
			Path.Combine(SolutionPaths.Root, "tests/key.pem"))
		.WithCommand("./entrypoint.sh", "--config-path", "config/custom_config.yaml")
		.Build();

	public string Host => _container.Hostname;

	public ushort HttpPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantHttpPort);

	public ushort GrpcPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantGrpcPort);

	public Task InitializeAsync() => _container.StartAsync();

	public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
