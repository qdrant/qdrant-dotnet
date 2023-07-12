using System.Security.Authentication;
using FluentAssertions;
using Grpc.Core;
using Xunit;

namespace Qdrant.Grpc.Tests;

[Collection("QdrantSecured")]
public class ApiKeyCertificateThumbprintTests
{
	private const string CertificateThumbprint = "28fb5a5ef762238ec259ae46720180b855be3274ff39a9edc1b465a5b46a4546";
	private const string ApiKey = "password!";
	private readonly string _address;

	public ApiKeyCertificateThumbprintTests(QdrantSecuredFixture qdrantFixture) =>
		// must explicitly use localhost, as dotnet linux will send 127.0.0.1 which
		// errors in rustls
		// [ WARN  rustls::msgs::handshake] Illegal SNI hostname received [49, 50, 55, 46, 48, 46, 48, 46, 49]
		// [ WARN  rustls::common_state] Sending fatal alert DecodeError
		_address = $"https://localhost:{qdrantFixture.GrpcPort}";

	[Fact]
	public void ClientConfiguredWithApiKeyAndCertValidationCanConnect()
	{
		var address = QdrantChannel.ForAddress(_address,
			new ClientConfiguration
			{
				ApiKey = ApiKey,
				CertificateThumbprint = CertificateThumbprint
			});

		var client = new QdrantGrpcClient(address);

		var response = client.Qdrant.HealthCheck(new HealthCheckRequest());
		response.Title.Should().NotBeNullOrEmpty();
		response.Version.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void ClientConfiguredWithoutApiKeyCannotConnect()
	{
		var address = QdrantChannel.ForAddress(_address,
			new ClientConfiguration
			{
				CertificateThumbprint = CertificateThumbprint
			});

		var client = new QdrantGrpcClient(address);

		var exception = Assert.Throws<RpcException>(() => client.Qdrant.HealthCheck(new HealthCheckRequest()));
		exception.Status.StatusCode.Should().Be(StatusCode.PermissionDenied);
		exception.Status.Detail.Should().Be("Invalid api-key");
	}

	[Fact]
	public void ClientConfiguredWithoutCertificateThumbprintCannotConnect()
	{
		var address = QdrantChannel.ForAddress(_address,
			new ClientConfiguration
			{
				ApiKey = ApiKey
			});

		var client = new QdrantGrpcClient(address);

		var exception = Assert.Throws<RpcException>(() => client.Qdrant.HealthCheck(new HealthCheckRequest()));
		exception.Status.StatusCode.Should().Be(StatusCode.Internal);
		exception.InnerException.Should().BeOfType<HttpRequestException>();
		exception.InnerException?.InnerException.Should().BeOfType<AuthenticationException>();
	}
}
