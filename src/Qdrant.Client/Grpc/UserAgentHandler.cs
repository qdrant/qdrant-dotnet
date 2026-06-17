using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Qdrant.Client.Grpc;

/// <summary>
/// Appends a <c>User-Agent</c> token to the request so
/// server-side tooling can attribute traffic to this client.
/// </summary>
internal sealed class UserAgentHandler : DelegatingHandler
{
	internal const string ProductName = "qdrant-dotnet";
	internal static readonly string ProductVersion = ResolveVersion();

	private static readonly ProductInfoHeaderValue Token = new(ProductName, ProductVersion);

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// Append, not replace, to keep grpc-dotnet's token.
		// The guard avoids duplicates when a request is re-sent on retry.
		if (!request.Headers.UserAgent.Contains(Token))
			request.Headers.UserAgent.Add(Token);

		return base.SendAsync(request, cancellationToken);
	}

	private static string ResolveVersion()
	{
		var version = typeof(UserAgentHandler).Assembly
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

		// Strip build metadata: "1.2.3+abc123" -> "1.2.3".
		var plus = version.IndexOf('+');
		return plus < 0 ? version : version.Substring(0, plus);
	}
}
