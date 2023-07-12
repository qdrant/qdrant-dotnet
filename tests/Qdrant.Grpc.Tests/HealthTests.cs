using FluentAssertions;
using Xunit;

namespace Qdrant.Grpc.Tests;

[Collection("Qdrant")]
public class HealthTests
{
    private readonly QdrantGrpcClient _client;

    public HealthTests(QdrantFixture qdrantFixture)
    {
        var address = QdrantChannel.ForAddress($"http://{qdrantFixture.Host}:{qdrantFixture.GrpcPort}");
        _client = new QdrantGrpcClient(address);
    }

    [Fact]
    public void HealthCheck()
    {
        var response = _client.Qdrant.HealthCheck(new HealthCheckRequest());

        response.Title.Should().NotBeNullOrEmpty();
        response.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HealthCheckAsync()
    {
	    var response = await _client.Qdrant.HealthCheckAsync(new HealthCheckRequest());

	    response.Title.Should().NotBeNullOrEmpty();
	    response.Version.Should().NotBeNullOrEmpty();
    }
}
