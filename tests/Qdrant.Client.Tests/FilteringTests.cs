using Qdrant.Client.Grpc;
using Xunit;
using static Qdrant.Client.Grpc.Conditions;

namespace Qdrant.Client;

public class FilteringTests
{
	[Fact]
	public void LogicalNot_condition_produces_condition_filter_with_one_must_not_condition()
	{
		var actual = !HasId(1);
		var expected = new Filter
		{
			MustNot =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalNot_condition_doubly_negation_produces_condition_filter_with_one_must_condition()
	{
		var actual = !!HasId(1);
		var expected = new Filter
		{
			Must =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalAnd_conditions_produces_condition_filter_with_two_must_conditions()
	{
		var actual = HasId(1) & MatchKeyword("foo", "hello");
		var expected = new Filter
		{
			Must =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
				new Condition
				{
					Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
				},
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalAnd_with_logicalNot_condition_produces_condition_filter_with_must_and_must_not_conditions()
	{
		var actual = HasId(1) & !MatchKeyword("foo", "hello");
		var expected = new Filter
		{
			Must = { new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } }, },
			MustNot =
			{
				new Condition
				{
					Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
				}
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalAnd_with_logicalNot_conditions_produces_condition_filter_must_not_conditions()
	{
		var actual = !HasId(1) & !MatchKeyword("foo", "hello");
		var expected = new Filter
		{
			MustNot =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
				new Condition
				{
					Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
				}
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalAnd_conditions_with_logicalNot_produces_condition_filter_must_not_conditions()
	{
		var actual = !(HasId(1) & MatchKeyword("foo", "hello"));
		var expected = new Filter
		{
			MustNot =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
				new Condition
				{
					Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
				}
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalOr_conditions_produces_condition_filter_with_two_should_conditions()
	{
		var actual = HasId(1) | MatchKeyword("foo", "hello");
		var expected = new Filter
		{
			Should =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
				new Condition
				{
					Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
				},
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalOr_conditions_with_logicalNot_produces_condition_filter_must_not_condition()
	{
		var actual = !(HasId(1) | MatchKeyword("foo", "hello"));
		var expected = new Filter
		{
			MustNot =
			{
				new Condition
				{
					Filter = new Filter
					{
						Should =
						{
							new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
							new Condition
							{
								Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
							}
						}
					}
				}
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalOr_logicalNot_conditions_produces_condition_filter_must_not_condition()
	{
		var actual = !HasId(1) | !MatchKeyword("foo", "hello");
		var expected = new Filter
		{
			Should =
			{
				new Condition
				{
					Filter = new Filter
					{
						MustNot =
						{
							new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } }
						}
					}
				},
				new Condition
				{
					Filter = new Filter
					{
						MustNot =
						{
							new Condition
							{
								Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
							}
						}
					}
				}
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void LogicalOr_with_condition_or_logicalNot_condition_produces_condition_filter_with_should_conditions()
	{
		var actual = HasId(1) | !MatchKeyword("foo", "hello");
		var expected = new Filter
		{
			Should =
			{
				new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 1 } } } },
				new Condition
				{
					Filter = new Filter
					{
						MustNot =
						{
							new Condition
							{
								Field = new FieldCondition { Key = "foo", Match = new Match { Keyword = "hello" } }
							}
						}
					}
				},
			}
		};

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Complex_condition()
	{
		var actual = ((HasId(1) | HasId(2)) & !MatchKeyword("foo", "hello")) |
					 (HasId(3) & (MatchKeyword("foo", "hello") | !IsNull("foo")));

		var expected =
			new Filter
			{
				Should =
				{
					new Condition
					{
						Filter = new Filter
						{
							Must =
							{
								new Condition
								{
									Filter = new Filter
									{
										Should =
										{
											new Condition
											{
												HasId = new HasIdCondition
												{
													HasId = { new PointId { Num = 1 } }
												}
											},
											new Condition
											{
												HasId = new HasIdCondition
												{
													HasId = { new PointId { Num = 2 } }
												}
											}
										}
									}
								},
								new Condition
								{
									Filter =
										new Filter
										{
											MustNot =
											{
												new Condition
												{
													Field = new FieldCondition
													{
														Key = "foo",
														Match = new Match { Keyword = "hello" }
													}
												}
											}
										}
								}
							}
						}
					},
					new Condition
					{
						Filter = new Filter
						{
							Must =
							{
								new Condition { HasId = new HasIdCondition { HasId = { new PointId { Num = 3 } } } },
								new Condition
								{
									Filter = new Filter
									{
										Should =
										{
											new Condition
											{
												Field = new FieldCondition
												{
													Key = "foo",
													Match =
														new Match { Keyword = "hello" }
												}
											},
											new Condition
											{
												Filter = new Filter
												{
													MustNot =
													{
														new Condition
														{
															IsNull =
																new
																	IsNullCondition
																	{
																		Key =
																			"foo"
																	}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			};

		Assert.Equal(expected, actual);
	}
}
