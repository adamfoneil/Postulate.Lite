using Postulate.Lite.Core;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Postulate.Lite.MySql
{
	public partial class MySqlProvider<TKey> : CommandProvider<TKey>
	{
		public override string CommentPrefix => "# ";
		public override bool SupportsSchemas => false;
		public override string DefaultSchema => throw new NotImplementedException();

		public override string CreateTableCommand(Type modelType)
		{
			var columns = _integrator.GetMappedColumns(modelType);
			var pkColumns = GetPrimaryKeyColumns(modelType, columns, out bool identityIsPrimaryKey);
			var identityName = modelType.GetIdentityName();

			List<string> members = new List<string>();
			members.AddRange(columns.Select(pi => SqlColumnSyntax(pi, (identityName.Equals(pi.Name)))));
			members.Add(PrimaryKeySyntax(pkColumns));
			if (!identityIsPrimaryKey) members.Add(UniqueIdSyntax(modelType.GetIdentityProperty()));

			return
				$"CREATE TABLE {ApplyDelimiter(_integrator.GetTableName(modelType))} (" +
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

		public override string DropColumnCommand(ColumnInfo columnInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropForeignKeyCommand(ForeignKeyInfo foreignKeyInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string DropTableCommand(TableInfo tableInfo)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ForeignKeyInfo> GetDependentForeignKeys(IDbConnection connection, TableInfo tableInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddForeignKeyCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddForeignKeyCommand(ForeignKeyInfo foreignkeyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string AlterColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string CreateSchemaCommand(string schemaName)
		{
			return $"CREATE SCHEMA `{schemaName}`";
		}

		public override bool SchemaExists(IDbConnection connection, string schemaName)
		{
			return connection.Exists("`information_schema`.`schemata` WHERE `schema_name`=@schema", new { schema = schemaName });
		}

		public override void MapProviderSpecificInfo(PropertyInfo pi, ColumnInfo col)
		{
			throw new NotImplementedException();
		}

		public override Task<IEnumerable<ColumnInfo>> GetSchemaColumnsAsync(IDbConnection connection, string[] excludeSchemas)
		{
			throw new NotImplementedException();
		}

		protected override string SchemaCriteria(string[] excludeSchemas)
		{
			throw new NotImplementedException();
		}

		public override Task<IEnumerable<TableInfo>> GetSchemaTablesAsync(IDbConnection connection, string[] excludeSchemas)
		{
			throw new NotImplementedException();
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