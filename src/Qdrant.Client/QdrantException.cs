namespace Qdrant.Client;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class QdrantException : Exception
{
	public QdrantException(string message)
		: base(message)
	{
	}
}
