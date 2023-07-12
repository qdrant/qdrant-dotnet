namespace Qdrant.Grpc;

/// <summary>
/// Configuration for the gRPC client
/// </summary>
public class ClientConfiguration
{
	/// <summary>
	/// The API key to sue
	/// </summary>
	public string? ApiKey { get; set; }

	/// <summary>
	/// The certificate thumbprint to use when using a
	/// self-signed certificate for TLS
	/// </summary>
	public string? CertificateThumbprint { get; set; }
}
