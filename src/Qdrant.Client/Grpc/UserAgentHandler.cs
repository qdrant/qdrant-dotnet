using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Qdrant.Client.Grpc;

/// <summary>
/// A <see cref="DelegatingHandler"/> that advertises the Qdrant .NET client in the
/// request <c>User-Agent</c> header.
/// </summary>
/// <remarks>
/// The underlying gRPC library (<c>Grpc.Net.Client</c>) always injects its own
/// <c>grpc-dotnet/&lt;version&gt;</c> token and does not let callers replace the
/// <c>User-Agent</c> via gRPC metadata. By adding a <c>qdrant-dotnet/&lt;version&gt;</c>
/// token at the HTTP layer, requests carry a Qdrant-branded token (e.g.
/// <c>grpc-dotnet/2.71.0 (...) qdrant-dotnet/1.2.3</c>) so that server-side tooling
/// can attribute traffic to this client and its version.
/// </remarks>
internal sealed class UserAgentHandler : DelegatingHandler
{
	/// <summary>
	/// The product name advertised in the <c>User-Agent</c> header.
	/// </summary>
	internal const string ProductName = "qdrant-dotnet";

	/// <summary>
	/// The product version advertised in the <c>User-Agent</c> header, resolved from
	/// the assembly's informational version (stamped at build time). Falls back to the
	/// assembly version, and finally to "0.0.0" when neither is available.
	/// </summary>
	internal static readonly string ProductVersion = ResolveVersion();

	private static readonly ProductInfoHeaderValue QdrantUserAgent =
		new(ProductName, ProductVersion);

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// Add our token to the User-Agent. We add rather than clear so the gRPC
		// library and runtime information is preserved; the resulting header looks
		// like "grpc-dotnet/<v> (...) qdrant-dotnet/<v>". Token order is not
		// significant for attribution.
		if (!request.Headers.UserAgent.Contains(QdrantUserAgent))
			request.Headers.UserAgent.Add(QdrantUserAgent);

		return base.SendAsync(request, cancellationToken);
	}

	private static string ResolveVersion()
	{
		var assembly = typeof(UserAgentHandler).Assembly;

		var informational = assembly
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
			.InformationalVersion;
		if (!string.IsNullOrEmpty(informational))
		{
			// Strip any build metadata suffix (e.g. "1.2.3+abcdef0") to keep the
			// token compact and avoid leaking commit hashes.
			var plus = informational!.IndexOf('+');
			return plus >= 0 ? informational.Substring(0, plus) : informational;
		}

		return assembly.GetName().Version?.ToString() ?? "0.0.0";
	}
}
