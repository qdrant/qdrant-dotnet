using Qdrant.Grpc.Tests.Container;
using Xunit;

namespace Qdrant.Grpc.Tests;

[CollectionDefinition("Qdrant")]
public sealed class QdrantCollection : ICollectionFixture<QdrantFixture> { }

public sealed class QdrantFixture : IAsyncLifetime
{
    private readonly QdrantContainer _container = new QdrantBuilder().Build();

    public string Host => _container.Hostname;

    public ushort HttpPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantHttpPort);

    public ushort GrpcPort => _container.GetMappedPublicPort(QdrantBuilder.QdrantGrpcPort);

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
