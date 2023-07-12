# .NET SDK for Qdrant vector database

[![NuGet Release][Qdrant-image]][Qdrant-nuget-url]
[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2Fqdrant%2Fqdrant-dotnet%2Fbadge%3Fref%3Dmain&style=flat)](https://actions-badge.atrox.dev/qdrant/qdrant-dotnet/goto?ref=main)
[![Documentation][Qdrant-image]][Documentation-url]

.NET SDK for [Qdrant vector database](https://qdrant.tech/).

## Getting started

### Installing

```sh
dotnet add package Qdrant.Grpc
```

### Usage

The `QdrantGrpcClient` provides an entry point to interact with all of 
qdrant's gRPC services

```csharp
using Qdrant.Grpc;

public class Program
{
    public static void Main()
    {
        var address = GrpcChannel.ForAddress("http://localhost:6334");
        var client = new QdrantGrpcClient(address);
        
        // check qdrant is healthy
        var healthResponse = client.Qdrant.HealthCheck(new HealthCheckRequest());
        
        // create a collection
        var collectionOperationResponse = client.Collections.Create(new CreateCollection
        {
            CollectionName = "my_collection",
            VectorsConfig = new VectorsConfig
            {
                Params = new VectorParams
                { 
                    Size = 4,
                    Distance = Distance.Cosine
                }
            }
        });
    }
}
```

[Documentation-url]:https://forloop.co.uk/qdrant-dotnet-client/
[Qdrant-image]:
https://img.shields.io/badge/Documentation-blue

[Qdrant-nuget-url]:https://www.nuget.org/packages/Qdrant.Grpc/
[Qdrant-image]:
https://img.shields.io/nuget/v/Qdrant.Grpc.svg