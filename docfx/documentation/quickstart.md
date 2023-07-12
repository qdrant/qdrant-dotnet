# Getting started

## Installing

The [gRPC client is available on nuget](https://www.nuget.org/packages/Qdrant.Grpc), and can be installed with

```sh
dotnet add package Qdrant.Grpc
```

## Creating the client

Create a new instance of the client with

[!code-csharp[](../../examples/Examples/Client.cs#CreateClient)]

The client is thread safe, so create a single instance and reuse it.

## Creating a collection

Create a new collection with

[!code-csharp[](../../examples/Examples/Collections.cs#CreateCollection)]

## Indexing

Points are the central entity that Qdrant operates with. A point has a vector and an optional payload.
Points can be indexed with

[!code-csharp[](../../examples/Examples/Points.cs#Upsert)]

The write ordering

## Search

To perform an Approximate Nearest Neighbour (ANN) search 

[!code-csharp[](../../examples/Examples/Points.cs#Search)]

This will return the nearest neighbours up to the limit specified.

## Deleting a collection

Delete an existing collection with

[!code-csharp[](../../examples/Examples/Collections.cs#DeleteCollection)]

The timeout specifies how long to wait for the operation to commit.
