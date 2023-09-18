using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;

namespace Qdrant.Client.Tests.Container;

public class QdrantContainer : DockerContainer
{
	public QdrantContainer(QdrantConfiguration configuration, ILogger logger) : base(configuration, logger)
	{
	}
}
