namespace Qdrant.Client;

/// <summary>
/// Configuration for the gRPC client
/// </summary>
public class ClientConfiguration
{
	/// <summary>
	/// The API key to use.
	/// </summary>
	public string? ApiKey { get; set; }

	/// <summary>
	/// The certificate thumbprint to use when using a self-signed certificate for TLS.
	/// </summary>
	public string? CertificateThumbprint { get; set; }

	/// <summary>
	/// Optional headers to send with every gRPC request.
	/// </summary>
	public Dictionary<string, string> Headers { get; set; } = [];
}
