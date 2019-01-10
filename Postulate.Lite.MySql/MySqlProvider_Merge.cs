using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Postulate.Lite.MySql
{
	public partial class MySqlProvider<TKey> : CommandProvider<TKey>
	{
		public override string CommentPrefix => "# ";
		public override bool SupportsSchemas => false;
		public override string DefaultSchema => throw new NotImplementedException();

		public override string CreateTableCommand(Type modelType, string tableName = null)
		{
			var columns = _integrator.GetMappedColumns(modelType);
			var pkColumns = GetPrimaryKeyColumns(modelType, columns, out bool identityIsPrimaryKey);
			var identityName = modelType.GetIdentityName();

			List<string> members = new List<string>();
			members.AddRange(columns.Select(pi => SqlColumnSyntax(pi, (identityName.Equals(pi.Name)))));
			members.Add(PrimaryKeySyntax(pkColumns));
			if (!identityIsPrimaryKey) members.Add(UniqueIdSyntax(modelType.GetIdentityProperty()));

			string name = tableName ?? _integrator.GetTableName(modelType);

			return
				$"CREATE TABLE {ApplyDelimiter(name)} (" +
					string.Join(",\r\n\t", members) +
				")";
		}

		private string UniqueIdSyntax(PropertyInfo propertyInfo)
		{
			return $"UNIQUE ({string.Join(", ", ApplyDelimiter(propertyInfo.GetColumnName()))})";
		}

		private string PrimaryKeySyntax(IEnumerable<PropertyInfo> pkColumns)
		{
			return $"PRIMARY KEY ({string.Join(", ", pkColumns.Select(pi => ApplyDelimiter(pi.GetColumnName())))})";
		}

		public override string CreateSchemaCommand(string schemaName)
		{
			return $"CREATE SCHEMA `{schemaName}`";
		}

		public override bool SchemaExists(IDbConnection connection, string schemaName)
		{
			return connection.Exists("`information_schema`.`schemata` WHERE `schema_name`=@schema", new { schema = schemaName });
		}

		protected override string CreateTableScript(TableInfo table, Type modelType)
		{
			return CreateTableCommandInner(modelType, table.ToString(), requireIdentity: false);
		}

		protected override bool TableExists(IDbConnection connection, TableInfo table)
		{
			return connection.Exists("`information_schema`.`tables` WHERE `table_name`=@table AND `table_schema`=@schema", new { table = table.Name, schema = table.Schema });
		}

		protected override string PrimaryKeySyntax(string constraintName, IEnumerable<PropertyInfo> pkColumns)
		{
			return $"PRIMARY KEY ({string.Join(", ", pkColumns.Select(pi => ApplyDelimiter(pi.GetColumnName())))})";
		}

		protected override string UniqueIdSyntax(string constraintName, PropertyInfo identityProperty)
		{
			return $"UNIQUE ({ApplyDelimiter(identityProperty.GetColumnName())}";
		}
	}
}