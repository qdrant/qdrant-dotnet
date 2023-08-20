# Connecting

This page contains the information you need to create an instance of the Qdrant gRPC .NET client, to
connect to your Qdrant cluster.

The client is thread safe, so create a single instance and reuse it for the lifetime of the application.

## Simple configuration

To connect to a locally running cluster

# [.NET](#tab/net)

[!code-csharp[](../../examples/Examples/Client.cs#CreateClient)]

# [.NET Framework](#tab/netframework)

> [!IMPORTANT]
> 
> .NET Framework has limited supported for gRPC over HTTP/2, but it can be enabled by
> 
> - Configuring qdrant to use TLS, and you **must** use HTTPS, so you will need to set up 
>   [server certificate validation](#validating-self-signed-tls-certificates)
> - Referencing [Grpc.Net.Client.Web](https://www.nuget.org/packages/Grpc.Net.Client.Web) and configuring `GrpcWebHandler` as the `HttpHandler`
> - Referencing [System.Net.Http.WinHttpHandler](https://www.nuget.org/packages/System.Net.Http.WinHttpHandler/) 6.0.1 or later, and configuring `WinHttpHandler` as the inner handler for  `GrpcWebHandler`
> 
> See [Configure gRPC-Web with the .NET gRPC client](https://learn.microsoft.com/en-au/aspnet/core/grpc/grpcweb?view=aspnetcore-7.0#configure-grpc-web-with-the-net-grpc-client) and use [gRPC client with .NET Standard 2.0](https://learn.microsoft.com/en-au/aspnet/core/grpc/netstandard?view=aspnetcore-7.0#net-framework) for further details.

[!code-csharp[](../../examples/Examples/Client.cs#CreateClientNetFramework)]

---

## Setting an API key

When your Qdrant cluster is secured by API key based authentication, a client can be configured
with an API to use as follows

[!code-csharp[](../../examples/Examples/Client.cs#CreateClientWithApiKey)]

where `<api key>` is the [API key defined in the Qdrant cluster configuration](https://qdrant.tech/documentation/guides/security/#authentication).

## Validating self-signed TLS certificates

When using API key based authentication, it is **strongly recommended** to also secure the cluster
with Transport Layer Security (TLS) for encrypted communications.

If the Qdrant cluster is secured with a certificate that is signed by a trusted Certificate Authority (CA),
no further action needs to be taken; the certificate will be validated by default. A trusted CA is one that
is trusted by the operating system on which the client is running, which typically means that the CA
certificate is in the certificate/truststore of the operating system.

Often, self-signed certificates are used to secure self-hosted clusters, where certificates are generated
by your own CA. The client can be configured to
validate the SHA-256 certificate thumbprint/fingerprint of the certificate

[!code-csharp[](../../examples/Examples/Client.cs#CreateClientWithApiKeyAndSelfSignedCert)]

where `<certificate thumbprint>` is the SHA-256 certificate thumbprint of the certificate or CA certificate.
The thumbprint of a certificate can be retrieved at any time with

# [Linux / macOS](#tab/linux)

Using [openssl](https://www.openssl.org/)

```sh
openssl x509 -in cert.pem -fingerprint -sha256 -noout
```

where `cert.pem` is the path to the certificate.

This outputs the thumbprint/fingerprint similar to

```
SHA256 Fingerprint=28:FB:5A:5E:F7:62:23:8E:C2:59:AE:46:72:01:80:B8:55:BE:32:74:FF:39:A9:ED:C1:B4:65:A5:B4:6A:45:46
```

# [Windows](#tab/windows)

If you have WSL2 installed, you can use openssl as described for Linux / macOS.

Thumbprints can be retrieved with Powershell

```powershell
 $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 "$(pwd)\cert.pem"
 $sha256 = [System.Security.Cryptography.SHA256]::Create()
 $bytes = $sha256.ComputeHash($cert.GetRawCertData())
 [BitConverter]::ToString($bytes)
```

where `"$(pwd)\cert.pem"` is the absolute path to the certificate.

This outputs the thumbprint/fingerprint similar to

```
28-FB-5A-5E-F7-62-23-8E-C2-59-AE-46-72-01-80-B8-55-BE-32-74-FF-39-A9-ED-C1-B4-65-A5-B4-6A-45-46
```

---

The client handles differences in output format, so you can copy and paste the thumbprint verbatim.

## Advanced Client configuration

`QdrantChannel` is a convenient way to configure a gRPC channel for the client. If you're in need of
more control over client configuration, you are free to use `GrpcChannel` directly. For example

# [.NET](#tab/net)

[!code-csharp[](../../examples/Examples/Client.cs#CreateClientWithGrpcChannel)]

# [.NET Framework](#tab/netframework)

[!code-csharp[](../../examples/Examples/Client.cs#CreateWithGrpcChannelNetFramework)]

---

This configures the client for API key based authentication and TLS certificate thumbprint validation,
in addition to applying some other settings.