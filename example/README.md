# Qdrant .NET Client Example

This example demonstrates how to use the Qdrant .NET client to connect to a Qdrant instance and create a collection.

## What This Example Does

Minimal example of instanciating Qdrant client and running a simple request

## Prerequisites

- Docker installed (for building the example application)
- Qdrant instance running on localhost:6334
- .NET 6.0 SDK (for local development)

## Quick Start

### 1. Start Qdrant

First, make sure Qdrant is running. You can start it with Docker:

```bash
docker run --rm -it -p 6334:5334 -p 6333:6333 qdrant/qdrant
```


### 2. Building and Running application


```bash
bash -x build-and-run.sh
```

