using FluentAssertions;
using Grpc.Core;
using Moq;
using Qdrant;
using Xunit;

namespace Qdrant.Grpc.Tests;

public class MockingTests
{
	[Fact]
	public void CanMockClientCalls()
	{
		var client = new Mock<QdrantGrpcClient>();
		var qdrant = new Mock<Qdrant.QdrantClient>();

		qdrant.Setup(q =>
				q.HealthCheck(
					It.IsAny<HealthCheckRequest>(),
					It.IsAny<CallOptions>()))
			.Returns(new HealthCheckReply { Title = "from Moq", Version = "v1.0.0" });

		client.SetupGet(c => c.Qdrant).Returns(qdrant.Object);
		var response = client.Object.Qdrant.HealthCheck(new HealthCheckRequest(), new CallOptions());

		response.Should().NotBeNull();
		response.Title.Should().Be("from Moq");
		response.Version.Should().Be("v1.0.0");
	}
}
