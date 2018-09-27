using Dapper;
using Postulate.Lite.Core;
using Postulate.Lite.Core.Attributes;
using Postulate.Lite.Core.Extensions;
using Postulate.Lite.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

		public override void MapProviderSpecificInfo(PropertyInfo pi, ColumnInfo col)
		{
			var fkAttr = pi.GetCustomAttribute<ReferencesAttribute>();
			if (fkAttr != null)
			{
				Type referencedType = fkAttr.PrimaryType;
				TableInfo tbl = _integrator.GetTableInfo(referencedType);
				col.ForeignKeyInfo = new ForeignKeyInfo();
				col.ReferencedSchema = tbl.Schema;
				col.ReferencedTable = tbl.Name;
				col.ReferencedColumn = referencedType.GetIdentityName();
			}

			col.TableInfo = _integrator.GetTableInfo(pi.DeclaringType);
		}

		public override bool IsTableEmpty(IDbConnection connection, Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string CreateTableCommand(Type modelType)
		{
			var columns = _integrator.GetMappedColumns(modelType);
			var pkColumns = GetPrimaryKeyColumns(modelType, columns, out bool identityIsPrimaryKey);
			var identityName = modelType.GetIdentityName();

			string constraintName = _integrator.GetTableName(modelType).Replace(".", "_");

			List<string> members = new List<string>();
			members.AddRange(columns.Select(pi => SqlColumnSyntax(pi, (identityName.Equals(pi.Name)))));
			members.Add(PrimaryKeySyntax(constraintName, pkColumns));
			if (!identityIsPrimaryKey) members.Add(UniqueIdSyntax(constraintName, modelType.GetIdentityProperty()));

			return
				$"CREATE TABLE {ApplyDelimiter(_integrator.GetTableName(modelType))} (" +
					string.Join(",\r\n\t", members) +
				")";
		}

		public override string AddForeignKeyCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AddColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string AlterColumnCommand(PropertyInfo propertyInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropColumnCommand(ColumnInfo columnInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropForeignKeyCommand(ForeignKeyInfo foreignKeyInfo)
		{
			return $"ALTER TABLE [{foreignKeyInfo.Child.Schema}].[{foreignKeyInfo.Child.TableName}] DROP CONSTRAINT [{foreignKeyInfo.ConstraintName}]";
		}

		public override string DropTableCommand(TableInfo tableInfo)
		{
			throw new NotImplementedException();
		}

		public override string DropPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string AddPrimaryKeyCommand(Type modelType)
		{
			throw new NotImplementedException();
		}

		public override string AddForeignKeyCommand(ForeignKeyInfo foreignkeyInfo)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<ForeignKeyInfo> GetDependentForeignKeys(IDbConnection connection, TableInfo tableInfo)
		{
			return connection.Query<ForeignKeyData>(
				@"SELECT
                    [fk].[name] AS [ConstraintName],
                    SCHEMA_NAME([parent].[schema_id]) AS [ReferencedSchema],
                    [parent].[name] AS [ReferencedTable],
                    [refdcol].[name] AS [ReferencedColumn],
                    SCHEMA_NAME([child].[schema_id]) AS [ReferencingSchema],
                    [child].[name] AS [ReferencingTable],
                    [rfincol].[name] AS [ReferencingColumn],
					CONVERT(bit, CASE [fk].[delete_referential_action] WHEN 1 THEN 1 ELSE 0 END) AS [CascadeDelete]
                FROM
                    [sys].[foreign_keys] [fk] INNER JOIN [sys].[tables] [child] ON [fk].[parent_object_id]=[child].[object_id]
                    INNER JOIN [sys].[tables] [parent] ON [fk].[referenced_object_id]=[parent].[object_id]
                    INNER JOIN [sys].[foreign_key_columns] [fkcol] ON
                        [fk].[parent_object_id]=[fkcol].[parent_object_id] AND
                        [fk].[object_id]=[fkcol].[constraint_object_id]
                    INNER JOIN [sys].[columns] [refdcol] ON
                        [fkcol].[referenced_column_id]=[refdcol].[column_id] AND
                        [fkcol].[referenced_object_id]=[refdcol].[object_id]
                    INNER JOIN [sys].[columns] [rfincol] ON
                        [fkcol].[parent_column_id]=[rfincol].[column_id] AND
                        [fkcol].[parent_object_id]=[rfincol].[object_id]
				WHERE
                    [fk].[referenced_object_id]=OBJECT_ID(@schema+'.' + @table)", new { schema = tableInfo.Schema, table = tableInfo.Name })
			.Select(fk => new ForeignKeyInfo()
			{
				ConstraintName = fk.ConstraintName,
				Child = new ColumnInfo() { Schema = fk.ReferencingSchema, TableName = fk.ReferencingTable, ColumnName = fk.ReferencingColumn },
				Parent = new ColumnInfo() { Schema = fk.ReferencedSchema, TableName = fk.ReferencedTable, ColumnName = fk.ReferencedColumn },
				CascadeDelete = fk.CascadeDelete
			});
		}

		public override async Task<IEnumerable<ColumnInfo>> GetSchemaColumnsAsync(IDbConnection connection, string[] excludeSchemas)
		{
			return await connection.QueryAsync<ColumnInfo>(
				$@"SELECT
					SCHEMA_NAME([t].[schema_id]) AS [Schema],
					[t].[name] AS [TableName], [c].[Name] AS [ColumnName],
					TYPE_NAME([c].[system_type_id]) AS [DataType],
					[c].[max_length] AS [ByteLength],
					[c].[is_nullable] AS [IsNullable],
					[c].[precision] AS [Precision],
					[c].[scale] as [Scale], [c].[collation_name] AS [Collation],
					[c].[is_computed] AS [IsCalculated],
					SCHEMA_NAME([parentTbl].[schema_id]) AS [ReferencedSchema],
					[parentTbl].[name] AS [ReferencedTable],
					[parentCol].[name] AS [ReferencedColumn],
					[fk].[name] AS [ForeignKeyConstraint],
					CONVERT(bit, CASE [fk].[delete_referential_action] WHEN 1 THEN 1 ELSE 0 END) AS [CascadeDelete],
					[ccol].[definition] AS [Expression]
				FROM
					[sys].[tables] [t] INNER JOIN [sys].[columns] [c] ON [t].[object_id]=[c].[object_id]
					LEFT JOIN [sys].[foreign_key_columns] [fkcol] ON
						[c].[object_id]=[fkcol].[parent_object_id] AND
						[c].[column_id]=[fkcol].[parent_column_id]
					LEFT JOIN [sys].[foreign_keys] [fk] ON [fkcol].[constraint_object_id]=[fk].[object_id]
					LEFT JOIN [sys].[computed_columns] [ccol] ON [c].[object_id]=[ccol].[object_id] AND [c].[name]=[ccol].[name]
					LEFT JOIN [sys].[columns] [parentCol] ON
						[fkcol].[referenced_object_id]=[parentCol].[object_id] AND
						[fkcol].[referenced_column_id]=[parentCol].[column_id]
					LEFT JOIN [sys].[tables] [parentTbl] ON
						[parentCol].[object_id]=[parentTbl].[object_id]
					WHERE
						([t].[name] NOT LIKE 'AspNet%' OR [t].[name] LIKE 'AspNetUsers') AND
						[t].[name] NOT LIKE '__MigrationHistory'{SchemaCriteria(excludeSchemas)}");
		}

		protected override string SchemaCriteria(string[] excludeSchemas)
		{
			string schemaCriteria = string.Empty;
			if (excludeSchemas?.Any() ?? false)
			{
				schemaCriteria = $" AND SCHEMA_NAME([t].[schema_id]) NOT IN ({string.Join(", ", excludeSchemas.Select(s => $"'{s}'"))})";
			}

			return schemaCriteria;
		}

		public override async Task<IEnumerable<TableInfo>> GetSchemaTablesAsync(IDbConnection connection, string[] excludeSchemas)
		{
			// row count trick thanks to https://blogs.msdn.microsoft.com/martijnh/2010/07/15/sql-serverhow-to-quickly-retrieve-accurate-row-count-for-table/

			return await connection.QueryAsync<TableInfo>(
				$@"SELECT
                    SCHEMA_NAME([t].[schema_id]) AS [Schema], [t].[name] AS [Name],
					(SELECT SUM(row_count) FROM sys.dm_db_partition_stats WHERE object_id=[t].[object_id] AND (index_id=0 or index_id=1)) AS [RowCount]
                FROM
                    [sys].[tables] [t]
                WHERE
                    [name] NOT LIKE 'AspNet%' AND
                    [name] NOT LIKE '__MigrationHistory'{SchemaCriteria(excludeSchemas)}");
		}

		public override string AddColumnCommand(ColumnInfo columnInfo)
		{
			throw new NotImplementedException();
		}
	}
}