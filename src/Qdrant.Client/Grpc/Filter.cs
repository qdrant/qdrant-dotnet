using static Qdrant.Client.Grpc.Condition;
using static Qdrant.Client.Grpc.Conditions;

namespace Qdrant.Client.Grpc;

/// <summary>
/// Filters points to those that match conditions.
/// </summary>
public partial class Filter
{
	/// <summary>
	/// Implicitly converts a <see cref="Condition"/> to a new instance of <see cref="Filter"/>
	/// </summary>
	/// <param name="condition">the condition</param>
	/// <returns>a new instance of <see cref="Filter"/></returns>
	public static implicit operator Filter(Condition condition) =>
		// if the condition is already a filter, unwrap it
		condition.ConditionOneOfCase == ConditionOneOfOneofCase.Filter
			? condition.Filter
			: new Filter { Must = { condition } };

	/// <summary>
	/// Evaluates whether the given <see cref="Filter"/> is <c>false</c>
	/// </summary>
	/// <param name="_">The filter.</param>
	/// <returns>Always returns <c>false</c> so that both operands in boolean logical operators are evaluated.</returns>
	public static bool operator false(Filter _) => false;

	/// <summary>
	/// Evaluates whether the given <see cref="Filter"/> is <c>true</c>
	/// </summary>
	/// <param name="_">The filter.</param>
	/// <returns>Always returns <c>false</c> so that both operands in boolean logical operators are evaluated.</returns>
	public static bool operator true(Filter _) => false;

	/// <summary>
	/// Combines two <see cref="Filter"/> together in a conjunction.
	/// </summary>
	/// <param name="left">The left filter</param>
	/// <param name="right">The right filter</param>
	/// <returns>a new instance of <see cref="Filter"/></returns>
	public static Filter operator &(Filter left, Filter right)
	{
		if (left.Should.Count == 0 && right.Should.Count == 0)
			return new Filter { Must = { left.Must, right.Must }, MustNot = { left.MustNot, right.MustNot } };

		return new Filter { Must = { Filter(left), Filter(right) } };
	}

	/// <summary>
	/// Combines two <see cref="Filter"/> together in a disjunction.
	/// </summary>
	/// <param name="left">The left filter</param>
	/// <param name="right">The right filter</param>
	/// <returns>a new instance of <see cref="Filter"/></returns>
	public static Filter operator |(Filter left, Filter right) =>
		new() { Should = { Filter(left), Filter(right) } };
}
