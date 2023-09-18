using FluentAssertions;
using Qdrant.Client;
using Xunit;

namespace Qdrant.Client.Tests;

[Collection("Qdrant")]
public class HealthTests
{
	private readonly QdrantGrpcClient _client;

	public HealthTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateGrpcClient();

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
