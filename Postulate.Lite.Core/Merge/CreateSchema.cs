using System.Collections.Generic;
using System.Data;

namespace Postulate.Lite.Core.Merge
{
	public class CreateSchema : Action
	{
		public CreateSchema(string schema) : base(ObjectType.Schema, ActionType.Create)
		{
			SchemaName = schema;
		}

		public string SchemaName { get; private set; }

		public override IEnumerable<string> SqlCommands<TKey>(CommandProvider<TKey> commandProvider, IDbConnection connection)
		{
			if (commandProvider.SupportsSchemas && !commandProvider.SchemaExists(connection, SchemaName))
			{
				yield return commandProvider.CreateSchemaCommand(SchemaName);
			}
		}
	}
}