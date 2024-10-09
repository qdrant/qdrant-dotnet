using FluentAssertions;
using Grpc.Core;
using Xunit;

namespace Qdrant.Client
{
	public class HealthCheckTests : IClassFixture<QdrantFixture>
	{
		private readonly QdrantClient _client;

		public HealthCheckTests(QdrantFixture qdrantFixture) => _client = qdrantFixture.CreateClient();

		[Fact]
		public async Task Server_Should_Be_Healthy()
		{
			var response = await _client.HealthAsync();

			response.Title.Should().NotBeNullOrEmpty();
			response.Version.Should().NotBeNullOrEmpty();
		}

		[Fact]
		public async Task Server_Should_Be_UnHealthy()
		{
			var client = new QdrantClient("localhost", 9999);
			await Assert.ThrowsAsync<RpcException>(async () => await client.HealthAsync());
		}
	}
}
