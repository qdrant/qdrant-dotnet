using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Qdrant.Grpc;

/// <summary>
/// Server Certificate validation methods
/// </summary>
public static class CertificateValidation
{
	private const string Colon = ":";
	private const string Hyphen = "-";

	/// <summary>
	/// A ServerCertificateCustomValidationCallback that validates a certificate used for TLS using its thumbprint.
	/// </summary>
	/// <param name="thumbprint">The SHA256 thumbprint of the certificate used for TLS</param>
	/// <returns>a function that can be called to validate a server certificate</returns>
	public static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> Thumbprint(string thumbprint)
	{
		var normalizedThumbprint = NormalizeThumbprint(thumbprint);
		return (message, certificate, chain, policyErrors) =>
		{
			if (chain is not null)
			{
				foreach (var element in chain.ChainElements)
				{
					if (ValidateThumbprint(element.Certificate, normalizedThumbprint))
						return true;
				}
			}

			return certificate is not null && ValidateThumbprint(certificate, normalizedThumbprint);
		};
	}

	private static string NormalizeThumbprint(string thumbprint)
	{
		var normalized = thumbprint;

		if (thumbprint.Contains(Colon))
			normalized = thumbprint.Replace(Colon, string.Empty);
		else if (thumbprint.Contains(Hyphen))
			normalized = thumbprint.Replace(Hyphen, string.Empty);

		return normalized;
	}

	private static bool ValidateThumbprint(X509Certificate certificate, string thumbprint)
	{
#if DOTNETCORE && !NETSTANDARD
		var certificateThumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
#else
		using var sha256 = SHA256.Create();
		var bytes = sha256.ComputeHash(certificate.GetRawCertData());
		var certificateThumbprint = BitConverter.ToString(bytes);
#endif
		certificateThumbprint = NormalizeThumbprint(certificateThumbprint);
		return thumbprint.Equals(certificateThumbprint, StringComparison.OrdinalIgnoreCase);
	}
}
