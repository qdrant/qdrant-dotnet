using Grpc.Core;

namespace Qdrant.Grpc;

/// <summary>
/// Call invoker that throws <c>NotImplementedException</c> for all requests.
/// </summary>
internal class UnimplementedCallInvoker : CallInvoker
{
	/// <inheritdoc />
	public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options) =>
		throw new NotImplementedException();

	/// <inheritdoc />
	public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options) =>
		throw new NotImplementedException();
}
