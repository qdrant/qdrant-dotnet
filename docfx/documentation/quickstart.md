# Getting started

## Installing

The [gRPC client is available on nuget](https://www.nuget.org/packages/Qdrant.Grpc), and can be installed with

```sh
dotnet add package Qdrant.Grpc
```

## Creating the client

Create a new instance of the client with

# [.NET](#tab/net)

[!code-csharp[](../../examples/Examples/Client.cs#CreateClient)]

# [.NET Framework](#tab/netframework)

> [!IMPORTANT]
>
> .NET Framework has limited supported for gRPC over HTTP/2, but it can be enabled by
>
> - Configuring qdrant to use TLS, and you **must** use HTTPS, so you will need to set up
>   [server certificate validation](connecting.md#validating-self-signed-tls-certificates)
> - Referencing [Grpc.Net.Client.Web](https://www.nuget.org/packages/Grpc.Net.Client.Web) and configuring `GrpcWebHandler` as the `HttpHandler`
> - Referencing [System.Net.Http.WinHttpHandler](https://www.nuget.org/packages/System.Net.Http.WinHttpHandler/) 6.0.1 or later, and configuring `WinHttpHandler` as the inner handler for  `GrpcWebHandler`
>
> See [Configure gRPC-Web with the .NET gRPC client](https://learn.microsoft.com/en-au/aspnet/core/grpc/grpcweb?view=aspnetcore-7.0#configure-grpc-web-with-the-net-grpc-client) and use [gRPC client with .NET Standard 2.0](https://learn.microsoft.com/en-au/aspnet/core/grpc/netstandard?view=aspnetcore-7.0#net-framework) for further details.

[!code-csharp[](../../examples/Examples/Client.cs#CreateClientNetFramework)]

---

The client is thread safe, so create a single instance and reuse it.

## Creating a collection

Create a new collection with

[!code-csharp[](../../examples/Examples/Collections.cs#CreateCollection)]

## Indexing

Points are the central entity that Qdrant operates with. A point has a vector and an optional payload.
Points can be indexed with

[!code-csharp[](../../examples/Examples/Points.cs#Upsert)]

The write ordering defines the ordering guarantees of the operation.

## Search

To perform an Approximate Nearest Neighbour (ANN) search 

[!code-csharp[](../../examples/Examples/Points.cs#Search)]

This will return the nearest neighbours up to the limit specified.

## Deleting a collection

Delete an existing collection with

[!code-csharp[](../../examples/Examples/Collections.cs#DeleteCollection)]

The timeout specifies how long to wait for the operation to commit.
