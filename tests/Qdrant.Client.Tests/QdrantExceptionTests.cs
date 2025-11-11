using FluentAssertions;
using Xunit;

namespace Qdrant.Client;

public class QdrantExceptionTests
{
	[Fact]
	public void ShouldCreateQdrantException()
	{
		var exceptionMessage = "Test error message";
		var subject = new QdrantException(exceptionMessage);

		subject.Should().NotBeNull();
		subject.Message.Should().Be(exceptionMessage);
		subject.InnerException.Should().BeNull();
	}

	[Fact]
	public void ShouldCreateQdrantExceptionWithInnerException()
	{
		var exceptionMessage = "Test error message";
		var innerException = new InvalidOperationException("Inner exception message");
		var subject = new QdrantException(exceptionMessage, innerException);

		subject.Should().NotBeNull();
		subject.Message.Should().Be(exceptionMessage);
		subject.InnerException.Should().Be(innerException);
	}
}
