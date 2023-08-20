#if NETFRAMEWORK
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
#endif

using System.Security.Authentication;
using FluentAssertions;
using Grpc.Core;
using Xunit;

namespace Qdrant.Grpc.Tests;

[Collection("QdrantSecured")]
public class ApiKeyCertificateThumbprintTests
{
	private const string CertificateThumbprint = "cf81bede843ebe18d43b98987e19dcba4b34244897df74d03976dae79d3b3a26";
	private const string ApiKey = "password!";
	private readonly string _address;

	public ApiKeyCertificateThumbprintTests(QdrantSecuredFixture qdrantFixture)
	{
		// must explicitly use localhost, as dotnet linux will send 127.0.0.1 which
		// errors in rustls
		// [ WARN  rustls::msgs::handshake] Illegal SNI hostname received [49, 50, 55, 46, 48, 46, 48, 46, 49]
		// [ WARN  rustls::common_state] Sending fatal alert DecodeError
		var host = qdrantFixture.Host == "127.0.0.1" ? "localhost" : qdrantFixture.Host;
		_address = $"https://{host}:{qdrantFixture.GrpcPort}";
	}

	[Fact]
	public void ClientConfiguredWithApiKeyAndCertValidationCanConnect()
	{
#if NETFRAMEWORK
		var channel = GrpcChannel.ForAddress(_address, new GrpcChannelOptions
		{
			HttpHandler = new GrpcWebHandler(new WinHttpHandler
			{
				ServerCertificateValidationCallback = CertificateValidation.Thumbprint(CertificateThumbprint)
			})
		});
		var callInvoker = channel.Intercept(metadata =>
		{
			metadata.Add("api-key", "password!");
			return metadata;
		});

#else
		var callInvoker = QdrantChannel.ForAddress(_address,
			new ClientConfiguration
			{
				ApiKey = ApiKey,
				CertificateThumbprint = CertificateThumbprint
			}).CreateCallInvoker();
#endif

		var client = new QdrantGrpcClient(callInvoker);

		var response = client.Qdrant.HealthCheck(new HealthCheckRequest());
		response.Title.Should().NotBeNullOrEmpty();
		response.Version.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void ClientConfiguredWithoutApiKeyCannotConnect()
	{
#if NETFRAMEWORK
		var callInvoker = GrpcChannel.ForAddress(_address, new GrpcChannelOptions
		{
			HttpHandler = new GrpcWebHandler(new WinHttpHandler
			{
				ServerCertificateValidationCallback = CertificateValidation.Thumbprint(CertificateThumbprint)
			})
		}).CreateCallInvoker();
#else
		var callInvoker = QdrantChannel.ForAddress(_address,
			new ClientConfiguration
			{
				CertificateThumbprint = CertificateThumbprint
			}).CreateCallInvoker();
#endif

		var client = new QdrantGrpcClient(callInvoker);

		var exception = Assert.Throws<RpcException>(() => client.Qdrant.HealthCheck(new HealthCheckRequest()));
		exception.Status.StatusCode.Should().Be(StatusCode.PermissionDenied);
		exception.Status.Detail.Should().Be("Invalid api-key");
	}

	[Fact]
	public void ClientConfiguredWithoutCertificateThumbprintCannotConnect()
	{
#if NETFRAMEWORK
		var channel = GrpcChannel.ForAddress(_address, new GrpcChannelOptions
		{
			HttpHandler = new GrpcWebHandler(new WinHttpHandler())
		});
		var callInvoker = channel.Intercept(metadata =>
		{
			metadata.Add("api-key", "password!");
			return metadata;
		});
#else
		var callInvoker = QdrantChannel.ForAddress(_address,
			new ClientConfiguration
			{
				ApiKey = ApiKey
			}).CreateCallInvoker();
#endif

		var client = new QdrantGrpcClient(callInvoker);

		var exception = Assert.Throws<RpcException>(() => client.Qdrant.HealthCheck(new HealthCheckRequest()));
		exception.Status.StatusCode.Should().Be(StatusCode.Internal);
		exception.InnerException.Should().BeOfType<HttpRequestException>();

#if !NETFRAMEWORK
		exception.InnerException?.InnerException.Should().BeOfType<AuthenticationException>();
#endif
	}
}
