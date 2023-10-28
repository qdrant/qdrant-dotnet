namespace Qdrant.Client.Grpc;

/// <summary>
/// A filter condition
/// </summary>
public partial class Condition
{
	/// <summary>
	/// Combines two <see cref="Condition"/> together in an AND conjunction.
	/// </summary>
	/// <param name="left">The left condition</param>
	/// <param name="right">The right condition</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition operator &(Condition left, Condition right)
	{
		var leftIsFilter = left.ConditionOneOfCase == ConditionOneOfOneofCase.Filter;
		var rightIsFilter = right.ConditionOneOfCase == ConditionOneOfOneofCase.Filter;

		if (leftIsFilter && rightIsFilter)
			return new Condition { Filter = left.Filter & right.Filter };

		if (leftIsFilter && left.Filter.Should.Count == 0)
		{
			var condition = left.Clone();
			condition.Filter.Must.Add(right);
			return condition;
		}

		if (rightIsFilter && right.Filter.Should.Count == 0)
		{
			var condition = right.Clone();
			condition.Filter.Must.Add(left);
			return condition;
		}

		return new Condition { Filter = new() { Must = { left, right } } };
	}

	/// <summary>
	/// Combines two <see cref="Condition"/> together in an OR disjunction.
	/// </summary>
	/// <param name="left">The left condition</param>
	/// <param name="right">The right condition</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition operator |(Condition left, Condition right) =>
		new() { Filter = new() { Should = { left, right } } };

	/// <summary>
	/// Negates a <see cref="Condition"/>.
	/// </summary>
	/// <param name="condition">The condition to negate</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition operator !(Condition condition) =>
		condition.ConditionOneOfCase switch
		{
			// if the condition is a filter and contains only must conditions, move the
			// conditions to must not conditions
			ConditionOneOfOneofCase.Filter when
				condition.Filter.Should.Count == 0 && condition.Filter.MustNot.Count == 0 =>
				new Condition { Filter = new() { MustNot = { condition.Filter.Must } } },
			// if the condition is a filter and contains only must not conditions, move the
			// conditions to must conditions
			ConditionOneOfOneofCase.Filter when
				condition.Filter.Should.Count == 0 && condition.Filter.Must.Count == 0 =>
				new Condition { Filter = new() { Must = { condition.Filter.MustNot } } },
			_ => new Condition { Filter = new() { MustNot = { condition } } }
		};
}
