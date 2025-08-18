#!/bin/bash

# Qdrant .NET Client Example - Build Script
# This script builds the example application Docker image
# Assumes Qdrant is already running on localhost:6334

set -e

echo "📦 Building Docker image..."
docker build -t qdrant-dotnet-example .


docker run --rm -it --network host qdrant-dotnet-example
