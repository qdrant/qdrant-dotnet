namespace Qdrant.Client;

/// <summary>
/// Utilities for attaching per-request headers to gRPC calls.
/// </summary>
public static class RequestHeaders
{
	private static readonly AsyncLocal<IReadOnlyDictionary<string, string>?> _headers = new();

	internal static IReadOnlyDictionary<string, string>? Current => _headers.Value;

	/// <summary>
	/// Sets key and value as metadata on requests made within the returned scope.
	/// </summary>
	public static IDisposable Use(string key, string value) =>
		Use(new Dictionary<string, string> { [key] = value });

	/// <summary>
	/// Sets all entries of headers as metadata on requests made within the returned scope.
	/// </summary>
	public static IDisposable Use(IDictionary<string, string> headers)
	{
		var previous = _headers.Value;
		var merged = new Dictionary<string, string>();
		if (previous is not null)
			foreach (var header in previous)
				merged[header.Key] = header.Value;
		foreach (var header in headers)
			merged[header.Key] = header.Value;
		_headers.Value = merged;
		return new Scope(() => _headers.Value = previous);
	}

	private sealed class Scope : IDisposable
	{
		private readonly Action _restore;

		internal Scope(Action restore) => _restore = restore;

		public void Dispose() => _restore();
	}
}
