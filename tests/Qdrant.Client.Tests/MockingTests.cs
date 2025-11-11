using FluentAssertions;
using Grpc.Core;
using Moq;
using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

public class MockingTests
{
	[Fact]
	public void CanMockGrpcClientCalls()
	{
		var client = new Mock<QdrantGrpcClient>();
		var qdrant = new Mock<Qdrant.Client.Grpc.Qdrant.QdrantClient>();

		var mockResponse = new HealthCheckReply { Title = "from Moq", Version = "v1.0.0" };

		qdrant.Setup(q =>
				q.HealthCheck(
					It.IsAny<HealthCheckRequest>(),
					It.IsAny<CallOptions>()))
			.Returns(mockResponse);

		client.SetupGet(c => c.Qdrant).Returns(qdrant.Object);
		var response = client.Object.Qdrant.HealthCheck(new HealthCheckRequest(), new CallOptions());

		response.Should().NotBeNull();
		response.Title.Should().Be("from Moq");
		response.Version.Should().Be("v1.0.0");
	}

	[Fact]
	public async Task CanMockHighLevelClientCalls()
	{
		var grpcClient = new Mock<QdrantGrpcClient>();
		var client = new QdrantClient(grpcClient.Object);
		var qdrant = new Mock<Qdrant.Client.Grpc.Qdrant.QdrantClient>();

		var mockResponse = new HealthCheckReply { Title = "from Moq", Version = "v1.0.0" };

		qdrant.Setup(q =>
				q.HealthCheckAsync(
					It.IsAny<HealthCheckRequest>(),
					It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
			.Returns(new AsyncUnaryCall<HealthCheckReply>(
				Task.FromResult(mockResponse),
				null!,
				null!,
				null!,
				() => { }
			));

		grpcClient.SetupGet(c => c.Qdrant).Returns(qdrant.Object);
		var response = await client.HealthAsync();

		response.Should().NotBeNull();
		response.Title.Should().Be("from Moq");
		response.Version.Should().Be("v1.0.0");
	}

	[Fact]
	public async Task CanMockUsingInterface()
	{
		var expectedResult = new HealthCheckReply { Title = "from Moq", Version = "v1.0.0" };
		using var cts = new CancellationTokenSource();
		var mockedClient = new Mock<IQdrantClient>();
		var subject = mockedClient.Object;

		mockedClient.Setup(q => q.HealthAsync(cts.Token))
					.ReturnsAsync(new HealthCheckReply { Title = "from Moq", Version = "v1.0.0" });

		var actualResult = await subject.HealthAsync(cts.Token);

		actualResult.Should().BeEquivalentTo(expectedResult);
	}
}
