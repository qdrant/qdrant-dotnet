# .NET SDK for Qdrant vector database

[![NuGet Release][Qdrant-image]][Qdrant-nuget-url]
[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2Fqdrant%2Fqdrant-dotnet%2Fbadge%3Fref%3Dmain&style=flat)](https://actions-badge.atrox.dev/qdrant/qdrant-dotnet/goto?ref=main)

.NET SDK for [Qdrant vector database](https://qdrant.tech/).

## Getting started

### Installing

```sh
dotnet add package Qdrant.Client
```

### Creating a client

A client can be instantiated with

```csharp
var client = new QdrantClient("localhost");
```

which creates a client that will connect to Qdrant on `http://localhost:6334`.

Internally, the high level client uses a low level gRPC client to interact with Qdrant.
Additional constructor overloads provide more control over how the gRPC client is
configured. The following example configures a client to use TLS, validating the 
certificate using its thumbprint, and also configures API key authentication:

```csharp
var channel = QdrantChannel.ForAddress("https://localhost:6334", new ClientConfiguration
{
    ApiKey = "<api key>",
    CertificateThumbprint = "<certificate thumbprint>"
});
var grpcClient = new QdrantGrpcClient(channel);
var client = new QdrantClient(grpcClient);
```

---
**IMPORTANT NOTICE for .NET Framework**

.NET Framework has limited supported for gRPC over HTTP/2, but it can be enabled by

- Configuring qdrant to use TLS, and you **must** use HTTPS, so you will need to set up 
server certificate validation
- Referencing `System.Net.Http.WinHttpHandler` 6.0.1 or later, and configuring 
`WinHttpHandler` as the inner handler for `GrpcChannelOptions`

The following example configures a client for .NET Framework to use TLS, validating 
the certificate using its thumbprint, and also configures API key authentication:

```csharp
var channel = GrpcChannel.ForAddress($"https://localhost:6334", new GrpcChannelOptions
{
  HttpHandler = new WinHttpHandler
  {
    ServerCertificateValidationCallback =
      CertificateValidation.Thumbprint("<certificate thumbprint>")
  }
});
var callInvoker = channel.Intercept(metadata =>
{
  metadata.Add("api-key", "<api key>");
  return metadata;
});

var grpcClient = new QdrantGrpcClient(callInvoker);
var client = new QdrantClient(grpcClient);
```
---

### Working with collections

Once a client has been created, create a new collection

```csharp
await client.CreateCollectionAsync("my_collection", 
    new VectorParams { Size = 100, Distance = Distance.Cosine });
```

Insert vectors into a collection

```csharp
// generate some vectors
var random = new Random();
var points = Enumerable.Range(1, 100).Select(i => new PointStruct
{
  Id = i,
  Vectors = Enumerable.Range(1, 100)
    .Select(_ => (float)random.NextDouble()).ToArray(),
  Payload = 
  { 
    ["color"] = "red", 
    ["rand_number"] = i % 10 
  }
}).ToList();

var updateResult = await client.UpsertAsync("my_collection", points);
```

Search for similar vectors

```csharp
var queryVector = Enumerable.Range(1, 100)
    .Select(_ => (float)random.NextDouble()).ToArray();

// return the 5 closest points
var points = await client.SearchAsync(
  "my_collection",
  queryVector,
  limit: 5);
```

Search for similar vectors with filtering condition

```csharp
// static import Conditions to easily build filtering
using static Qdrant.Client.Grpc.Conditions;

// return the 5 closest points where rand_number >= 3
var points = await _client.SearchAsync(
  "my_collection",
  queryVector,
  filter: Range("rand_number", new Range { Gte = 3 }),
  limit: 5);
```


[Qdrant-nuget-url]:https://www.nuget.org/packages/Qdrant.Client/
[Qdrant-image]:
https://img.shields.io/nuget/v/Qdrant.Client.svg
