namespace Qdrant.Client.Grpc;

/// <summary>
/// Methods to simplify building <see cref="Condition"/>
/// </summary>
public static class Conditions
{
	/// <summary>
	/// Match all records with the provided ids
	/// </summary>
	/// <param name="ids">The ids to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition HasId(IReadOnlyList<Guid> ids)
	{
		var pointIds = ids.Select(id => new PointId { Uuid = id.ToString() });
		return new Condition { HasId = new HasIdCondition { HasId = { pointIds } } };
	}

	/// <summary>
	/// Match all records with the provided ids
	/// </summary>
	/// <param name="ids">The ids to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition HasId(IReadOnlyList<ulong> ids)
	{
		var pointIds = ids.Select(id => new PointId { Num = id });
		return new Condition { HasId = new HasIdCondition { HasId = { pointIds } } };
	}

	/// <summary>
	/// Match all records with the provided id
	/// </summary>
	/// <param name="id">The id to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition HasId(Guid id) =>
		new() { HasId = new HasIdCondition { HasId = { id } } };

	/// <summary>
	/// Match all records with the provided id
	/// </summary>
	/// <param name="id">The id to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition HasId(ulong id) =>
		new() { HasId = new HasIdCondition { HasId = { id } } };

	/// <summary>
	/// Match all records where the given field either does not exist, or has null or empty value.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition IsEmpty(string field) =>
		new() { IsEmpty = new IsEmptyCondition { Key = field } };

	/// <summary>
	/// Match all records where the given field is null.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition IsNull(string field) =>
		new() { IsNull = new IsNullCondition { Key = field } };

	/// <summary>
	/// Match records where the given field matches the given keyword
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="keyword">The keyword to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition MatchKeyword(string field, string keyword) =>
		new() { Field = new FieldCondition { Key = field, Match = new Match { Keyword = keyword } } };

	/// <summary>
	/// Match records where the given field matches the given text
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="text">The text to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition MatchText(string field, string text) =>
		new() { Field = new FieldCondition { Key = field, Match = new Match { Text = text } } };

	/// <summary>
	/// Match records where the given field matches the given boolean value.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="value">The value to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition Match(string field, bool value) =>
		new() { Field = new FieldCondition { Key = field, Match = new Match { Boolean = value } } };

	/// <summary>
	/// Match records where the given field matches the given numeric value.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="value">The value to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition Match(string field, long value) =>
		new() { Field = new FieldCondition { Key = field, Match = new Match { Integer = value } } };

	/// <summary>
	/// Match records where the given field matches any of the given keywords
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="keywords">The keywords to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition Match(string field, IReadOnlyList<string> keywords)
	{
		var repeatedStrings = new RepeatedStrings { Strings = { keywords } };
		return new Condition { Field = new FieldCondition { Key = field, Match = new Match { Keywords = repeatedStrings } } };
	}

	/// <summary>
	/// Match records where the given field matches any of the given values
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="values">The values to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition Match(string field, IReadOnlyList<long> values)
	{
		var repeatedIntegers = new RepeatedIntegers { Integers = { values } };
		return new Condition { Field = new FieldCondition { Key = field, Match = new Match { Integers = repeatedIntegers } } };
	}

	/// <summary>
	/// Match records where the given field does not match any of the given keywords
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="keywords">The keywords not to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition MatchExcept(string field, IReadOnlyList<string> keywords)
	{
		var repeatedStrings = new RepeatedStrings { Strings = { keywords } };
		return new Condition { Field = new FieldCondition { Key = field, Match = new Match { ExceptKeywords = repeatedStrings } } };
	}

	/// <summary>
	/// Match records where the given field does not match any of the given values
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="values">The values not to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition MatchExcept(string field, IReadOnlyList<long> values)
	{
		var repeatedIntegers = new RepeatedIntegers { Integers = { values } };
		return new Condition { Field = new FieldCondition { Key = field, Match = new Match { ExceptIntegers = repeatedIntegers } } };
	}

	/// <summary>
	/// Match records where the given nested field matches the given condition.
	/// </summary>
	/// <param name="field">The name of the nested field</param>
	/// <param name="condition">The condition to match</param>
	/// <returns>A new instance of <see cref="Condition"/></returns>
	public static Condition Nested(string field, Condition condition) =>
		new() { Nested = new NestedCondition { Key = field, Filter = new Filter { Must = { condition } } } };

	/// <summary>
	/// Match records where the given nested field matches the given filter.
	/// </summary>
	/// <param name="field">The name of the nested field</param>
	/// <param name="filter">The filter to match</param>
	/// <returns>A new instance of <see cref="Condition"/></returns>
	public static Condition Nested(string field, Filter filter) =>
		new() { Nested = new NestedCondition { Key = field, Filter = filter } };

	/// <summary>
	/// Match records where the given field matches the given range.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="range">The range to match</param>
	/// <returns>A new instance of <see cref="Condition"/></returns>
	public static Condition Range(string field, Range range) =>
		new() { Field = new FieldCondition { Key = field, Range = range } };

	/// <summary>
	/// Match records where the given field has values inside a circle centred at a given latitude and
	/// longitude with a given radius.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="latitude">The latitude of the center</param>
	/// <param name="longitude">The longitude of the center</param>
	/// <param name="radius">The radius in meters</param>
	/// <returns>A new instance of <see cref="Condition"/></returns>
	public static Condition GeoRadius(string field, double latitude, double longitude, float radius) =>
		new()
		{
			Field = new FieldCondition
			{
				Key = field,
				GeoRadius = new GeoRadius
				{
					Center = new GeoPoint { Lat = latitude, Lon = longitude },
					Radius = radius
				}
			}
		};

	/// <summary>
	/// Match records where the given field has values inside a bounding box specified by the top left and
	/// bottom right coordinates.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="topLeftLatitude">The latitude of the top left point</param>
	/// <param name="topLeftLongitude">The longitude of the top left point</param>
	/// <param name="bottomRightLatitude">The latitude of the bottom right point</param>
	/// <param name="bottomRightLongitude">The longitude of the bottom right point</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition GeoBoundingBox(string field, double topLeftLatitude, double topLeftLongitude,
		double bottomRightLatitude, double bottomRightLongitude) =>
		new()
		{
			Field = new FieldCondition
			{
				Key = field,
				GeoBoundingBox = new GeoBoundingBox
				{
					TopLeft = new GeoPoint { Lat = topLeftLatitude, Lon = topLeftLongitude },
					BottomRight = new GeoPoint { Lat = bottomRightLatitude, Lon = bottomRightLongitude }
				}
			}
		};

	/// <summary>
	/// Matches records where the given field has values inside the provided polygon. A polygon always has an exterior
	/// ring and may optionally have interior rings, which represent independent areas or holes.
	/// When defining a ring, you must pick either a clockwise or counterclockwise ordering for your points; it is
	/// convention to orient the exterior ring clockwise, and interior rings counterclockwise.
	/// The first and last point of the polygon must be the same.
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="exterior">The exterior ring of the polygon</param>
	/// <param name="interiors">The interior rings of the polygon</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition GeoPolygon(string field, GeoLineString exterior, IReadOnlyList<GeoLineString>? interiors = null)
	{
		var geoPolygon = new GeoPolygon { Exterior = exterior };

		if (interiors is not null)
			geoPolygon.Interiors.AddRange(interiors);

		return new() { Field = new FieldCondition { Key = field, GeoPolygon = geoPolygon } };
	}

	/// <summary>
	/// Matches records where the given field has a count of values within the specified count range
	/// </summary>
	/// <param name="field">The name of the field</param>
	/// <param name="valuesCount">The count range to match</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition ValuesCount(string field, ValuesCount valuesCount) =>
		new() { Field = new FieldCondition { Key = field, ValuesCount = valuesCount } };

	/// <summary>
	/// Nests a filter
	/// </summary>
	/// <param name="filter">The filter to nest</param>
	/// <returns>a new instance of <see cref="Condition"/></returns>
	public static Condition Filter(Filter filter) =>
		new() { Filter = filter };
}


