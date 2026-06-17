using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

public class UserAgentHandlerTests
{
	[Fact]
	public async Task AddsQdrantUserAgentToken()
	{
		var (handler, capture) = CreateInvoker();

		using var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost");
		await handler.SendAsync(request, CancellationToken.None);

		capture.Request.Should().NotBeNull();
		capture.Request!.Headers.UserAgent
			.Should().ContainSingle(p =>
				p.Product != null
				&& p.Product.Name == UserAgentHandler.ProductName
				&& p.Product.Version == UserAgentHandler.ProductVersion);
	}

	[Fact]
	public async Task PreservesExistingUserAgentTokens()
	{
		var (handler, capture) = CreateInvoker();

		using var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost");
		// Mimic the token that Grpc.Net.Client injects.
		request.Headers.UserAgent.Add(new ProductInfoHeaderValue("grpc-dotnet", "2.71.0"));

		await handler.SendAsync(request, CancellationToken.None);

		var userAgents = capture.Request!.Headers.UserAgent;
		userAgents.Should().Contain(p => p.Product != null && p.Product.Name == "grpc-dotnet");
		userAgents.Should().Contain(p => p.Product != null && p.Product.Name == UserAgentHandler.ProductName);
	}

	[Fact]
	public async Task DoesNotDuplicateTokenOnRetry()
	{
		var (handler, capture) = CreateInvoker();

		using var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost");
		await handler.SendAsync(request, CancellationToken.None);
		// Re-send the same request instance (as can happen on a retry).
		await handler.SendAsync(request, CancellationToken.None);

		capture.Request!.Headers.UserAgent
			.Count(p => p.Product != null && p.Product.Name == UserAgentHandler.ProductName)
			.Should().Be(1);
	}

	private static (HttpMessageInvoker invoker, RequestCapturingHandler capture) CreateInvoker()
	{
		var capture = new RequestCapturingHandler();
		var handler = new UserAgentHandler { InnerHandler = capture };
		return (new HttpMessageInvoker(handler), capture);
	}

	private sealed class RequestCapturingHandler : HttpMessageHandler
	{
		public HttpRequestMessage? Request { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			Request = request;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
		}
	}
}
