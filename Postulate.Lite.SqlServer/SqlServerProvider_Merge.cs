using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using System;
using System.Data;

namespace Postulate.Lite.SqlServer
{
	public partial class SqlServerProvider<TKey> : CommandProvider<TKey>
	{
		public override string CommentPrefix => "-- ";
		public override bool SupportsSchemas => true;
		public override string DefaultSchema => "dbo";

		public override string CreateSchemaCommand(string schemaName)
		{
			return $"CREATE SCHEMA [{schemaName}]";
		}

		public override bool SchemaExists(IDbConnection connection, string schemaName)
		{
			return connection.Exists("[sys].[schemas] WHERE [name]=@name", new { name = schemaName });
		}

		public override string CreateTableCommand(Type modelType)
		{
			string tableName = _integrator.GetTableName(modelType);
			return CreateTableCommandInner(modelType, tableName);
		}
	}
}