namespace Qdrant.Client;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// The exception that is thrown when a requested method or operation related with Qdrant fails during execution.
/// </summary>
public class QdrantException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="QdrantException"/> class with a specified error message.
	/// </summary>
	/// <param name="message">Message that describes the error.</param>
	public QdrantException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QdrantException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">Message that describes the error.</param>
	/// <param name="innerException">The exception that is the cause of the current exception. If the inner parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
	public QdrantException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
