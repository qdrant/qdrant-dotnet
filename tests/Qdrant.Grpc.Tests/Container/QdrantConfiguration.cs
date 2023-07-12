using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Qdrant.Grpc.Tests.Container;

public sealed class QdrantConfiguration : ContainerConfiguration
{
	/// <summary>
	/// Initializes a new instance of the <see cref="QdrantConfiguration" /> class.
	/// </summary>
	public QdrantConfiguration()
	{
	}

	/// <summary>
    /// Initializes a new instance of the <see cref="QdrantConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public QdrantConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public QdrantConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public QdrantConfiguration(QdrantConfiguration resourceConfiguration)
        : this(new QdrantConfiguration(), resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public QdrantConfiguration(QdrantConfiguration oldValue, QdrantConfiguration newValue)
	    : base(oldValue, newValue)
    {
    }
}
