using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Qdrant.Grpc.Tests;

public class VersionTests
{
	[Fact]
	public void VersionMatchesBuildVersion()
	{
		var buildXml = XDocument.Load(Path.Combine(SolutionPaths.Root, "Directory.Build.props"));
		var version = buildXml.Descendants(XName.Get("QdrantVersion", "http://schemas.microsoft.com/developer/msbuild/2003")).First().Value;
		QdrantGrpcClient.QdrantVersion.Should().Be(version);
	}
}
