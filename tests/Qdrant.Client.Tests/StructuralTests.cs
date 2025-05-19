using Qdrant.Client.Grpc;
using Xunit;

namespace Qdrant.Client;

public class StructuralTests
{
	[Fact]
	public void All_PayloadSchemaTypes_map_to_FieldType()
	{
		var schemaTypes =
			((PayloadSchemaType[])Enum.GetValues(typeof(PayloadSchemaType)))
			.Except([PayloadSchemaType.UnknownType]);

		foreach (var schemaType in schemaTypes)
		{
			var ex = Record.Exception(() =>
			{
				var fieldType = QdrantClient.FieldTypeFromPayloadSchemaType(schemaType);
			});

			Assert.Null(ex);
		}
	}
}
